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
        public void QueryWithZeroResults()
        {
            var issues = from i in _jira.Issues
                         where i.Created == new DateTime(2010,1,1)
                         select i;

            Assert.Equal(0, issues.Count());
        }

        [Fact]
        public void CreateAndQueryForIssueWithMinimumFieldsSet()
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
        public void CreateAndQueryIssueWithAllFieldsSet()
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
        public void UpdateWithAllFieldsSet()
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
        public void UploadAndDownloadOfAttachments()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = summaryValue
            };

            // create an issue, verify no attachments
            issue = _jira.CreateIssue(issue);
            Assert.Equal(0, issue.GetAttachments().Count);

            // upload an attachment
            File.WriteAllText("testfile.txt", "Test File Content");
            issue.AddAttachment("testfile.txt");

            var attachments = issue.GetAttachments();
            Assert.Equal(1, attachments.Count);
            Assert.Equal("testfile.txt", attachments[0].FileName);

            // download an attachment
            var tempFile = Path.GetTempFileName();
            attachments[0].Download(tempFile);
            Assert.Equal("Test File Content", File.ReadAllText(tempFile));
        }

        [Fact]
        public void AddingAndRetrievingComments()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = summaryValue
            };

            // create an issue, verify no comments
            issue = _jira.CreateIssue(issue);
            Assert.Equal(0, issue.GetComments().Count);

            // Add a comment
            issue.AddComment("new comment");

            var comments = issue.GetComments();
            Assert.Equal(1, comments.Count);
            Assert.Equal("new comment", comments[0].Body);

        }

        [Fact]
        public void MaximumNumberOfIssuesPerRequest()
        {
            // create 2 issues with same summary
            var randomNumber = _random.Next(int.MaxValue);
            _jira.CreateIssue(new Issue() { Project = "TST", Type = "1", Summary = "Test Summary " + randomNumber });
            _jira.CreateIssue(new Issue() { Project = "TST", Type = "1", Summary = "Test Summary " + randomNumber }); 

            //set maximum issues and query
            _jira.MaxIssuesPerRequest = 1;
            var issues = from i in _jira.Issues
                         where i.Summary == randomNumber.ToString()
                         select i;

            Assert.Equal(1, issues.Count());

        }

        [Fact]
        public void QueryIssuesWithTakeExpression()
        {
            // create 2 issues with same summary
            var randomNumber = _random.Next(int.MaxValue);
            _jira.CreateIssue(new Issue() { Project = "TST", Type = "1", Summary = "Test Summary " + randomNumber });
            _jira.CreateIssue(new Issue() { Project = "TST", Type = "1", Summary = "Test Summary " + randomNumber });

            // query with take method to only return 1
            var issues = (from i in _jira.Issues
                         where i.Summary == randomNumber.ToString()
                         select i).Take(1);

            Assert.Equal(1, issues.Count());
        }

        [Fact]
        public void RetrieveIssueTypesForProject()
        {
            var issueTypes = _jira.GetIssueTypes("TST");

            Assert.Equal(4, issueTypes.Count());
            Assert.True(issueTypes.Any(i => i.Name == "Bug"));
        }

        [Fact]
        public void RetrievesIssuePriorities()
        {
            var priorities = _jira.GetIssuePriorities();

            Assert.True(priorities.Any(i => i.Name == "Blocker"));
        }

        [Fact]
        public void RetrievesIssueResolutions()
        {
            var resolutions = _jira.GetIssueResolutions();

            Assert.True(resolutions.Any(i => i.Name == "Fixed"));
        }

        [Fact]
        public void RetrievesIssueStatuses()
        {
            var statuses = _jira.GetIssueStatuses();

            Assert.True(statuses.Any(i => i.Name == "Open"));
        }

        /// <summary>
        /// https://bitbucket.org/farmas/atlassian.net-sdk/issue/3/serialization-error-when-querying-some
        /// </summary>
        [Fact]
        public void HandleRetrievalOfMessagesWithLargeContentStrings()
        {
            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = "Serialization nastiness"
            };

            issue.Description = File.ReadAllText("LongIssueDescription.txt"); 
            var serverIssue = _jira.CreateIssue(issue);

            Assert.Contains("Second stack trace:", serverIssue.Description);
        }

        [Fact]
        public void RetrievesAffectsVersions()
        {
            var issues = from i in _jira.Issues
                         where i.Key == "TST-1"
                         select i;

            var versions = issues.First().AffectsVersions;
            Assert.Equal(2, versions.Count());
            Assert.True(versions.Any(v => v.Name == "1.0"));
            Assert.True(versions.Any(v => v.Name == "2.0"));

            var v1 = versions.First(v => v.Name == "1.0");
            Assert.False(v1.IsArchived);
            Assert.True(v1.IsReleased);
            Assert.Equal(new DateTime(2011, 9, 01).ToUniversalTime(), v1.ReleasedDate);
        }

        [Fact]
        public void RetrievesFixVersions()
        {
            var issues = from i in _jira.Issues
                         where i.Key == "TST-1"
                         select i;

            var versions = issues.First().FixVersions;
            Assert.Equal(2, versions.Count());
            Assert.True(versions.Any(v => v.Name == "1.0"));
            Assert.True(versions.Any(v => v.Name == "3.0"));

            var v1 = versions.First(v => v.Name == "1.0");
            Assert.False(v1.IsArchived);
            Assert.True(v1.IsReleased);
            Assert.Equal(new DateTime(2011, 9, 01).ToUniversalTime(), v1.ReleasedDate);
        }

        [Fact]
        public void RetrievesAllVersions()
        {
            var versions = _jira.GetVersions("TST");
            Assert.Equal(3, versions.Count());
            Assert.True(versions.Any(v => v.Name == "1.0"));
            Assert.True(versions.Any(v => v.Name == "2.0"));
            Assert.True(versions.Any(v => v.Name == "3.0"));
        }

        [Fact]
        public void UpdateVersionsOfIssue()
        {
            var summaryValue = "Test issue with versions " + _random.Next(int.MaxValue);

            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = summaryValue
            };

            issue = _jira.CreateIssue(issue);

            var versions = _jira.GetVersions("TST");

            issue.FixVersions.Add(versions.First(v => v.Name == "2.0"));
            issue.AffectsVersions.Add(versions.First(v => v.Name == "1.0"));

            var newIssue = _jira.UpdateIssue(issue);

            Assert.Equal(1, newIssue.FixVersions.Count);
            Assert.Equal("2.0", newIssue.FixVersions[0].Name);

            Assert.Equal(1, newIssue.AffectsVersions.Count);
            Assert.Equal("1.0", newIssue.AffectsVersions[0].Name);

        }

    }
}
