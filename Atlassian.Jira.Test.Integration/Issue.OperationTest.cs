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
            var changelogs = _jira.GetIssue("TST-1").GetChangeLogs().OrderBy(log => log.CreatedDate);
            Assert.Equal(4, changelogs.Count());

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

            issue.Watchers.Add("test");
            Assert.Equal(2, issue.Watchers.Get().Count());

            issue.Watchers.Remove("admin");
            Assert.Equal(1, issue.Watchers.Get().Count());

            var user = issue.Watchers.Get().First();
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

            var results = parentTask.GetSubTasks();
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

            Assert.Empty(issue.GetIssueLinks());
        }

        [Fact]
        void CreateAndRetrieveIssueLinks()
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
            issue1.LinkToIssue(issue2.Key.Value, "Duplicate");
            issue1.LinkToIssue(issue3.Key.Value, "Duplicate");

            // Verify links of first issue.
            var issueLinks = issue1.GetIssueLinks();
            Assert.Equal(2, issueLinks.Count());
            Assert.True(issueLinks.All(l => l.OutwardIssue.Key.Value == issue1.Key.Value));
            Assert.True(issueLinks.All(l => l.LinkType.Name == "Duplicate"));
            Assert.True(issueLinks.Any(l => l.InwardIssue.Key.Value == issue2.Key.Value));
            Assert.True(issueLinks.Any(l => l.InwardIssue.Key.Value == issue3.Key.Value));

            // Verify link of second issue.
            var issueLink = issue2.GetIssueLinks().Single();
            Assert.Equal("Duplicate", issueLink.LinkType.Name);
            Assert.Equal(issue1.Key.Value, issueLink.OutwardIssue.Key.Value);
            Assert.Equal(issue2.Key.Value, issueLink.InwardIssue.Key.Value);

            // Verify link of third issue.
            issueLink = issue3.GetIssueLinks().Single();
            Assert.Equal("Duplicate", issueLink.LinkType.Name);
            Assert.Equal(issue1.Key.Value, issueLink.OutwardIssue.Key.Value);
            Assert.Equal(issue3.Key.Value, issueLink.InwardIssue.Key.Value);
        }

        [Fact]
        public async Task TransitionIssueAsync()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Null(issue.ResolutionDate);

            await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, CancellationToken.None);

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Fixed", issue.Resolution.Name);
        }

        [Fact]
        public async Task TransitionIssueAsyncWithComment()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Null(issue.ResolutionDate);
            var updates = new WorkflowTransitionUpdates() { Comment = "Comment with transition" };

            await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, updates, CancellationToken.None);

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Fixed", issue.Resolution.Name);

            var comments = issue.GetComments();
            Assert.Equal(1, comments.Count);
            Assert.Equal("Comment with transition", comments[0].Body);
        }

        [Fact]
        void Transition_ResolveIssue()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            Assert.Null(issue.ResolutionDate);

            issue.WorkflowTransition(WorkflowActions.Resolve);

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Fixed", issue.Resolution.Name);

#if !SOAP
            Assert.NotNull(issue.ResolutionDate);
#endif
        }

        [Fact]
        void Transition_ResolveIssue_AsWontFix()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            issue.Resolution = "Won't Fix";
            issue.WorkflowTransition(WorkflowActions.Resolve);

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

            var timetracking = issue.GetTimeTrackingData();
            Assert.Null(timetracking.TimeSpent);

            issue.AddWorklog("2d");

            timetracking = issue.GetTimeTrackingData();
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
            Assert.Null(issue.GetResolutionDate());

            // Act, Assert: Returns null for saved unresolved issue.
            issue.SaveChanges();
            Assert.Null(issue.GetResolutionDate());

            // Act, Assert: returns date for saved resolved issue.
            issue.WorkflowTransition(WorkflowActions.Resolve);
            var date = issue.GetResolutionDate();
            Assert.NotNull(date);
            Assert.Equal(date.Value.Year, currentDate.Year);
        }

        [Fact]
        public void UploadAndDownloadOfAttachments()
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
            Assert.Equal(0, issue.GetAttachments().Count);

            // upload multiple attachments
            File.WriteAllText("testfile1.txt", "Test File Content 1");
            File.WriteAllText("testfile2.txt", "Test File Content 2");
            issue.AddAttachment("testfile1.txt", "testfile2.txt");

            var attachments = issue.GetAttachments();
            Assert.Equal(2, attachments.Count);
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile1.txt")), "'testfile1.txt' was not downloaded from server");
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile2.txt")), "'testfile2.txt' was not downloaded from server");

            // download an attachment
            var tempFile = Path.GetTempFileName();
            attachments.First(a => a.FileName.Equals("testfile1.txt")).Download(tempFile);
            Assert.Equal("Test File Content 1", File.ReadAllText(tempFile));
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
            Assert.Equal(0, issue.GetComments().Count);

            // Add a comment
            issue.AddComment("new comment");

            var comments = issue.GetComments();
            Assert.Equal(1, comments.Count);
            Assert.Equal("new comment", comments[0].Body);
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
            var comments = await issue.GetCommentsAsync();
            Assert.Equal(0, comments.Count());

            // Add a comment
            await issue.AddCommentAsync("new comment");

            // Verify comment retrieval
            comments = await issue.GetCommentsAsync();
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
            Assert.True(_jira.Issues.Where(i => i.Key == issue.Key).Any(), "Expected issue in server");

            // Delete issue and verify it is no longer found.
            _jira.DeleteIssue(issue);
#if SOAP
            Assert.Throws<System.ServiceModel.FaultException>(() => _jira.GetIssue(issue.Key.Value));
#else
            Assert.Throws<InvalidOperationException>(() => _jira.GetIssue(issue.Key.Value));
#endif
        }

        [Fact]
        public void AddAndRetriveLabelsFromIssue()
        {
            var summaryValue = "Test issue with labels (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();
            issue.Labels.Set("label1", "label2");

            issue = _jira.GetIssue(issue.Key.Value);
            Assert.Equal(2, issue.Labels.Cached.Length);

            issue.Labels.Set("label1", "label2", "label3");
            Assert.Equal(3, issue.Labels.Get().Length);
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

            issue.AddWorklog("1d");
            issue.AddWorklog("1h", WorklogStrategy.RetainRemainingEstimate);
            issue.AddWorklog("1m", WorklogStrategy.NewRemainingEstimate, "2d");

            issue.AddWorklog(new Worklog("2d", new DateTime(2012, 1, 1), "comment"));

            var logs = issue.GetWorklogs();
            Assert.Equal(4, logs.Count);
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

            var worklog = issue.AddWorklog("1h");
            Assert.Equal(1, issue.GetWorklogs().Count);

            issue.DeleteWorklog(worklog);
            Assert.Equal(0, issue.GetWorklogs().Count);
        }
    }
}
