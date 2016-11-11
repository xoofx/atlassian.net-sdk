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
    public class IssueCreateTest : BaseIntegrationTest
    {
        [Fact]
        public async Task CreateIssueAsync()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            var newIssue = await issue.SaveChangesAsync();
            Assert.Equal(summaryValue, newIssue.Summary);
            Assert.Equal("TST", newIssue.Project);
            Assert.Equal("1", newIssue.Type.Id);

            // Create a subtask async.
            var subTask = new Issue(_jira, "TST", newIssue.Key.Value)
            {
                Type = "5",
                Summary = "My Subtask",
                Assignee = "admin"
            };

            var newSubTask = await subTask.SaveChangesAsync();

            Assert.Equal(newIssue.Key.Value, newSubTask.ParentIssueKey);
        }

        public void CreateAndQueryIssueWithMinimumFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            var issues = (from i in _jira.Issues.Queryable
                          where i.Key == issue.Key
                          select i).ToArray();

            Assert.Equal(1, issues.Count());

            Assert.Equal(summaryValue, issues[0].Summary);
            Assert.Equal("TST", issues[0].Project);
            Assert.Equal("1", issues[0].Type.Id);
        }

        [Fact]
        public void CreateAndQueryIssueWithAllFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var expectedDueDate = new DateTime(2011, 12, 12);
            var issue = _jira.CreateIssue("TST");
            issue.AffectsVersions.Add("1.0");
            issue.Assignee = "admin";
            issue.Components.Add("Server");
            issue["Custom Text Field"] = "Test Value";  // custom field
            issue.Description = "Test Description";
            issue.DueDate = expectedDueDate;
            issue.Environment = "Test Environment";
            issue.FixVersions.Add("2.0");
            issue.Priority = "Major";
            issue.Reporter = "admin";
            issue.Summary = summaryValue;
            issue.Type = "1";

            issue.SaveChanges();

            var queriedIssue = (from i in _jira.Issues.Queryable
                                where i.Key == issue.Key
                                select i).ToArray().First();

            Assert.Equal(summaryValue, queriedIssue.Summary);
            Assert.NotNull(queriedIssue.JiraIdentifier);
            Assert.Equal(expectedDueDate, queriedIssue.DueDate.Value);
            Assert.NotNull(queriedIssue.Priority.IconUrl);
            Assert.NotNull(queriedIssue.Type.IconUrl);
            Assert.NotNull(queriedIssue.Status.IconUrl);
        }

        [Fact]
        public void CreateAndQueryIssueWithSubTask()
        {
            var parentTask = _jira.CreateIssue("TST");
            parentTask.Type = "1";
            parentTask.Summary = "Test issue with SubTask" + _random.Next(int.MaxValue);
            parentTask.SaveChanges();

            var subTask = _jira.CreateIssue("TST", parentTask.Key.Value);
            subTask.Type = "5"; // SubTask issue type.
            subTask.Summary = "Test SubTask" + _random.Next(int.MaxValue);
            subTask.SaveChanges();

            Assert.False(parentTask.Type.IsSubTask);
            Assert.True(subTask.Type.IsSubTask);
            Assert.Equal(parentTask.Key.Value, subTask.ParentIssueKey);

            // query the subtask again to make sure it loads everything from server.
            subTask = _jira.Issues.GetIssueAsync(subTask.Key.Value).Result;
            Assert.False(parentTask.Type.IsSubTask);
            Assert.True(subTask.Type.IsSubTask);
            Assert.Equal(parentTask.Key.Value, subTask.ParentIssueKey);
        }

        [Fact]
        public void CreateAndQueryIssueWithVersions()
        {
            var summaryValue = "Test issue with versions (Created)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.AffectsVersions.Add("1.0");
            issue.AffectsVersions.Add("2.0");

            issue.FixVersions.Add("3.0");
            issue.FixVersions.Add("2.0");

            issue.SaveChanges();

            var newIssue = (from i in _jira.Issues.Queryable
                            where i.AffectsVersions == "1.0" && i.AffectsVersions == "2.0"
                                    && i.FixVersions == "2.0" && i.FixVersions == "3.0"
                            select i).First();

            Assert.Equal(2, newIssue.AffectsVersions.Count);
            Assert.True(newIssue.AffectsVersions.Any(v => v.Name == "1.0"));
            Assert.True(newIssue.AffectsVersions.Any(v => v.Name == "2.0"));

            Assert.Equal(2, newIssue.FixVersions.Count);
            Assert.True(newIssue.FixVersions.Any(v => v.Name == "2.0"));
            Assert.True(newIssue.FixVersions.Any(v => v.Name == "3.0"));
        }

        [Fact]
        public void CreateAndQueryIssueWithComponents()
        {
            var summaryValue = "Test issue with components (Created)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.Components.Add("Server");
            issue.Components.Add("Client");

            issue.SaveChanges();

            var newIssue = (from i in _jira.Issues.Queryable
                            where i.Summary == summaryValue && i.Components == "Server" && i.Components == "Client"
                            select i).First();

            Assert.Equal(2, newIssue.Components.Count);
            Assert.True(newIssue.Components.Any(c => c.Name == "Server"));
            Assert.True(newIssue.Components.Any(c => c.Name == "Client"));
        }

        [Fact]
        public void CreateAndQueryIssueWithCustomField()
        {
            var summaryValue = "Test issue with custom field (Created)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue["Custom Text Field"] = "My new value";
            issue["Custom User Field"] = "admin";

            issue.SaveChanges();

            var newIssue = (from i in _jira.Issues.Queryable
                            where i.Summary == summaryValue && i["Custom Text Field"] == "My new value"
                            select i).First();

            Assert.Equal("My new value", newIssue["Custom Text Field"]);
            Assert.Equal("admin", newIssue["Custom User Field"]);
        }

        [Fact]
        public void CreateIssueAsSubtask()
        {
            var summaryValue = "Test issue as subtask " + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST", "TST-1")
            {
                Type = "5", //subtask
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue.SaveChanges();

            var subtasks = _jira.Issues.GetIssuesFromJqlAsync("project = TST and parent = TST-1").Result;

            Assert.True(subtasks.Any(s => s.Summary.Equals(summaryValue)),
                String.Format("'{0}' was not found as a sub-task of TST-1", summaryValue));
        }
    }
}
