using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IntegrationTest
    {
        private readonly Jira _jira;
        private readonly Random _random;

        public IntegrationTest()
        {
            _jira = new Jira("http://localhost:2990/jira", "admin", "admin");
            _jira.Debug = true;
            _random = new Random();
        }

        [Fact]
        public void TestQueryWithZeroResults()
        {
            var issues = from i in _jira.IssueSearch()
                         where i.Created == new DateTime(2010,1,1)
                         select i;

            Assert.Equal(0, issues.Count());
        }

        [Fact]
        public void TestQueryWithOneResult()
        {
            var issues = (from i in _jira.IssueSearch()
                         where i.Reporter == "admin"
                         select i).ToList();

            Assert.Equal(1, issues.Count);
            Assert.Equal("Sample bug in Test Project", issues[0].Summary);
        }

        [Fact]
        public void TestCreateNewIssueWithMinimumFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = summaryValue
            };

            issue = _jira.CreateIssue(issue);


            var issues = (from i in _jira.IssueSearch()
                                where i.Key == issue.Key
                                select i).ToArray();

            Assert.Equal(1, issues.Count());

            Assert.Equal(summaryValue, issues[0].Summary);
            Assert.Equal("TST", issues[0].Project);
            Assert.Equal("1", issues[0].Type);
        }


        //[Fact]
        public void TestCreateNewIssueWithAllFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = new Issue()
            {
                Assignee = "admin",
                Description = "Test Description",
               // DueDate = new DateTime(2011, 12, 12),
                Environment = "Test Environment",
                Project = "TST",
                Reporter = "admin",
                Type = "1",
                Summary = summaryValue
            };

            issue = _jira.CreateIssue(issue);


            var queriedIssue = (from i in _jira.IssueSearch()
                          where i.Key == issue.Key
                          select i).First();

            Assert.Equal(summaryValue, queriedIssue.Summary);
        }
    }
}
