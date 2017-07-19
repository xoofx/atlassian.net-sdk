using System;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssuePropertiesTest : BaseIntegrationTest
    {
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
    }
}
