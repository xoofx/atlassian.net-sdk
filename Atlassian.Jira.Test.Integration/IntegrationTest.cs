using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

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
            var issues = from i in _jira.Issues
                         where i.Created == new DateTime(2010,1,1)
                         select i;

            Assert.Equal(0, issues.Count());
        }

        [Fact]
        public void TestCreateAndQueryForIssueWithMinimumFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = summaryValue
            };

            issue = _jira.CreateIssue(issue);


            var issues = (from i in _jira.Issues
                                where i.Key == issue.Key
                                select i).ToArray();

            Assert.Equal(1, issues.Count());

            Assert.Equal(summaryValue, issues[0].Summary);
            Assert.Equal("TST", issues[0].Project);
            Assert.Equal("1", issues[0].Type);
        }


        [Fact]
        public void TestCreateAndQueryIssueWithAllFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = new Issue()
            {
                Assignee = "admin",
                Description = "Test Description",
                DueDate = new DateTime(2011, 12, 12),
                Environment = "Test Environment",
                Project = "TST",
                Reporter = "admin",
                Type = "1",
                Summary = summaryValue
            };

            issue = _jira.CreateIssue(issue);


            var queriedIssues = (from i in _jira.Issues
                          where i.Key == issue.Key
                          select i).ToArray();

            Assert.Equal(summaryValue, queriedIssues[0].Summary);
        }

        [Fact]
        public void TestUpdateWithAllFieldsSet()
        {
            // arrange, create an issue to test.
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue()
            {
                Assignee = "admin",
                Description = "Test Description",
                DueDate = new DateTime(2011, 12, 12),
                Environment = "Test Environment",
                Project = "TST",
                Reporter = "admin",
                Type = "1",
                Summary = summaryValue
            };
            issue = _jira.CreateIssue(issue);


            // act, get an issue and update it
            var serverIssue = (from i in _jira.Issues
                                 where i.Key == issue.Key
                                 select i).ToArray().First();

            serverIssue.Description = "Updated Description";
            serverIssue.DueDate = new DateTime(2011, 10, 10);
            serverIssue.Environment = "Updated Environment";
            serverIssue.Summary = "Updated " + summaryValue;
            _jira.UpdateIssue(serverIssue);

            // assert, get the issue again and verify
            var newServerIssue = (from i in _jira.Issues
                               where i.Key == issue.Key
                               select i).ToArray().First();

            Assert.Equal("Updated " + summaryValue, newServerIssue.Summary);
            Assert.Equal("Updated Description", newServerIssue.Description);
            Assert.Equal("Updated Environment", newServerIssue.Environment);

            // Note: Dates returned from JIRA are UTC
            Assert.Equal(new DateTime(2011, 10, 10).ToUniversalTime(), newServerIssue.DueDate);
        }

        [Fact]
        public void TestRetrievalOfAttachments()
        {
            var issue = (from i in _jira.Issues
                         where i.Key == "TST-1"
                         select i).ToArray().First();

            Assert.Equal(2, issue.Attachments.Count());
            Assert.True(issue.Attachments.Any(a => a.FileName.Equals("SampleImage.png")));
            Assert.True(issue.Attachments.Any(a => a.FileName.Equals("SampleTextFile.txt")));
        }
    }
}
