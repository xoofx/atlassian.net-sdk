using Newtonsoft.Json;
using System.Linq;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueCustomFieldTest : BaseIntegrationTest
    {
#if !SOAP
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

            var newIssue = _jira.GetIssue(issue.Key.Value);
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

            var newIssue = _jira.GetIssue(issue.Key.Value);

            var cascadingSelect = newIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
            Assert.Equal(cascadingSelect.ParentOption, "Option3");
            Assert.Null(cascadingSelect.ChildOption);
            Assert.Equal(cascadingSelect.Name, "Custom Cascading Select Field");
        }

        [Fact]
        public void CreateAndQueryIssueWithComplexCustomFields()
        {
            var summaryValue = "Test issue with lots of custom fields (Created)" + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue["Custom Text Field"] = "My new value";
            issue["Custom Date Field"] = "2015-10-03";
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

            var newIssue = _jira.GetIssue(issue.Key.Value);

            Assert.Equal("My new value", newIssue["Custom Text Field"]);
            Assert.Equal("2015-10-03", newIssue["Custom Date Field"]);
            Assert.Equal("admin", newIssue["Custom User Field"]);
            Assert.Equal("Blue", newIssue["Custom Select Field"]);
            Assert.Equal("jira-users", newIssue["Custom Group Field"]);
            Assert.Equal("TST", newIssue["Custom Project Field"]);
            Assert.Equal("1.0", newIssue["Custom Version Field"]);
            Assert.Equal("option1", newIssue["Custom Radio Field"]);
            Assert.Equal("12.34", newIssue["Custom Number Field"]);

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
        public void CreateAndUpdateIssueWithComplexCustomFields()
        {
            var summaryValue = "Test issue with lots of custom fields (Created)" + _random.Next(int.MaxValue);
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };

            issue.SaveChanges();

            var newIssue = _jira.GetIssue(issue.Key.Value);

            newIssue["Custom Text Field"] = "My new value";
            newIssue["Custom Date Field"] = "2015-10-03";
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

            var updatedIssue = _jira.GetIssue(issue.Key.Value);

            Assert.Equal("My new value", updatedIssue["Custom Text Field"]);
            Assert.Equal("2015-10-03", updatedIssue["Custom Date Field"]);
            Assert.Equal("admin", updatedIssue["Custom User Field"]);
            Assert.Equal("Blue", updatedIssue["Custom Select Field"]);
            Assert.Equal("jira-users", updatedIssue["Custom Group Field"]);
            Assert.Equal("TST", updatedIssue["Custom Project Field"]);
            Assert.Equal("1.0", updatedIssue["Custom Version Field"]);
            Assert.Equal("option1", updatedIssue["Custom Radio Field"]);
            Assert.Equal("1234", updatedIssue["Custom Number Field"]);

            Assert.Equal(new string[2] { "label1", "label2" }, updatedIssue.CustomFields["Custom Labels Field"].Values);
            Assert.Equal(new string[2] { "jira-developers", "jira-users" }, updatedIssue.CustomFields["Custom Multi Group Field"].Values);
            Assert.Equal(new string[2] { "option1", "option2" }, updatedIssue.CustomFields["Custom Multi Select Field"].Values);
            Assert.Equal(new string[2] { "admin", "test" }, updatedIssue.CustomFields["Custom Multi User Field"].Values);
            Assert.Equal(new string[2] { "option1", "option2" }, updatedIssue.CustomFields["Custom Checkboxes Field"].Values);
            Assert.Equal(new string[2] { "2.0", "3.0" }, updatedIssue.CustomFields["Custom Multi Version Field"].Values);

            var cascadingSelect = updatedIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
            Assert.Equal(cascadingSelect.ParentOption, "Option2");
            Assert.Equal(cascadingSelect.ChildOption, "Option2.2");
            Assert.Equal(cascadingSelect.Name, "Custom Cascading Select Field");
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
#endif
    }
}
