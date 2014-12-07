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
            _random = new Random();
        }

        [Fact]
        void WithAccessTokenInsteadOfUserAndPassword()
        {
            // get access token for user
            var accessToken = _jira.GetAccessToken();

            // create a new jira instance using access token only
            var jiraAccessToken = new Jira("http://localhost:2990/jira", accessToken);

            // create and query issues
            var summaryValue = "Test Summary from JIRA with access token " + _random.Next(int.MaxValue);
            var issue = new Issue(jiraAccessToken, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };
            issue.SaveChanges();

            var issues = (from i in jiraAccessToken.Issues
                          where i.Key == issue.Key
                          select i).ToArray();

            Assert.Equal(1, issues.Count());
        }

        [Fact]
        void Transition_ResolveIssue()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            issue.WorkflowTransition(WorkflowActions.Resolve);

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Fixed", issue.Resolution.Name);
        }

        [Fact]
        public void GetResolutionDate()
        {
            // Arrange
            var issue = _jira.CreateIssue("TST");
            var currentDate = DateTime.Now;
            issue.Summary = "Issue to resolve " + Guid.NewGuid().ToString();
            issue.Type = "Bug";

            // Act, Assert: Returns null for unsaved issue.
            Assert.Null(issue.GetResolutionDate());

            // Act, Assert: Returns null for saved unresolved issue.
            issue.SaveChanges();
            Assert.Null(issue.GetResolutionDate());

            // Act, Assert: returns date for saved resolved issue.
            issue.WorkflowTransition(WorkflowActions.Resolve);
            var date = issue.GetResolutionDate();
            Assert.NotNull(date);
            Assert.Equal(date.Value.Year, currentDate.Year);
        }

        [Fact]
        void Transition_ResolveIssue_AsWontFix()
        {
            var issue = _jira.CreateIssue("TST");
            issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
            issue.Type = "Bug";
            issue.SaveChanges();

            issue.Resolution = "Won't Fix";
            issue.WorkflowTransition(WorkflowActions.Resolve);

            Assert.Equal("Resolved", issue.Status.Name);
            Assert.Equal("Won't Fix", issue.Resolution.Name);
        }

        [Fact]
        public void GetFilters()
        {
            var filters = _jira.GetFilters();

            Assert.Equal(1, filters.Count());
            Assert.Equal("One Issue Filter", filters.First().Name);
        }

        [Fact]
        public void GetIssuesFromFilter()
        {
            var issues = _jira.GetIssuesFromFilter("One Issue Filter");

            Assert.Equal(1, issues.Count());
            Assert.Equal("TST-1", issues.First().Key.Value);
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
        public void RetrieveNamedEntities()
        {
            var issue = _jira.GetIssue("TST-1");

            Assert.Equal("Bug", issue.Type.Name);
            Assert.Equal("Major", issue.Priority.Name);
            Assert.Equal("Open", issue.Status.Name);
            Assert.Null(issue.Resolution);
        }

        [Fact]
        public void CreateAndQueryIssueWithMinimumFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };

            issue.SaveChanges();

            var issues = (from i in _jira.Issues
                                where i.Key == issue.Key
                                select i).ToArray();

            Assert.Equal(1, issues.Count());

            Assert.Equal(summaryValue, issues[0].Summary);
            Assert.Equal("TST", issues[0].Project);
            Assert.Equal("1", issues[0].Type.Id);
        }


        [Fact]
        public void CreateAndQueryIssueWithAllFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = _jira.CreateIssue("TST");
            issue.AffectsVersions.Add("1.0");
            issue.Assignee = "admin";
            issue.Components.Add("Server");
            issue["Custom Text Field"] = "Test Value";  // custom field
            issue.Description = "Test Description";
            issue.DueDate = new DateTime(2011, 12, 12);
            issue.Environment = "Test Environment";
            issue.FixVersions.Add("2.0");
            issue.Priority = "Major";
            issue.Reporter = "admin";
            issue.Summary = summaryValue;
            issue.Type = "1";

            issue.SaveChanges();

            var queriedIssues = (from i in _jira.Issues
                          where i.Key == issue.Key
                          select i).ToArray();

            Assert.Equal(summaryValue, queriedIssues[0].Summary);
            Assert.NotNull(queriedIssues[0].JiraIdentifier);
        }

        [Fact]
        public void UpdateIssueType()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
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

            // Note: Dates returned from JIRA are UTC
            Assert.Equal(new DateTime(2011, 10, 10).ToUniversalTime(), newServerIssue.DueDate);
        }

        [Fact]
        public void UploadAndDownloadOfAttachments()
        {
            var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };

            // create an issue, verify no attachments
            issue.SaveChanges();
            Assert.Equal(0, issue.GetAttachments().Count);

            // upload multiple attachments
            File.WriteAllText("testfile1.txt", "Test File Content 1");
            File.WriteAllText("testfile2.txt", "Test File Content 2");
            issue.AddAttachment("testfile1.txt", "testfile2.txt");

            var attachments = issue.GetAttachments();
            Assert.Equal(2, attachments.Count);
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile1.txt")), "'testfile1.txt' was not downloaded from server");
            Assert.True(attachments.Any(a => a.FileName.Equals("testfile2.txt")), "'testfile2.txt' was not downloaded from server");

            // download an attachment
            var tempFile = Path.GetTempFileName();
            attachments.First(a => a.FileName.Equals("testfile1.txt")).Download(tempFile);
            Assert.Equal("Test File Content 1", File.ReadAllText(tempFile));
        }

        [Fact]
        public void AddAndGetComments()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };

            // create an issue, verify no comments
            issue.SaveChanges();
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
            (new Issue(_jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber }).SaveChanges();
            (new Issue(_jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber }).SaveChanges(); 

            //set maximum issues and query
            _jira.MaxIssuesPerRequest = 1;
            var issues = from i in _jira.Issues
                         where i.Summary == randomNumber.ToString()
                         select i;

            Assert.Equal(1, issues.Count());

        }

        [Fact]
        public void QueryIssueWithCustomDateField()
        {
            var issue = (from i in _jira.Issues
                         where i["Custom Date Field"] <= new DateTime(2012,4,1)
                         select i).First();

            Assert.Equal("Sample bug in Test Project", issue.Summary);
        }

        [Fact]
        public void QueryIssuesWithTakeExpression()
        {
            // create 2 issues with same summary
            var randomNumber = _random.Next(int.MaxValue);
            (new Issue(_jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber }).SaveChanges();
            (new Issue(_jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber }).SaveChanges();

            // query with take method to only return 1
            var issues = (from i in _jira.Issues
                         where i.Summary == randomNumber.ToString()
                         select i).Take(1);

            Assert.Equal(1, issues.Count());
        }

        [Fact]
        public void GetIssueTypes()
        {
            var issueTypes = _jira.GetIssueTypes("TST");

            Assert.Equal(4, issueTypes.Count());
            Assert.True(issueTypes.Any(i => i.Name == "Bug"));
        }

        [Fact]
        public void GetIssuePriorities()
        {
            var priorities = _jira.GetIssuePriorities();

            Assert.True(priorities.Any(i => i.Name == "Blocker"));
        }

        [Fact]
        public void GetIssueResolutions()
        {
            var resolutions = _jira.GetIssueResolutions();

            Assert.True(resolutions.Any(i => i.Name == "Fixed"));
        }

        [Fact]
        public void GetIssueStatuses()
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
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = "Serialization nastiness"
            };

            issue.Description = File.ReadAllText("LongIssueDescription.txt"); 
            issue.SaveChanges();

            Assert.Contains("Second stack trace:", issue.Description);
        }

        [Fact]
        public void GetCustomFields()
        {
            var fields = _jira.GetCustomFields();
            Assert.Equal(4, fields.Count());
        }

        [Fact]
        public void GetProjectVersions()
        {
            var versions = _jira.GetProjectVersions("TST");
            Assert.Equal(3, versions.Count());
        }

        [Fact]
        public void GetProjectComponents()
        {
            var components = _jira.GetProjectComponents("TST");
            Assert.Equal(2, components.Count());
        }

        [Fact]
        public void UpdateAssignee()
        {
            var summaryValue = "Test issue with assignee (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
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
        public void UpdateVersions()
        {
            var summaryValue = "Test issue with versions (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };

            issue.SaveChanges();

            issue.AffectsVersions.Add("1.0");
            issue.AffectsVersions.Add("2.0");

            issue.FixVersions.Add("3.0");
            issue.FixVersions.Add("2.0");

            issue.SaveChanges();

            Assert.Equal(2, issue.AffectsVersions.Count);
            Assert.True(issue.AffectsVersions.Any(v => v.Name == "1.0"));
            Assert.True(issue.AffectsVersions.Any(v => v.Name == "2.0"));

            Assert.Equal(2, issue.FixVersions.Count);
            Assert.True(issue.FixVersions.Any(v => v.Name == "2.0"));
            Assert.True(issue.FixVersions.Any(v => v.Name == "3.0"));
        }

        [Fact]
        public void CreateAndQueryIssueWithSubTask()
        {
            var parentTask = _jira.CreateIssue("TST");
            parentTask.Type = "1";
            parentTask.Summary = "Test issue with SubTask" + _random.Next(int.MaxValue);
            parentTask.SaveChanges();

            var subTask = _jira.CreateIssue("TST", parentTask.Key.Value);
            subTask.Type = "5"; // SubTask issue type.
            subTask.Summary = "Test SubTask" + _random.Next(int.MaxValue);
            subTask.SaveChanges();

            Assert.False(parentTask.Type.IsSubTask);
            Assert.True(subTask.Type.IsSubTask);
        }

        [Fact]
        public void CreateAndQueryIssueWithVersions()
        {
            var summaryValue = "Test issue with versions (Created)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };

            issue.AffectsVersions.Add("1.0");
            issue.AffectsVersions.Add("2.0");

            issue.FixVersions.Add("3.0");
            issue.FixVersions.Add("2.0");

            issue.SaveChanges();

            var newIssue = (from i in _jira.Issues
                            where i.AffectsVersions == "1.0" && i.AffectsVersions == "2.0" 
                                    && i.FixVersions == "2.0" && i.FixVersions == "3.0"
                            select i).First();

            Assert.Equal(2, newIssue.AffectsVersions.Count);
            Assert.True(newIssue.AffectsVersions.Any(v => v.Name == "1.0"));
            Assert.True(newIssue.AffectsVersions.Any(v => v.Name == "2.0"));

            Assert.Equal(2, newIssue.FixVersions.Count);
            Assert.True(newIssue.FixVersions.Any(v => v.Name == "2.0"));
            Assert.True(newIssue.FixVersions.Any(v => v.Name == "3.0"));
        }

        [Fact]
        public void CreateAndQueryIssueWithComponents()
        {
            var summaryValue = "Test issue with components (Created)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };

            issue.Components.Add("Server");
            issue.Components.Add("Client");

            issue.SaveChanges();

            var newIssue = (from i in _jira.Issues
                            where i.Summary == summaryValue && i.Components == "Server" && i.Components == "Client"
                            select i).First();

            Assert.Equal(2, newIssue.Components.Count);
            Assert.True(newIssue.Components.Any(c => c.Name == "Server"));
            Assert.True(newIssue.Components.Any(c => c.Name == "Client"));
        }

        [Fact]
        public void DeleteIssue()
        {
            // Create issue and verify it is found in server.
            var issue = _jira.CreateIssue("TST");
            issue.Type = "1";
            issue.Summary = String.Format("Issue to delete ({0})", _random.Next(int.MaxValue));
            issue.SaveChanges();
            Assert.True(_jira.Issues.Where(i => i.Key == issue.Key).Any(), "Expected issue in server");

            // Delete issue and verify it is no longer found.
            _jira.DeleteIssue(issue);
            Assert.Throws<System.ServiceModel.FaultException>(() => _jira.GetIssue(issue.Key.Value));
        }

        [Fact]
        public void UpdateComponents()
        {
            var summaryValue = "Test issue with components (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };

            issue.SaveChanges();

            issue.Components.Add("Server");
            issue.Components.Add("Client");

            issue.SaveChanges();

            Assert.Equal(2, issue.Components.Count);
            Assert.True(issue.Components.Any(c => c.Name == "Server"));
            Assert.True(issue.Components.Any(c => c.Name == "Client"));
        }

        [Fact]
        public void AddLabelsToIssue()
        {
            var summaryValue = "Test issue with labels (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };

            issue.SaveChanges();

            issue.AddLabels("label1", "label2");
        }

        [Fact]
        public void CreateAndQueryIssueWithCustomField()
        {
            var summaryValue = "Test issue with custom field (Created)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };
            issue["Custom Text Field"] = "My new value";

            issue.SaveChanges();

            var newIssue = (from i in _jira.Issues
                            where i.Summary == summaryValue && i["Custom Text Field"] == "My new value"
                            select i).First();

            Assert.Equal("My new value", newIssue["Custom Text Field"]);
        }

        [Fact]
        public void UpdateIssueWithCustomField()
        {
            var summaryValue = "Test issue with custom field (Updated)" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };
            issue["Custom Text Field"] = "My new value";

            issue.SaveChanges();

            issue["Custom Text Field"] = "My updated value";
            issue.SaveChanges();

            Assert.Equal("My updated value", issue["Custom Text Field"]);
        }

        [Fact]
        public void AddAndGetWorklogs()
        {
            var summaryValue = "Test issue with work logs" + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue
            };
            issue.SaveChanges();

            issue.AddWorklog("1d");
            issue.AddWorklog("1h", WorklogStrategy.RetainRemainingEstimate);
            issue.AddWorklog("1m", WorklogStrategy.NewRemainingEstimate, "2d");

            issue.AddWorklog(new Worklog("2d", new DateTime(2012, 1, 1), "comment"));

            var logs = issue.GetWorklogs();
            Assert.Equal(4, logs.Count);
            Assert.Equal("comment", logs.ElementAt(3).Comment);
            Assert.Equal(new DateTime(2012, 1, 1), logs.ElementAt(3).StartDate);

        }

        [Fact]
        public void GetProjects()
        {
            Assert.Equal(1, _jira.GetProjects().Count());
        }

        [Fact]
        public void AddIssueAsSubtask() 
        {
            var summaryValue = "Test issue as subtask " + _random.Next(int.MaxValue);

            var issue = new Issue(_jira, "TST", "TST-1")
            {
                Type = "5", //subtask
                Summary = summaryValue
            };
            issue.SaveChanges();

            var subtasks = _jira.GetIssuesFromJql("project = TST and parent = TST-1");

            Assert.True(subtasks.Any(s => s.Summary.Equals(summaryValue)), 
                String.Format("'{0}' was not found as a sub-task of TST-1", summaryValue));
        }

        [Fact]
        public void DeleteWorklog()
        {
            var summary = "Test issue with worklogs" + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summary
            };
            issue.SaveChanges();

            var worklog = issue.AddWorklog("1h");
            Assert.Equal(1, issue.GetWorklogs().Count);

            issue.DeleteWorklog(worklog);
            Assert.Equal(0, issue.GetWorklogs().Count);
        }
    }
}
