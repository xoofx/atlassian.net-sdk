using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueCreateTest : BaseIntegrationTest
    {
        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task CreateIssueWithIssueTypesPerProject(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "Bug",
                Summary = "Test Summary " + _random.Next(int.MaxValue),
                Assignee = "admin"
            };

            issue.Type.SearchByProjectOnly = true;
            var newIssue = await issue.SaveChangesAsync();

            Assert.Equal("Bug", newIssue.Type.Name);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task CreateIssueWithOriginalEstimate(Jira jira)
        {
            var fields = new CreateIssueFields("TST")
            {
                TimeTrackingData = new IssueTimeTrackingData("1d")
            };

            var issue = new Issue(jira, fields)
            {
                Type = "1",
                Summary = "Test Summary " + _random.Next(int.MaxValue),
                Assignee = "admin"
            };

            var newIssue = await issue.SaveChangesAsync();
            Assert.Equal("1d", newIssue.TimeTrackingData.OriginalEstimate);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task CreateIssueAsync(Jira jira)
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(jira, "TST")
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
            var subTask = new Issue(jira, "TST", newIssue.Key.Value)
            {
                Type = "5",
                Summary = "My Subtask",
                Assignee = "admin"
            };

            var newSubTask = await subTask.SaveChangesAsync();

            Assert.Equal(newIssue.Key.Value, newSubTask.ParentIssueKey);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateAndQueryIssueWithMinimumFieldsSet(Jira jira)
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            var issues = (from i in jira.Issues.Queryable
                          where i.Key == issue.Key
                          select i).ToArray();

            Assert.Single(issues);

            Assert.Equal(summaryValue, issues[0].Summary);
            Assert.Equal("TST", issues[0].Project);
            Assert.Equal("1", issues[0].Type.Id);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateAndQueryIssueWithAllFieldsSet(Jira jira)
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var expectedDueDate = new DateTime(2011, 12, 12);
            var issue = jira.CreateIssue("TST");
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
            issue.Labels.Add("testLabel");

            issue.SaveChanges();

            var queriedIssue = (from i in jira.Issues.Queryable
                                where i.Key == issue.Key
                                select i).ToArray().First();

            Assert.Equal(summaryValue, queriedIssue.Summary);
            Assert.NotNull(queriedIssue.JiraIdentifier);
            Assert.Equal(expectedDueDate, queriedIssue.DueDate.Value);
            Assert.NotNull(queriedIssue.Priority.IconUrl);
            Assert.NotNull(queriedIssue.Type.IconUrl);
            Assert.NotNull(queriedIssue.Status.IconUrl);
            Assert.Contains("testLabel", queriedIssue.Labels);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateAndQueryIssueWithSubTask(Jira jira)
        {
            var parentTask = jira.CreateIssue("TST");
            parentTask.Type = "1";
            parentTask.Summary = "Test issue with SubTask" + _random.Next(int.MaxValue);
            parentTask.SaveChanges();

            var subTask = jira.CreateIssue("TST", parentTask.Key.Value);
            subTask.Type = "5"; // SubTask issue type.
            subTask.Summary = "Test SubTask" + _random.Next(int.MaxValue);
            subTask.SaveChanges();

            Assert.False(parentTask.Type.IsSubTask);
            Assert.True(subTask.Type.IsSubTask);
            Assert.Equal(parentTask.Key.Value, subTask.ParentIssueKey);

            // query the subtask again to make sure it loads everything from server.
            subTask = jira.Issues.GetIssueAsync(subTask.Key.Value).Result;
            Assert.False(parentTask.Type.IsSubTask);
            Assert.True(subTask.Type.IsSubTask);
            Assert.Equal(parentTask.Key.Value, subTask.ParentIssueKey);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateAndQueryIssueWithVersions(Jira jira)
        {
            var summaryValue = "Test issue with versions (Created)" + _random.Next(int.MaxValue);

            var issue = new Issue(jira, "TST")
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

            var newIssue = (from i in jira.Issues.Queryable
                            where i.AffectsVersions == "1.0" && i.AffectsVersions == "2.0"
                                    && i.FixVersions == "2.0" && i.FixVersions == "3.0"
                            select i).First();

            Assert.Equal(2, newIssue.AffectsVersions.Count);
            Assert.Contains(newIssue.AffectsVersions, v => v.Name == "1.0");
            Assert.Contains(newIssue.AffectsVersions, v => v.Name == "2.0");

            Assert.Equal(2, newIssue.FixVersions.Count);
            Assert.Contains(newIssue.FixVersions, v => v.Name == "2.0");
            Assert.Contains(newIssue.FixVersions, v => v.Name == "3.0");
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateAndQueryIssueWithComponents(Jira jira)
        {
            var summaryValue = "Test issue with components (Created)" + _random.Next(int.MaxValue);

            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.Components.Add("Server");
            issue.Components.Add("Client");

            issue.SaveChanges();

            var newIssue = (from i in jira.Issues.Queryable
                            where i.Summary == summaryValue && i.Components == "Server" && i.Components == "Client"
                            select i).First();

            Assert.Equal(2, newIssue.Components.Count);
            Assert.Contains(newIssue.Components, c => c.Name == "Server");
            Assert.Contains(newIssue.Components, c => c.Name == "Client");
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateAndQueryIssueWithCustomField(Jira jira)
        {
            var summaryValue = "Test issue with custom field (Created)" + _random.Next(int.MaxValue);

            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue["Custom Text Field"] = "My new value";
            issue["Custom User Field"] = "admin";

            issue.SaveChanges();

            var newIssue = (from i in jira.Issues.Queryable
                            where i.Summary == summaryValue && i["Custom Text Field"] == "My new value"
                            select i).First();

            Assert.Equal("My new value", newIssue["Custom Text Field"]);
            Assert.Equal("admin", newIssue["Custom User Field"]);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateIssueAsSubtask(Jira jira)
        {
            var summaryValue = "Test issue as subtask " + _random.Next(int.MaxValue);

            var issue = new Issue(jira, "TST", "TST-1")
            {
                Type = "5", //subtask
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue.SaveChanges();

            var subtasks = jira.Issues.GetIssuesFromJqlAsync("project = TST and parent = TST-1").Result;

            Assert.True(subtasks.Any(s => s.Summary.Equals(summaryValue)),
                String.Format("'{0}' was not found as a sub-task of TST-1", summaryValue));
        }
    }
}
