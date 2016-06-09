using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueUpdateTest : BaseIntegrationTest
    {
        [Fact]
        public async Task UpdateIssueAsync()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue.SaveChanges();

            //retrieve the issue from server and update
            issue = await _jira.RestClient.GetIssueAsync(issue.Key.Value, CancellationToken.None);
            issue.Type = "2";
            issue = await _jira.RestClient.UpdateIssueAsync(issue, CancellationToken.None);
            Assert.Equal("2", issue.Type.Id);
        }

        [Fact]
        public void UpdateNamedEntities_ById()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "AutoLoadNamedEntities_ById " + _random.Next(int.MaxValue);
            issue.Type = "1";
            issue.Priority = "5";
            issue.SaveChanges();

            Assert.Equal("1", issue.Type.Id);
            Assert.Equal("Bug", issue.Type.Name);

            Assert.Equal("5", issue.Priority.Id);
            Assert.Equal("Trivial", issue.Priority.Name);
        }

        [Fact]
        public void UpdateNamedEntities_ByName()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "AutoLoadNamedEntities_Name " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.Priority = "Trivial";
            issue.SaveChanges();

            Assert.Equal("1", issue.Type.Id);
            Assert.Equal("Bug", issue.Type.Name);

            Assert.Equal("5", issue.Priority.Id);
            Assert.Equal("Trivial", issue.Priority.Name);
        }

        [Fact]
        public void UpdateIssueType()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue.SaveChanges();

            //retrieve the issue from server and update
            issue = _jira.GetIssue(issue.Key.Value);
            issue.Type = "2";
            issue.SaveChanges();

            //retrieve again and verify
            issue = _jira.GetIssue(issue.Key.Value);
            Assert.Equal("2", issue.Type.Id);
        }

        [Fact]
        public void UpdateWithAllFieldsSet()
        {
            // arrange, create an issue to test.
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Assignee = "admin",
                Description = "Test Description",
                DueDate = new DateTime(2011, 12, 12),
                Environment = "Test Environment",
                Reporter = "admin",
                Type = "1",
                Summary = summaryValue
            };
            issue.SaveChanges();

            // act, get an issue and update it
            var serverIssue = (from i in _jira.Issues
                               where i.Key == issue.Key
                               select i).ToArray().First();

            serverIssue.Description = "Updated Description";
            serverIssue.DueDate = new DateTime(2011, 10, 10);
            serverIssue.Environment = "Updated Environment";
            serverIssue.Summary = "Updated " + summaryValue;
            serverIssue.SaveChanges();

            // assert, get the issue again and verify
            var newServerIssue = (from i in _jira.Issues
                                  where i.Key == issue.Key
                                  select i).ToArray().First();

            Assert.Equal("Updated " + summaryValue, newServerIssue.Summary);
            Assert.Equal("Updated Description", newServerIssue.Description);
            Assert.Equal("Updated Environment", newServerIssue.Environment);
            Assert.Equal(serverIssue.DueDate, newServerIssue.DueDate);
        }

        [Fact]
        public void UpdateAssignee()
        {
            var summaryValue = "Test issue with assignee (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            issue.Assignee = "test"; //username
            issue.SaveChanges();
            Assert.Equal("test", issue.Assignee);

            issue.Assignee = "admin";
            issue.SaveChanges();
            Assert.Equal("admin", issue.Assignee);
        }

        [Fact]
        public void AddAndRemoveVersions()
        {
            var summaryValue = "Test issue with versions (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            issue.AffectsVersions.Add("1.0");
            issue.FixVersions.Add("2.0");
            issue.SaveChanges();
            Assert.Equal(1, issue.AffectsVersions.Count);
            Assert.Equal(1, issue.FixVersions.Count);
            Assert.Equal("1.0", issue.AffectsVersions.First().Name);
            Assert.Equal("2.0", issue.FixVersions.First().Name);

            issue.AffectsVersions.Remove("1.0");
            issue.AffectsVersions.Add("2.0");
            issue.FixVersions.Remove("2.0");
            issue.FixVersions.Add("3.0");
            issue.SaveChanges();
            Assert.Equal(1, issue.AffectsVersions.Count);
            Assert.Equal(1, issue.FixVersions.Count);
            Assert.Equal("2.0", issue.AffectsVersions.First().Name);
            Assert.Equal("3.0", issue.FixVersions.First().Name);

            issue.AffectsVersions.Remove("2.0");
            issue.FixVersions.Remove("3.0");
            issue.SaveChanges();
            Assert.Equal(0, issue.AffectsVersions.Count);
            Assert.Equal(0, issue.FixVersions.Count);

            issue.AffectsVersions.Add("1.0");
            issue.AffectsVersions.Add("2.0");
            issue.FixVersions.Add("2.0");
            issue.FixVersions.Add("3.0");
            issue.SaveChanges();

            Assert.Equal(2, issue.FixVersions.Count);
            Assert.True(issue.FixVersions.Any(v => v.Name == "2.0"));
            Assert.True(issue.FixVersions.Any(v => v.Name == "3.0"));

            Assert.Equal(2, issue.AffectsVersions.Count);
            Assert.True(issue.AffectsVersions.Any(v => v.Name == "1.0"));
            Assert.True(issue.AffectsVersions.Any(v => v.Name == "2.0"));
        }

        [Fact]
        public void AddAndRemoveComponents()
        {
            var summaryValue = "Test issue with components (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            issue.Components.Add("Client");
            issue.SaveChanges();
            Assert.Equal(1, issue.Components.Count);
            Assert.Equal("Client", issue.Components.First().Name);

            issue.Components.Remove("Client");
            issue.Components.Add("Server");
            issue.SaveChanges();
            Assert.Equal(1, issue.Components.Count);
            Assert.Equal("Server", issue.Components.First().Name);

            issue.Components.Remove("Server");
            issue.SaveChanges();
            Assert.Equal(0, issue.Components.Count);

            issue.Components.Add("Client");
            issue.Components.Add("Server");
            issue.SaveChanges();
            Assert.Equal(2, issue.Components.Count);
            Assert.True(issue.Components.Any(c => c.Name == "Server"));
            Assert.True(issue.Components.Any(c => c.Name == "Client"));
        }

        [Fact]
        public void UpdateIssueWithCustomField()
        {
            var summaryValue = "Test issue with custom field (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue["Custom Text Field"] = "My new value";

            issue.SaveChanges();

            issue["Custom Text Field"] = "My updated value";
            issue.SaveChanges();

            Assert.Equal("My updated value", issue["Custom Text Field"]);
        }
    }
}
