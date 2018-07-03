using System;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueCustomFieldTest : BaseIntegrationTest
    {
        [Fact]
        public void CanHandleCustomFieldWithoutSerializerThatIsArrayOfObjects()
        {
            var jira = Jira.CreateRestClient(new TraceReplayer("Trace_CustomFieldArrayOfObjects.txt"));
            var issue = jira.Issues.GetIssuesFromJqlAsync("foo").Result.Single();

            Assert.True(issue["Watchers"].Value.Length > 0);
        }

        [Fact]
        public void CanHandleCustomFieldSetToEmptyArrayByDefaultFromServer()
        {
            // See: https://bitbucket.org/farmas/atlassian.net-sdk/issues/372
            var jira = Jira.CreateRestClient(new TraceReplayer("Trace_CustomFieldEmptyArray.txt"));
            var issue = jira.Issues.GetIssueAsync("GIT-103").Result;

            issue.Summary = "Some change";
            issue.SaveChanges();

            Assert.NotNull(issue);
        }

        [Fact]
        public void AddAndReadCustomFieldById()
        {
            var summaryValue = "Test issue with custom text" + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.CustomFields.AddById("customfield_10000", "My Sample Text");
            issue.SaveChanges();

            var newIssue = _jira.Issues.GetIssueAsync(issue.Key.Value).Result;
            Assert.Equal("My Sample Text", newIssue.CustomFields.First(f => f.Id.Equals("customfield_10000")).Values.First());
        }

        [Fact]
        public void CreateIssueWithCascadingSelectFieldWithOnlyParentOptionSet()
        {
            var summaryValue = "Test issue with cascading select" + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            // Add cascading select with only parent set.
            issue.CustomFields.AddCascadingSelectField("Custom Cascading Select Field", "Option3");
            issue.SaveChanges();

            var newIssue = _jira.Issues.GetIssueAsync(issue.Key.Value).Result;

            var cascadingSelect = newIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
            Assert.Equal("Option3", cascadingSelect.ParentOption);
            Assert.Null(cascadingSelect.ChildOption);
            Assert.Equal("Custom Cascading Select Field", cascadingSelect.Name);
        }

        [Fact]
        public void CreateAndQueryIssueWithComplexCustomFields()
        {
            var dateTime = new DateTime(2016, 11, 11, 11, 11, 0);
            var dateTimeStr = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
            dateTimeStr = dateTimeStr.Remove(dateTimeStr.LastIndexOf(":"), 1);

            var summaryValue = "Test issue with lots of custom fields (Created)" + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue["Custom Text Field"] = "My new value";
            issue["Custom Date Field"] = "2015-10-03";
            issue["Custom DateTime Field"] = dateTimeStr;
            issue["Custom User Field"] = "admin";
            issue["Custom Select Field"] = "Blue";
            issue["Custom Group Field"] = "jira-users";
            issue["Custom Project Field"] = "TST";
            issue["Custom Version Field"] = "1.0";
            issue["Custom Radio Field"] = "option1";
            issue["Custom Number Field"] = "12.34";
            issue.CustomFields.AddArray("Custom Labels Field", "label1", "label2");
            issue.CustomFields.AddArray("Custom Multi Group Field", "jira-developers", "jira-users");
            issue.CustomFields.AddArray("Custom Multi Select Field", "option1", "option2");
            issue.CustomFields.AddArray("Custom Multi User Field", "admin", "test");
            issue.CustomFields.AddArray("Custom Checkboxes Field", "option1", "option2");
            issue.CustomFields.AddArray("Custom Multi Version Field", "2.0", "3.0");
            issue.CustomFields.AddCascadingSelectField("Custom Cascading Select Field", "Option2", "Option2.2");

            issue.SaveChanges();

            var newIssue = _jira.Issues.GetIssueAsync(issue.Key.Value).Result;

            Assert.Equal("My new value", newIssue["Custom Text Field"]);
            Assert.Equal("2015-10-03", newIssue["Custom Date Field"]);
            Assert.Equal("admin", newIssue["Custom User Field"]);
            Assert.Equal("Blue", newIssue["Custom Select Field"]);
            Assert.Equal("jira-users", newIssue["Custom Group Field"]);
            Assert.Equal("TST", newIssue["Custom Project Field"]);
            Assert.Equal("1.0", newIssue["Custom Version Field"]);
            Assert.Equal("option1", newIssue["Custom Radio Field"]);
            Assert.Equal("12.34", newIssue["Custom Number Field"]);

            var serverDate = DateTime.Parse(newIssue["Custom DateTime Field"].Value);
            Assert.Equal(dateTime, serverDate);

            Assert.Equal(new string[2] { "label1", "label2" }, newIssue.CustomFields["Custom Labels Field"].Values);
            Assert.Equal(new string[2] { "jira-developers", "jira-users" }, newIssue.CustomFields["Custom Multi Group Field"].Values);
            Assert.Equal(new string[2] { "option1", "option2" }, newIssue.CustomFields["Custom Multi Select Field"].Values);
            Assert.Equal(new string[2] { "admin", "test" }, newIssue.CustomFields["Custom Multi User Field"].Values);
            Assert.Equal(new string[2] { "option1", "option2" }, newIssue.CustomFields["Custom Checkboxes Field"].Values);
            Assert.Equal(new string[2] { "2.0", "3.0" }, newIssue.CustomFields["Custom Multi Version Field"].Values);

            var cascadingSelect = newIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
            Assert.Equal("Option2", cascadingSelect.ParentOption);
            Assert.Equal("Option2.2", cascadingSelect.ChildOption);
            Assert.Equal("Custom Cascading Select Field", cascadingSelect.Name);
        }

        [Fact]
        public void CanClearValueOfCustomField()
        {
            var summaryValue = "Test issue " + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue["Custom Text Field"] = "My new value";
            issue["Custom Date Field"] = "2015-10-03";
            issue.SaveChanges();

            var newIssue = _jira.Issues.GetIssueAsync(issue.Key.Value).Result;
            Assert.Equal("My new value", newIssue["Custom Text Field"]);
            Assert.Equal("2015-10-03", newIssue["Custom Date Field"]);
            newIssue["Custom Text Field"] = null;
            newIssue["Custom Date Field"] = null;
            newIssue.SaveChanges();

            var updatedIssue = _jira.Issues.GetIssueAsync(issue.Key.Value).Result;

            Assert.Null(updatedIssue["Custom Text Field"]);
            Assert.Null(updatedIssue["Custom Date Field"]);
        }

        [Fact]
        public void CreateAndUpdateIssueWithComplexCustomFields()
        {
            var dateTime = new DateTime(2016, 11, 11, 11, 11, 0);
            var dateTimeStr = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
            dateTimeStr = dateTimeStr.Remove(dateTimeStr.LastIndexOf(":"), 1);

            var summaryValue = "Test issue with lots of custom fields (Created)" + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            var newIssue = _jira.Issues.GetIssueAsync(issue.Key.Value).Result;

            newIssue["Custom Text Field"] = "My new value";
            newIssue["Custom Date Field"] = "2015-10-03";
            newIssue["Custom DateTime Field"] = dateTimeStr;
            newIssue["Custom User Field"] = "admin";
            newIssue["Custom Select Field"] = "Blue";
            newIssue["Custom Group Field"] = "jira-users";
            newIssue["Custom Project Field"] = "TST";
            newIssue["Custom Version Field"] = "1.0";
            newIssue["Custom Radio Field"] = "option1";
            newIssue["Custom Number Field"] = "1234";
            newIssue.CustomFields.AddArray("Custom Labels Field", "label1", "label2");
            newIssue.CustomFields.AddArray("Custom Multi Group Field", "jira-developers", "jira-users");
            newIssue.CustomFields.AddArray("Custom Multi Select Field", "option1", "option2");
            newIssue.CustomFields.AddArray("Custom Multi User Field", "admin", "test");
            newIssue.CustomFields.AddArray("Custom Checkboxes Field", "option1", "option2");
            newIssue.CustomFields.AddArray("Custom Multi Version Field", "2.0", "3.0");
            newIssue.CustomFields.AddCascadingSelectField("Custom Cascading Select Field", "Option2", "Option2.2");

            newIssue.SaveChanges();

            var updatedIssue = _jira.Issues.GetIssueAsync(issue.Key.Value).Result;

            Assert.Equal("My new value", updatedIssue["Custom Text Field"]);
            Assert.Equal("2015-10-03", updatedIssue["Custom Date Field"]);
            Assert.Equal("admin", updatedIssue["Custom User Field"]);
            Assert.Equal("Blue", updatedIssue["Custom Select Field"]);
            Assert.Equal("jira-users", updatedIssue["Custom Group Field"]);
            Assert.Equal("TST", updatedIssue["Custom Project Field"]);
            Assert.Equal("1.0", updatedIssue["Custom Version Field"]);
            Assert.Equal("option1", updatedIssue["Custom Radio Field"]);
            Assert.Equal("1234", updatedIssue["Custom Number Field"]);

            var serverDate = DateTime.Parse(updatedIssue["Custom DateTime Field"].Value);
            Assert.Equal(dateTime, serverDate);

            Assert.Equal(new string[2] { "label1", "label2" }, updatedIssue.CustomFields["Custom Labels Field"].Values);
            Assert.Equal(new string[2] { "jira-developers", "jira-users" }, updatedIssue.CustomFields["Custom Multi Group Field"].Values);
            Assert.Equal(new string[2] { "option1", "option2" }, updatedIssue.CustomFields["Custom Multi Select Field"].Values);
            Assert.Equal(new string[2] { "admin", "test" }, updatedIssue.CustomFields["Custom Multi User Field"].Values);
            Assert.Equal(new string[2] { "option1", "option2" }, updatedIssue.CustomFields["Custom Checkboxes Field"].Values);
            Assert.Equal(new string[2] { "2.0", "3.0" }, updatedIssue.CustomFields["Custom Multi Version Field"].Values);

            var cascadingSelect = updatedIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
            Assert.Equal("Option2", cascadingSelect.ParentOption);
            Assert.Equal("Option2.2", cascadingSelect.ChildOption);
            Assert.Equal("Custom Cascading Select Field", cascadingSelect.Name);
        }
        [Fact]
        public void CreateAndQuerySprintName()
        {
            var issue = new Issue(_jira, "SCRUM")
            {
                Type = "Bug",
                Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
                Assignee = "admin"
            };
            // Set the sprint by id
            issue["Sprint"] = "1";
            issue.SaveChanges();

            // Get the sprint by name
            var newIssue = _jira.Issues.GetIssueAsync(issue.Key.Value).Result;
            Assert.Equal("Sprint 1", newIssue["Sprint"]);
        }
        [Fact]
        public void UpdateAndQuerySprintName()
        {
            var issue = new Issue(_jira, "SCRUM")
            {
                Type = "Bug",
                Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
                Assignee = "admin"
            };
            issue.SaveChanges();
            Assert.Null(issue["Sprint"]);

            // Set the sprint by id
            issue["Sprint"] = "1";
            issue.SaveChanges();

            // Get the sprint by name
            var newIssue = _jira.Issues.GetIssueAsync(issue.Key.Value).Result;
            Assert.Equal("Sprint 1", newIssue["Sprint"]);
        }
        [Fact]
        public void CanUpdateIssueWithoutModifyingCustomFields()
        {
            var issue = new Issue(_jira, "SCRUM")
            {
                Type = "Bug",
                Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
                Assignee = "admin"
            };
            issue["Sprint"] = "1";
            issue.SaveChanges();
            Assert.Equal("Sprint 1", issue["Sprint"]);

            issue.Summary += " (Updated)";
            issue.SaveChanges();
            Assert.Equal("Sprint 1", issue["Sprint"]);
        }
        [Fact]
        public void ThrowsErrorWhenSettingSprintByName()
        {
            var issue = new Issue(_jira, "SCRUM")
            {
                Type = "Bug",
                Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
                Assignee = "admin"
            };

            // Set the sprint by name
            issue["Sprint"] = "Sprint 1";

            try
            {
                issue.SaveChanges();
                throw new Exception("Method did not throw exception");
            }
            catch (AggregateException ex)
            {
                Assert.Contains("Number value expected as the Sprint id", ex.Flatten().InnerException.Message);
            }
        }

        public class IssueFieldMetadataCustomFieldOption
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("self")]
            public string Self { get; set; }
        }
    }
}
