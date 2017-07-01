using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueOperationsTest : BaseIntegrationTest
    {
        [Fact]
        void GetChangeLogsForIssue()
        {
            var changelogs = _jira.Issues.GetIssueAsync("TST-1").Result.GetChangeLogsAsync().Result.OrderBy(log => log.CreatedDate);
            Assert.True(changelogs.Count() >= 4);

            var firstChangeLog = changelogs.First();
            Assert.Equal("admin", firstChangeLog.Author.Username);
            Assert.NotNull(firstChangeLog.CreatedDate);
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
            Assert.Equal(1, issue.GetWatchersAsync().Result.Count());

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
            Assert.Equal(results.Count(), 1);
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
            Assert.True(issueLinks.Any(l => l.InwardIssue.Key.Value == issue2.Key.Value));
            Assert.True(issueLinks.Any(l => l.InwardIssue.Key.Value == issue3.Key.Value));

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
        void RetrieveEmptyRemoteLinks()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue with no links " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Empty(issue.GetRemoteLinksAsync().Result);
        }

        [Fact]
        public async Task AddAndRetrieveRemoteLinks()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to link from" + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

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
            Assert.True(remoteLinks.Any(l => l.RemoteUrl == url1 && l.Title == title1 && l.Summary == summary1));
            Assert.True(remoteLinks.Any(l => l.RemoteUrl == url2 && l.Title == title2 && l.Summary == null));
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
            Assert.Equal(1, comments.Count());
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
        public void GetTimeTrackingDataForIssue()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue with timetracking " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            var timetracking = issue.GetTimeTrackingDataAsync().Result;
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
            Assert.Equal(0, issue.GetAttachmentsAsync().Result.Count());

            // upload multiple attachments
            File.WriteAllText("testfile1.txt", "Test File Content 1");
            File.WriteAllText("testfile2.txt", "Test File Content 2");
            issue.AddAttachment("testfile1.txt", "testfile2.txt");

            var attachments = issue.GetAttachmentsAsync().Result;
            Assert.Equal(2, attachments.Count());
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile1.txt")), "'testfile1.txt' was not downloaded from server");
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile2.txt")), "'testfile2.txt' was not downloaded from server");

            // download an attachment
            var tempFile = Path.GetTempFileName();
            attachments.First(a => a.FileName.Equals("testfile1.txt")).Download(tempFile);
            Assert.Equal("Test File Content 1", File.ReadAllText(tempFile));

            // remove an attachment
            issue.DeleteAttachmentAsync(attachments.First()).Wait();
            Assert.Equal(1, issue.GetAttachmentsAsync().Result.Count());
        }

        [Fact]
        public async Task DownloadAttachmentsAsync()
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

            // download an attachment multiple times
            var tempFile = Path.GetTempFileName();
            var attachment = attachments.First(a => a.FileName.Equals("testfile1.txt"));

            var task1 = attachment.DownloadAsync(tempFile);
            var task2 = attachment.DownloadAsync(tempFile);

            await task2;

            Assert.True(task1.IsCanceled);
            Assert.Equal("Test File Content 1", File.ReadAllText(tempFile));
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
            Assert.Equal(0, issue.GetCommentsAsync().Result.Count());

            // Add a comment
            issue.AddCommentAsync("new comment").Wait();

            var comments = issue.GetCommentsAsync().Result;
            Assert.Equal(1, comments.Count());

            var comment = comments.First();
            Assert.Equal("new comment", comment.Body);
            Assert.Equal(DateTime.Now.Year, comment.CreatedDate.Value.Year);
        }

        [Fact]
        public async Task AddAndGetCommentsAsync()
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
            Assert.Equal(0, comments.Count());

            // Add a comment
            await issue.AddCommentAsync("new comment");

            // Verify comment retrieval
            comments = await issue.GetPagedCommentsAsync();
            Assert.Equal(1, comments.Count());
            Assert.Equal("new comment", comments.First().Body);
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
            Assert.Equal(1, issue.GetWorklogsAsync().Result.Count());

            issue.DeleteWorklogAsync(worklog).Wait();
            Assert.Equal(0, issue.GetWorklogsAsync().Result.Count());
        }
    }
}
