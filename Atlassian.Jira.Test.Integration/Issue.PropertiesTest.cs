using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssuePropertiesTest
    {
        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task ReporterUserAndAssigneeUserAvailableFromResponse(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "Bug",
                Summary = "Test Summary with assignee",
                Assignee = "admin"
            };

            issue = await issue.SaveChangesAsync();

            Assert.Equal("admin", issue.Reporter);
            Assert.Equal("admin@example.com", issue.ReporterUser.Email);
            Assert.Equal("admin", issue.Assignee);
            Assert.Equal("admin@example.com", issue.AssigneeUser.Email);

            Assert.NotNull(issue.ReporterUser.AvatarUrls);
            Assert.NotNull(issue.ReporterUser.AvatarUrls.XSmall);
            Assert.NotNull(issue.ReporterUser.AvatarUrls.Small);
            Assert.NotNull(issue.ReporterUser.AvatarUrls.Medium);
            Assert.NotNull(issue.ReporterUser.AvatarUrls.Large);

            Assert.NotNull(issue.AssigneeUser.AvatarUrls);
            Assert.NotNull(issue.AssigneeUser.AvatarUrls.XSmall);
            Assert.NotNull(issue.AssigneeUser.AvatarUrls.Small);
            Assert.NotNull(issue.AssigneeUser.AvatarUrls.Medium);
            Assert.NotNull(issue.AssigneeUser.AvatarUrls.Large);

            issue.Assignee = "test";
            issue = await issue.SaveChangesAsync();
            Assert.Equal("test", issue.Assignee);
            Assert.Equal("test@qa.com", issue.AssigneeUser.Email);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TimeTrackingPropertyIncludedInResponses(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with estimates",
                Assignee = "admin"
            };

            var newIssue = await issue.SaveChangesAsync();

            Assert.NotNull(newIssue.TimeTrackingData);
            Assert.Null(newIssue.TimeTrackingData.OriginalEstimate);

            await newIssue.AddWorklogAsync("1d");

            var issuesFromQuery = await jira.Issues.GetIssuesFromJqlAsync($"id = {newIssue.Key.Value}");
            Assert.Equal("1d", issuesFromQuery.Single().TimeTrackingData.TimeSpent);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task VotesAndHasVotedProperties(Jira jira)
        {
            var issue = new Issue(jira, "TST")
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
            var jiraTester = Jira.CreateRestClient(JiraProvider.HOST, "test", "test");
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
