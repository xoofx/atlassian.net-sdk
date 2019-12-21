using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueOperationsTest
    {
        private readonly Random _random = new Random();

        [Theory]
        [ClassData(typeof(JiraProvider))]
        async Task AssignIssue(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Type = "1";
            issue.Summary = "Test issue to assign" + _random.Next(int.MaxValue);

            issue.SaveChanges();
            Assert.Equal("admin", issue.Assignee);

            await issue.AssignAsync("test");
            Assert.Equal("test", issue.Assignee);

            issue = await jira.Issues.GetIssueAsync(issue.Key.Value);
            Assert.Equal("test", issue.Assignee);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        void GetChangeLogsForIssue(Jira jira)
        {
            var changelogs = jira.Issues.GetIssueAsync("TST-1").Result.GetChangeLogsAsync().Result.OrderBy(log => log.CreatedDate);
            Assert.True(changelogs.Count() >= 4);

            var firstChangeLog = changelogs.First();
            Assert.Equal("admin", firstChangeLog.Author.Username);
            //Assert.NotNull(firstChangeLog.CreatedDate); this can never be null
            Assert.Equal(2, firstChangeLog.Items.Count());

            var firstItem = firstChangeLog.Items.First();
            Assert.Equal("Attachment", firstItem.FieldName);
            Assert.Equal("jira", firstItem.FieldType);
            Assert.Null(firstItem.FromValue);
            Assert.Null(firstItem.FromId);
            Assert.NotNull(firstItem.ToId);
            Assert.Equal("SampleImage.png", firstItem.ToValue);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        void AddAndRemoveWatchersToIssue(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Type = "1";
            issue.Summary = "Test issue with watchers" + _random.Next(int.MaxValue);
            issue.SaveChanges();

            issue.AddWatcherAsync("test").Wait();
            Assert.Equal(2, issue.GetWatchersAsync().Result.Count());

            issue.DeleteWatcherAsync("admin").Wait();
            Assert.Single(issue.GetWatchersAsync().Result);

            var user = issue.GetWatchersAsync().Result.First();
            Assert.Equal("test", user.Username);
            Assert.True(user.IsActive);
            Assert.Equal("Tester", user.DisplayName);
            Assert.Equal("test@qa.com", user.Email);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        async Task AddAndRemoveWatchersToIssueWithEmailAsUsername(Jira jira)
        {
            // Create issue.
            var issue = jira.CreateIssue("TST");
            issue.Type = "1";
            issue.Summary = "Test issue with watchers" + _random.Next(int.MaxValue);
            issue.SaveChanges();

            // Create user with e-mail as username.
            var rand = _random.Next(int.MaxValue);
            var userInfo = new JiraUserCreationInfo()
            {
                Username = $"test{rand}@user.com",
                DisplayName = $"Test User {rand}",
                Email = $"test{rand}@user.com",
                Password = $"MyPass{rand}",
            };

            await jira.Users.CreateUserAsync(userInfo);

            // Add the user as a watcher on the issue.
            await issue.AddWatcherAsync(userInfo.Email);

            // Verify the watchers of the issue contains the username.
            var watchers = await issue.GetWatchersAsync();
            Assert.Contains(watchers, w => string.Equals(w.Username, userInfo.Username));

            // Delete user.
            await jira.Users.DeleteUserAsync(userInfo.Username);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        void GetSubTasks(Jira jira)
        {
            var parentTask = jira.CreateIssue("TST");
            parentTask.Type = "1";
            parentTask.Summary = "Test issue with SubTask" + _random.Next(int.MaxValue);
            parentTask.SaveChanges();

            var subTask = jira.CreateIssue("TST", parentTask.Key.Value);
            subTask.Type = "5"; // SubTask issue type.
            subTask.Summary = "Test SubTask" + _random.Next(int.MaxValue);
            subTask.SaveChanges();

            var results = parentTask.GetSubTasksAsync().Result;
            Assert.Single(results);
            Assert.Equal(results.First().Summary, subTask.Summary);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        void RetrieveEmptyIssueLinks(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Summary = "Issue with no links " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Empty(issue.GetIssueLinksAsync().Result);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        void AddAndRetrieveIssueLinks(Jira jira)
        {
            var issue1 = jira.CreateIssue("TST");
            issue1.Summary = "Issue to link from" + _random.Next(int.MaxValue);
            issue1.Type = "Bug";
            issue1.SaveChanges();

            var issue2 = jira.CreateIssue("TST");
            issue2.Summary = "Issue to link to " + _random.Next(int.MaxValue);
            issue2.Type = "Bug";
            issue2.SaveChanges();

            var issue3 = jira.CreateIssue("TST");
            issue3.Summary = "Issue to link to " + _random.Next(int.MaxValue);
            issue3.Type = "Bug";
            issue3.SaveChanges();

            // link the first issue to the second.
            issue1.LinkToIssueAsync(issue2.Key.Value, "Duplicate").Wait();
            issue1.LinkToIssueAsync(issue3.Key.Value, "Duplicate").Wait();

            // Verify links of first issue.
            var issueLinks = issue1.GetIssueLinksAsync().Result;
            Assert.Equal(2, issueLinks.Count());
            Assert.True(issueLinks.All(l => l.OutwardIssue.Key.Value == issue1.Key.Value));
            Assert.True(issueLinks.All(l => l.LinkType.Name == "Duplicate"));
            Assert.Contains(issueLinks, l => l.InwardIssue.Key.Value == issue2.Key.Value);
            Assert.Contains(issueLinks, l => l.InwardIssue.Key.Value == issue3.Key.Value);

            // Verify link of second issue.
            var issueLink = issue2.GetIssueLinksAsync().Result.Single();
            Assert.Equal("Duplicate", issueLink.LinkType.Name);
            Assert.Equal(issue1.Key.Value, issueLink.OutwardIssue.Key.Value);
            Assert.Equal(issue2.Key.Value, issueLink.InwardIssue.Key.Value);

            // Verify link of third issue.
            issueLink = issue3.GetIssueLinksAsync().Result.Single();
            Assert.Equal("Duplicate", issueLink.LinkType.Name);
            Assert.Equal(issue1.Key.Value, issueLink.OutwardIssue.Key.Value);
            Assert.Equal(issue3.Key.Value, issueLink.InwardIssue.Key.Value);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task AddAndRetrieveRemoteLinks(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Summary = "Issue to link from" + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            // Verify issue with no remote links.
            Assert.Empty(issue.GetRemoteLinksAsync().Result);

            var url1 = "https://google.com";
            var title1 = "Google";
            var summary1 = "Search engine";

            var url2 = "https://bing.com";
            var title2 = "Bing";

            // Add remote links
            await issue.AddRemoteLinkAsync(url1, title1, summary1);
            await issue.AddRemoteLinkAsync(url2, title2);

            // Verify remote links of issue.
            var remoteLinks = issue.GetRemoteLinksAsync().Result;
            Assert.Equal(2, remoteLinks.Count());
            Assert.Contains(remoteLinks, l => l.RemoteUrl == url1 && l.Title == title1 && l.Summary == summary1);
            Assert.Contains(remoteLinks, l => l.RemoteUrl == url2 && l.Title == title2 && l.Summary == null);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetTransitionsAsync(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Summary = "Issue with transitions " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            var transitions = await issue.GetAvailableActionsAsync();
            Assert.True(transitions.Count() > 1);

            var transition = transitions.Single(t => t.Name.Equals("Resolve Issue", StringComparison.OrdinalIgnoreCase));
            Assert.False(transition.HasScreen);
            Assert.False(transition.IsInitial);
            Assert.False(transition.IsInitial);
            Assert.False(transition.IsGlobal);
            Assert.Equal("Resolved", transition.To.Name);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TransitionIssueAsync(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Null(issue.ResolutionDate);

            await issue.WorkflowTransitionAsync(WorkflowActions.Resolve);

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Fixed", issue.Resolution.Name);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TransitionIssueByIdAsync(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            var transitions = await issue.GetAvailableActionsAsync();
            var transition = transitions.Single(t => t.Name.Equals("Resolve Issue", StringComparison.OrdinalIgnoreCase));

            await issue.WorkflowTransitionAsync(transition.Id);

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Fixed", issue.Resolution.Name);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TransitionIssueAsyncWithCommentAndFields(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Null(issue.ResolutionDate);
            var updates = new WorkflowTransitionUpdates() { Comment = "Comment with transition" };
            issue.FixVersions.Add("2.0");

            await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, updates, CancellationToken.None);

            var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value);
            Assert.Equal("Resolved", updatedIssue.Status.Name);
            Assert.Equal("Fixed", updatedIssue.Resolution.Name);
            Assert.Equal("2.0", updatedIssue.FixVersions.First().Name);

            var comments = updatedIssue.GetCommentsAsync().Result;
            Assert.Single(comments);
            Assert.Equal("Comment with transition", comments.First().Body);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        void Transition_ResolveIssue(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Null(issue.ResolutionDate);

            issue.WorkflowTransitionAsync(WorkflowActions.Resolve).Wait();

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Fixed", issue.Resolution.Name);
            Assert.NotNull(issue.ResolutionDate);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        void Transition_ResolveIssue_AsWontFix(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            issue.Resolution = "Won't Fix";
            issue.WorkflowTransitionAsync(WorkflowActions.Resolve).Wait();

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Won't Fix", issue.Resolution.Name);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetTimeTrackingDataForIssue(Jira jira)
        {
            var issue = jira.CreateIssue("TST");
            issue.Summary = "Issue with timetracking " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            var timetracking = await issue.GetTimeTrackingDataAsync();
            Assert.Null(timetracking.TimeSpent);

            issue.AddWorklogAsync("2d").Wait();

            timetracking = issue.GetTimeTrackingDataAsync().Result;
            Assert.Equal("2d", timetracking.TimeSpent);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void GetResolutionDate(Jira jira)
        {
            // Arrange
            var issue = jira.CreateIssue("TST");
            var currentDate = DateTime.Now;
            issue.Summary = "Issue to resolve " + Guid.NewGuid().ToString();
            issue.Type = "Bug";

            // Act, Assert: Returns null for unsaved issue.
            Assert.Null(issue.ResolutionDate);

            // Act, Assert: Returns null for saved unresolved issue.
            issue.SaveChanges();
            Assert.Null(issue.ResolutionDate);

            // Act, Assert: returns date for saved resolved issue.
            issue.WorkflowTransitionAsync(WorkflowActions.Resolve).Wait();
            Assert.NotNull(issue.ResolutionDate);
            Assert.Equal(issue.ResolutionDate.Value.Year, currentDate.Year);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void AddGetRemoveAttachmentsFromIssue(Jira jira)
        {
            var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            // create an issue, verify no attachments
            issue.SaveChanges();
            Assert.Empty(issue.GetAttachmentsAsync().Result);

            // upload multiple attachments
            File.WriteAllText("testfile1.txt", "Test File Content 1");
            File.WriteAllText("testfile2.txt", "Test File Content 2");
            issue.AddAttachment("testfile1.txt", "testfile2.txt");

            // verify all attachments can be retrieved.
            var attachments = issue.GetAttachmentsAsync().Result;
            Assert.Equal(2, attachments.Count());
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile1.txt")), "'testfile1.txt' was not downloaded from server");
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile2.txt")), "'testfile2.txt' was not downloaded from server");

            // verify properties of an attachment
            var attachment = attachments.First();
            Assert.NotEmpty(attachment.Author);
            Assert.NotNull(attachment.CreatedDate);
            Assert.True(attachment.FileSize > 0);
            Assert.NotEmpty(attachment.MimeType);

            // download an attachment
            var tempFile = Path.GetTempFileName();
            attachments.First(a => a.FileName.Equals("testfile1.txt")).Download(tempFile);
            Assert.Equal("Test File Content 1", File.ReadAllText(tempFile));

            // remove an attachment
            issue.DeleteAttachmentAsync(attachments.First()).Wait();
            Assert.Single(issue.GetAttachmentsAsync().Result);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task DownloadAttachments(Jira jira)
        {
            // create an issue
            var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue.SaveChanges();

            // upload multiple attachments
            File.WriteAllText("testfile1.txt", "Test File Content 1");
            File.WriteAllText("testfile2.txt", "Test File Content 2");
            issue.AddAttachment("testfile1.txt", "testfile2.txt");

            // Get attachment metadata
            var attachments = await issue.GetAttachmentsAsync(CancellationToken.None);
            Assert.Equal(2, attachments.Count());
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile1.txt")), "'testfile1.txt' was not downloaded from server");
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile2.txt")), "'testfile2.txt' was not downloaded from server");

            // download an attachment.
            var tempFile = Path.GetTempFileName();
            var attachment = attachments.First(a => a.FileName.Equals("testfile1.txt"));

            attachment.Download(tempFile);
            Assert.Equal("Test File Content 1", File.ReadAllText(tempFile));
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task DownloadAttachmentData(Jira jira)
        {
            // create an issue
            var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue.SaveChanges();

            // upload attachment
            File.WriteAllText("testfile.txt", "Test File Content");
            issue.AddAttachment("testfile.txt");

            // Get attachment metadata
            var attachments = await issue.GetAttachmentsAsync(CancellationToken.None);
            Assert.Equal("testfile.txt", attachments.Single().FileName);

            // download attachment as byte array
            var bytes = attachments.Single().DownloadData();

            Assert.Equal(17, bytes.Length);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void AddAndGetComments(Jira jira)
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            // create an issue, verify no comments
            issue.SaveChanges();
            Assert.Empty(issue.GetCommentsAsync().Result);

            // Add a comment
            issue.AddCommentAsync("new comment").Wait();

            var options = new CommentQueryOptions();
            options.Expand.Add("renderedBody");
            var comments = issue.GetCommentsAsync(options).Result;
            Assert.Single(comments);

            var comment = comments.First();
            Assert.Equal("new comment", comment.Body);
            Assert.Equal(DateTime.Now.Year, comment.CreatedDate.Value.Year);
            Assert.Null(comment.Visibility);
            Assert.Equal("new comment", comment.RenderedBody);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task AddAndUpdateComments(Jira jira)
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            // Add a comment
            var comment = new Comment()
            {
                Author = jira.Users.GetMyselfAsync().Result.Username,
                Body = "New comment",
                Visibility = new CommentVisibility("Developers")
            };
            var newComment = await issue.AddCommentAsync(comment);

            // Verify visibility.
            Assert.Equal("role", newComment.Visibility.Type);
            Assert.Equal("Developers", newComment.Visibility.Value);

            // Update the comment
            newComment.Visibility.Value = "Users";
            newComment.Body = "changed body";
            var updatedComment = await issue.UpdateCommentAsync(newComment);

            // verify changes.
            Assert.Equal("role", updatedComment.Visibility.Type);
            Assert.Equal("Users", updatedComment.Visibility.Value);
            Assert.Equal("changed body", updatedComment.Body);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task AddGetAndDeleteCommentsAsync(Jira jira)
        {
            var summaryValue = "Test Summary with comments " + _random.Next(int.MaxValue);
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            // create an issue, verify no comments
            issue.SaveChanges();
            var comments = await issue.GetPagedCommentsAsync();
            Assert.Empty(comments);

            // Add a comment
            var commentFromAdd = await issue.AddCommentAsync("new comment");
            Assert.Equal("new comment", commentFromAdd.Body);

            // Verify comment retrieval
            comments = await issue.GetPagedCommentsAsync();

            Assert.Single(comments);
            var commentFromGet = comments.First();
            Assert.Equal(commentFromAdd.Id, commentFromGet.Id);
            Assert.Equal("new comment", commentFromGet.Body);
            Assert.Empty(commentFromGet.Properties);

            // Delete comment.
            await issue.DeleteCommentAsync(commentFromGet);

            // Verify no comments
            comments = await issue.GetPagedCommentsAsync();
            Assert.Empty(comments);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task CanRetrievePagedCommentsAsync(Jira jira)
        {
            var summaryValue = "Test Summary with comments " + _random.Next(int.MaxValue);
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            // Add a comments
            await issue.AddCommentAsync("new comment1");
            await issue.AddCommentAsync("new comment2");
            await issue.AddCommentAsync("new comment3");
            await issue.AddCommentAsync("new comment4");

            // Verify first page of comments
            var comments = await issue.GetPagedCommentsAsync(2);
            Assert.Equal(2, comments.Count());
            Assert.Equal("new comment1", comments.First().Body);
            Assert.Equal("new comment2", comments.Skip(1).First().Body);

            // Verify second page of comments
            comments = await issue.GetPagedCommentsAsync(2, 2);
            Assert.Equal(2, comments.Count());
            Assert.Equal("new comment3", comments.First().Body);
            Assert.Equal("new comment4", comments.Skip(1).First().Body);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void DeleteIssue(Jira jira)
        {
            // Create issue and verify it is found in server.
            var issue = jira.CreateIssue("TST");
            issue.Type = "1";
            issue.Summary = String.Format("Issue to delete ({0})", _random.Next(int.MaxValue));
            issue.SaveChanges();
            Assert.True(jira.Issues.Queryable.Where(i => i.Key == issue.Key).Any(), "Expected issue in server");

            // Delete issue and verify it is no longer found.
            jira.Issues.DeleteIssueAsync(issue.Key.Value).Wait();
            Assert.Throws<AggregateException>(() => jira.Issues.GetIssueAsync(issue.Key.Value).Result);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void AddAndGetWorklogs(Jira jira)
        {
            var summaryValue = "Test issue with work logs" + _random.Next(int.MaxValue);

            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue.SaveChanges();

            issue.AddWorklogAsync("1d").Wait();
            issue.AddWorklogAsync("1h", WorklogStrategy.RetainRemainingEstimate).Wait();
            issue.AddWorklogAsync("1m", WorklogStrategy.NewRemainingEstimate, "2d").Wait();

            issue.AddWorklogAsync(new Worklog("2d", new DateTime(2012, 1, 1), "comment")).Wait();

            var logs = issue.GetWorklogsAsync().Result;
            Assert.Equal(4, logs.Count());
            Assert.Equal("comment", logs.ElementAt(3).Comment);
            Assert.Equal(new DateTime(2012, 1, 1), logs.ElementAt(3).StartDate);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void DeleteWorklog(Jira jira)
        {
            var summary = "Test issue with worklogs" + _random.Next(int.MaxValue);
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summary,
                Assignee = "admin"
            };
            issue.SaveChanges();

            var worklog = issue.AddWorklogAsync("1h").Result;
            Assert.Single(issue.GetWorklogsAsync().Result);

            issue.DeleteWorklogAsync(worklog).Wait();
            Assert.Empty(issue.GetWorklogsAsync().Result);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task AddAndRemovePropertyAndVerifyProperties(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with properties",
                Assignee = "admin",
            };

            issue.SaveChanges();

            // Verify no properties exist
            var propertyKeys = await jira.Issues.GetPropertyKeysAsync(issue.Key.Value);
            Assert.Empty(propertyKeys);

            // Set new property on issue
            var keyString = "test-property";
            var keyValue = JToken.FromObject("test-string");
            await issue.SetPropertyAsync(keyString, keyValue);

            // Verify one property exists.
            propertyKeys = await jira.Issues.GetPropertyKeysAsync(issue.Key.Value);
            Assert.True(propertyKeys.SequenceEqual(new List<string>() { keyString }));

            // Verify the property key returns the exact value
            var issueProperties = await issue.GetPropertiesAsync(new[] { keyString, "non-existent-property" });

            var truth = new Dictionary<string, JToken>()
            {
                { keyString, keyValue },
            };

            Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
            Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));

            // Delete the property
            await issue.DeletePropertyAsync(keyString);

            // Verify dictionary is empty
            issueProperties = await issue.GetPropertiesAsync(new[] { keyString });
            Assert.False(issueProperties.Any());
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task RemoveInexistantPropertyAndVerifyNoOp(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with properties",
                Assignee = "admin",
            };

            issue.SaveChanges();

            var keyString = "test-property-nonexist";
            await issue.DeletePropertyAsync(keyString);

            // Verify the property isn't returned by the service
            var issueProperties = await issue.GetPropertiesAsync(new[] { keyString });
            Assert.False(issueProperties.Any());
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task AddNullPropertyAndVerify(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with properties",
                Assignee = "admin",
            };

            issue.SaveChanges();

            // Set new property on issue
            var keyString = "test-property-null";
            JToken keyValue = JToken.Parse("null");
            await issue.SetPropertyAsync(keyString, keyValue);

            // Verify the property key returns the exact value
            var issueProperties = await issue.GetPropertiesAsync(new[] { keyString });
            var truth = new Dictionary<string, JToken>()
            {
                // WARN; JToken of null is effectively returned as null.
                // This probably depends on the serializersettings!
                { keyString, null },
            };

            Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
            Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]

        public async Task AddObjectPropertyAndVerify(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with properties",
                Assignee = "admin",
            };

            issue.SaveChanges();

            // Set new property on issue
            var keyString = "test-property-object";
            var valueObject = new
            {
                KeyName = "TestKey",
            };
            JToken keyValue = JToken.FromObject(valueObject);
            await issue.SetPropertyAsync(keyString, keyValue);

            // Verify the property key returns the exact value
            var issueProperties = await issue.GetPropertiesAsync(new[] { keyString });

            var truth = new Dictionary<string, JToken>()
            {
                { keyString, keyValue },
            };

            Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
            Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]

        public async Task AddBoolPropertyAndVerify(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with properties",
                Assignee = "admin",
            };

            issue.SaveChanges();

            // Set new property on issue
            var keyString = "test-property-bool";
            JToken keyValue = JToken.FromObject(true);
            await issue.SetPropertyAsync(keyString, keyValue);

            // Verify the property key returns the exact value
            var issueProperties = await issue.GetPropertiesAsync(new[] { keyString });

            var truth = new Dictionary<string, JToken>()
            {
                { keyString, keyValue },
            };

            Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
            Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]

        public async Task AddListPropertyAndVerify(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with properties",
                Assignee = "admin",
            };

            issue.SaveChanges();

            // Set new property on issue
            var keyString = "test-property-list";
            var valueObject = new List<string>() { "One", "Two", "Three" };
            JToken keyValue = JToken.FromObject(valueObject);
            await issue.SetPropertyAsync(keyString, keyValue);

            // Verify the property key returns the exact value
            var issueProperties = await issue.GetPropertiesAsync(new[] { keyString });

            var truth = new Dictionary<string, JToken>()
            {
                { keyString, keyValue },
            };

            Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
            Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));
        }
    }
}
