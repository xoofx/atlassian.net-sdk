using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueQueryTest : BaseIntegrationTest
    {
        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetIssueThatIncludesOnlyOneBasicField(Jira jira)
        {
            var options = new IssueSearchOptions("key = TST-1")
            {
                FetchBasicFields = false,
                AdditionalFields = new List<string>() { "summary" }
            };

            var issues = await jira.Issues.GetIssuesFromJqlAsync(options);
            Assert.NotNull(issues.First().Summary);
            Assert.Null(issues.First().Assignee);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetIssueThatIncludesOnlyOneNonBasicField(Jira jira)
        {
            var options = new IssueSearchOptions("key = TST-1")
            {
                FetchBasicFields = false,
                AdditionalFields = new List<string>() { "attachment" }
            };

            var issues = await jira.Issues.GetIssuesFromJqlAsync(options);
            var issue = issues.First();
            Assert.Null(issue.Summary);
            Assert.NotEmpty(issue.AdditionalFields.Attachments);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetIssueThatIncludesOnlyAllNonBasicFields(Jira jira)
        {
            // Arrange
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = "Test issue",
                Assignee = "admin"
            };

            issue.SaveChanges();

            await issue.AddCommentAsync("My comment");
            await issue.AddWorklogAsync("1d");

            // Act
            var options = new IssueSearchOptions($"key = {issue.Key.Value}")
            {
                FetchBasicFields = false,
                AdditionalFields = new List<string>() { "comment", "watches", "worklog" }
            };

            var issues = await jira.Issues.GetIssuesFromJqlAsync(options);
            var serverIssue = issues.First();

            // Assert
            Assert.Null(serverIssue.Summary);
            Assert.True(serverIssue.AdditionalFields.ContainsKey("watches"));

            var worklogs = serverIssue.AdditionalFields.Worklogs;
            Assert.Equal(20, worklogs.ItemsPerPage);
            Assert.Equal(0, worklogs.StartAt);
            Assert.Equal(1, worklogs.TotalItems);
            Assert.Equal("1d", worklogs.First().TimeSpent);

            var comments = serverIssue.AdditionalFields.Comments;
            Assert.Equal(1, comments.ItemsPerPage);
            Assert.Equal(0, comments.StartAt);
            Assert.Equal(1, comments.TotalItems);
            Assert.Equal("My comment", comments.First().Body);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetIssuesAsyncWhenIssueDoesNotExist(Jira jira)
        {
            var dict = await jira.Issues.GetIssuesAsync("TST-9999");

            Assert.False(dict.ContainsKey("TST-9999"));
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void GetIssuesWithPagingMetadata(Jira jira)
        {
            // Arrange: Create 3 issues to query.
            var summaryValue = "Test-Summary-" + Guid.NewGuid().ToString();
            for (int i = 0; i < 3; i++)
            {
                new Issue(jira, "TST")
                {
                    Type = "1",
                    Summary = summaryValue,
                    Assignee = "admin"
                }.SaveChanges();
            }

            // Act: Query for paged issues.
            var jql = String.Format("summary ~ \"{0}\"", summaryValue);
            var result = jira.Issues.GetIssuesFromJqlAsync(jql, 5, 1).Result as IPagedQueryResult<Issue>;

            // Assert
            Assert.Equal(1, result.StartAt);
            Assert.Equal(2, result.Count());
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(5, result.ItemsPerPage);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void GetIssuesFromFilter(Jira jira)
        {
            var issues = jira.Filters.GetIssuesFromFavoriteAsync("One Issue Filter").Result;

            Assert.Single(issues);
            Assert.Equal("TST-1", issues.First().Key.Value);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void QueryWithZeroResults(Jira jira)
        {
            var issues = from i in jira.Issues.Queryable
                         where i.Created == new DateTime(2010, 1, 1)
                         select i;

            Assert.Equal(0, issues.Count());
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void QueryIssueWithLabel(Jira jira)
        {
            var issue = new Issue(jira, "TST")
            {
                Type = "1",
                Summary = "Test issue with labels",
                Assignee = "admin"
            };

            issue.Labels.Add("test-label");
            issue.SaveChanges();

            var serverIssue = (from i in jira.Issues.Queryable
                               where i.Labels == "test-label"
                               select i).First();

            Assert.Contains("test-label", serverIssue.Labels);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void QueryIssueWithCustomDateField(Jira jira)
        {
            var issue = (from i in jira.Issues.Queryable
                         where i["Custom Date Field"] <= new DateTime(2012, 4, 1)
                         select i).First();

            Assert.Equal("Sample bug in Test Project", issue.Summary);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void QueryIssuesWithTakeExpression(Jira jira)
        {
            // create 2 issues with same summary
            var randomNumber = _random.Next(int.MaxValue);
            (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChanges();
            (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChanges();

            // query with take method to only return 1
            var issues = (from i in jira.Issues.Queryable
                          where i.Summary == randomNumber.ToString()
                          select i).Take(1);

            Assert.Equal(1, issues.Count());
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void MaximumNumberOfIssuesPerRequest(Jira jira)
        {
            // create 2 issues with same summary
            var randomNumber = _random.Next(int.MaxValue);
            (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChanges();
            (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChanges();

            //set maximum issues and query
            jira.Issues.MaxIssuesPerRequest = 1;
            var issues = from i in jira.Issues.Queryable
                         where i.Summary == randomNumber.ToString()
                         select i;

            Assert.Equal(1, issues.Count());

        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetIssuesFromJqlAsync(Jira jira)
        {
            var issues = await jira.Issues.GetIssuesFromJqlAsync("key = TST-1");
            Assert.Single(issues);
        }
    }
}
