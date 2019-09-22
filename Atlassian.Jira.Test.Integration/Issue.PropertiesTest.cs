using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssuePropertiesTest : BaseIntegrationTest
    {
        [Fact]
        public async Task TimeTrackingPropertyIncludedInResponses()
        {
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with estimates",
                Assignee = "admin"
            };

            var newIssue = await issue.SaveChangesAsync();

            Assert.NotNull(newIssue.TimeTrackingData);
            Assert.Null(newIssue.TimeTrackingData.OriginalEstimate);

            await newIssue.AddWorklogAsync("1d");

            var issuesFromQuery = await _jira.Issues.GetIssuesFromJqlAsync($"id = {newIssue.Key.Value}");
            Assert.Equal("1d", issuesFromQuery.Single().TimeTrackingData.TimeSpent);
        }

        [Fact]
        public async Task VotesAndHasVotedProperties()
        {
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with votes",
                Assignee = "admin"
            };

            issue.SaveChanges();

            // verify no votes
            Assert.Equal(0, issue.Votes.Value);
            Assert.False(issue.HasUserVoted);

            // cast a vote with a second user.
            var jiraTester = Jira.CreateRestClient(BaseIntegrationTest.HOST, "test", "test");
            await jiraTester.RestClient.ExecuteRequestAsync(RestSharp.Method.POST, $"rest/api/2/issue/{issue.Key.Value}/votes");

            // verify votes for first user
            issue.Refresh();
            Assert.Equal(1, issue.Votes.Value);
            Assert.False(issue.HasUserVoted);

            // verify votes for second user
            var issueTester = await jiraTester.Issues.GetIssueAsync(issue.Key.Value);
            Assert.Equal(1, issueTester.Votes.Value);
            Assert.True(issueTester.HasUserVoted);
        }

        [Fact]
        public async Task AddPropertyAndHasProperties()
        {
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with properties",
                Assignee = "admin",
            };

            issue.SaveChanges();

            // Verify no properties exist
            var propertyKeys = await _jira.Issues.GetPropertyKeysAsync(issue.Key.Value);
            Assert.Empty(propertyKeys);

            // Set new property on issue
            var keyString = "test-property";
            var keyValue = JToken.FromObject("test-string");
            await issue.SetPropertyAsync(keyString, keyValue);

            // Verify one property exists.
            propertyKeys = await _jira.Issues.GetPropertyKeysAsync(issue.Key.Value);
            Assert.True(propertyKeys.SequenceEqual(new List<string>() { keyString }));

            // Verify the property key returns the exact value
            var issueProperties = await issue.GetPropertiesAsync(new[] { keyString });

            var truth = new Dictionary<string, JToken>()
            {
                { keyString, keyValue },
            };

            Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
            Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));
        }

        [Fact]
        public async Task RemovePropertyAndPropertyRemoved()
        {
            var issue = new Issue(_jira, "TST")
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
            Assert.False(issueProperties.ContainsKey(keyString));
        }

        [Fact]
        public async Task AddNullPropertyAndVerify()
        {
            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public async Task AddObjectPropertyAndVerify()
        {
            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public async Task AddBoolPropertyAndVerify()
        {
            var issue = new Issue(_jira, "TST")
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

        [Fact]
        public async Task AddListPropertyAndVerify()
        {
            var issue = new Issue(_jira, "TST")
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
