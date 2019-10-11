using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueOperationsTest : BaseIntegrationTest
    {
        [Fact]
        async Task AssignIssue()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Type = "1";
            issue.Summary = "Test issue to assign" + _random.Next(int.MaxValue);

            issue.SaveChanges();
            Assert.Equal("admin", issue.Assignee);

            await issue.AssignAsync("test");
            Assert.Equal("test", issue.Assignee);

            issue = await _jira.Issues.GetIssueAsync(issue.Key.Value);
            Assert.Equal("test", issue.Assignee);
        }

        [Fact]
        void GetChangeLogsForIssue()
        {
            var changelogs = _jira.Issues.GetIssueAsync("TST-1").Result.GetChangeLogsAsync().Result.OrderBy(log => log.CreatedDate);
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

        [Fact]
        void AddAndRemoveWatchersToIssue()
        {
            var issue = _jira.CreateIssue("TST");
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

        [Fact]
        void GetSubTasks()
        {
            var parentTask = _jira.CreateIssue("TST");
            parentTask.Type = "1";
            parentTask.Summary = "Test issue with SubTask" + _random.Next(int.MaxValue);
            parentTask.SaveChanges();

            var subTask = _jira.CreateIssue("TST", parentTask.Key.Value);
            subTask.Type = "5"; // SubTask issue type.
            subTask.Summary = "Test SubTask" + _random.Next(int.MaxValue);
            subTask.SaveChanges();

            var results = parentTask.GetSubTasksAsync().Result;
            Assert.Single(results);
            Assert.Equal(results.First().Summary, subTask.Summary);
        }

        [Fact]
        void RetrieveEmptyIssueLinks()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue with no links " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Empty(issue.GetIssueLinksAsync().Result);
        }

        [Fact]
        void AddAndRetrieveIssueLinks()
        {
            var issue1 = _jira.CreateIssue("TST");
            issue1.Summary = "Issue to link from" + _random.Next(int.MaxValue);
            issue1.Type = "Bug";
            issue1.SaveChanges();

            var issue2 = _jira.CreateIssue("TST");
            issue2.Summary = "Issue to link to " + _random.Next(int.MaxValue);
            issue2.Type = "Bug";
            issue2.SaveChanges();

            var issue3 = _jira.CreateIssue("TST");
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

        [Fact]
        public async Task AddAndRetrieveRemoteLinks()
        {
            var issue = _jira.CreateIssue("TST");
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

        [Fact]
        public async Task GetTransitionsAsync()
        {
            var issue = _jira.CreateIssue("TST");
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

        [Fact]
        public async Task TransitionIssueAsync()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Null(issue.ResolutionDate);

            await issue.WorkflowTransitionAsync(WorkflowActions.Resolve);

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Fixed", issue.Resolution.Name);
        }

        [Fact]
        public async Task TransitionIssueAsyncWithCommentAndFields()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Null(issue.ResolutionDate);
            var updates = new WorkflowTransitionUpdates() { Comment = "Comment with transition" };
            issue.FixVersions.Add("2.0");

            await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, updates, CancellationToken.None);

            var updatedIssue = await _jira.Issues.GetIssueAsync(issue.Key.Value);
            Assert.Equal("Resolved", updatedIssue.Status.Name);
            Assert.Equal("Fixed", updatedIssue.Resolution.Name);
            Assert.Equal("2.0", updatedIssue.FixVersions.First().Name);

            var comments = updatedIssue.GetCommentsAsync().Result;
            Assert.Single(comments);
            Assert.Equal("Comment with transition", comments.First().Body);
        }

        [Fact]
        void Transition_ResolveIssue()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Null(issue.ResolutionDate);

            issue.WorkflowTransitionAsync(WorkflowActions.Resolve).Wait();

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Fixed", issue.Resolution.Name);
            Assert.NotNull(issue.ResolutionDate);
        }

        [Fact]
        void Transition_ResolveIssue_AsWontFix()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            issue.Resolution = "Won't Fix";
            issue.WorkflowTransitionAsync(WorkflowActions.Resolve).Wait();

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Won't Fix", issue.Resolution.Name);
        }

        [Fact]
        public async Task GetTimeTrackingDataForIssue()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue with timetracking " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            var timetracking = await issue.GetTimeTrackingDataAsync();
            Assert.Null(timetracking.TimeSpent);

            issue.AddWorklogAsync("2d").Wait();

            timetracking = issue.GetTimeTrackingDataAsync().Result;
            Assert.Equal("2d", timetracking.TimeSpent);
        }

        [Fact]
        public void GetResolutionDate()
        {
            // Arrange
            var issue = _jira.CreateIssue("TST");
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

        [Fact]
        public void AddGetRemoveAttachmentsFromIssue()
        {
            var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public async Task DownloadAttachments()
        {
            // create an issue
            var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public async Task DownloadAttachmentData()
        {
            // create an issue
            var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public void AddAndGetComments()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public async Task AddAndUpdateComments()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            // Add a comment
            var comment = new Comment()
            {
                Author = _jira.Credentials.UserName,
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

        [Fact]
        public async Task AddGetAndDeleteCommentsAsync()
        {
            var summaryValue = "Test Summary with comments " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public async Task CanRetrievePagedCommentsAsync()
        {
            var summaryValue = "Test Summary with comments " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public void DeleteIssue()
        {
            // Create issue and verify it is found in server.
            var issue = _jira.CreateIssue("TST");
            issue.Type = "1";
            issue.Summary = String.Format("Issue to delete ({0})", _random.Next(int.MaxValue));
            issue.SaveChanges();
            Assert.True(_jira.Issues.Queryable.Where(i => i.Key == issue.Key).Any(), "Expected issue in server");

            // Delete issue and verify it is no longer found.
            _jira.Issues.DeleteIssueAsync(issue.Key.Value).Wait();
            Assert.Throws<AggregateException>(() => _jira.Issues.GetIssueAsync(issue.Key.Value).Result);
        }

        [Fact]
        public void AddAndGetWorklogs()
        {
            var summaryValue = "Test issue with work logs" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public void DeleteWorklog()
        {
            var summary = "Test issue with worklogs" + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
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
    }
}
