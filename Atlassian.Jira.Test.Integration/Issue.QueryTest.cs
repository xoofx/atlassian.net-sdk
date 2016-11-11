using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueQueryTest : BaseIntegrationTest
    {
        [Fact]
        public void GetIssuesWithPagingMetadata()
        {
            // Arrange: Create 3 issues to query.
            var summaryValue = "Test-Summary-" + Guid.NewGuid().ToString();
            for (int i = 0; i < 3; i++)
            {
                new Issue(_jira, "TST")
                {
                    Type = "1",
                    Summary = summaryValue,
                    Assignee = "admin"
                }.SaveChanges();
            }

            // Act: Query for paged issues.
            var jql = String.Format("summary ~ \"{0}\"", summaryValue);
            var result = _jira.Issues.GetIssuesFromJqlAsync(jql, 5, 1).Result as IPagedQueryResult<Issue>;

            // Assert
            Assert.Equal(1, result.StartAt);
            Assert.Equal(2, result.Count());
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(5, result.ItemsPerPage);
        }

        [Fact]
        public void GetIssuesFromFilter()
        {
            var issues = _jira.Filters.GetIssuesFromFavoriteAsync("One Issue Filter").Result;

            Assert.Equal(1, issues.Count());
            Assert.Equal("TST-1", issues.First().Key.Value);
        }

        [Fact]
        public void QueryWithZeroResults()
        {
            var issues = from i in _jira.Issues.Queryable
                         where i.Created == new DateTime(2010, 1, 1)
                         select i;

            Assert.Equal(0, issues.Count());
        }

        [Fact]
        public void QueryIssueWithCustomDateField()
        {
            var issue = (from i in _jira.Issues.Queryable
                         where i["Custom Date Field"] <= new DateTime(2012, 4, 1)
                         select i).First();

            Assert.Equal("Sample bug in Test Project", issue.Summary);
        }

        [Fact]
        public void QueryIssuesWithTakeExpression()
        {
            // create 2 issues with same summary
            var randomNumber = _random.Next(int.MaxValue);
            (new Issue(_jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChanges();
            (new Issue(_jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChanges();

            // query with take method to only return 1
            var issues = (from i in _jira.Issues.Queryable
                          where i.Summary == randomNumber.ToString()
                          select i).Take(1);

            Assert.Equal(1, issues.Count());
        }

        [Fact]
        public void MaximumNumberOfIssuesPerRequest()
        {
            // create 2 issues with same summary
            var randomNumber = _random.Next(int.MaxValue);
            (new Issue(_jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChanges();
            (new Issue(_jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChanges();

            //set maximum issues and query
            _jira.MaxIssuesPerRequest = 1;
            var issues = from i in _jira.Issues.Queryable
                         where i.Summary == randomNumber.ToString()
                         select i;

            Assert.Equal(1, issues.Count());

        }

        [Fact]
        public async Task GetIssuesFromJqlAsync()
        {
            var issues = await _jira.Issues.GetIssuesFromJqlAsync("key = TST-1");
            Assert.Equal(issues.Count(), 1);
        }
    }
}
