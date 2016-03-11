using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueCustomFieldTest : BaseIntegrationTest
    {
#if !SOAP
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

        [Fact]
        public async Task TestCustomFieldOptions()
        {
            // prepare
            Issue iss = _jira.GetIssue("TST-1");

            // exercise
            var issueFields = await iss.GetIssueFieldsEditMetadataAsync();

            //assert: IssueFieldEditMetadata of issue
            Assert.True(issueFields.Count() >= 34);
            IssueFieldEditMetadata customRadioField = issueFields["Custom Radio Field"];
            Assert.Equal("Custom Radio Field", customRadioField.Name);
            Assert.False(customRadioField.IsRequired);
            Assert.Contains(IssueFieldEditMetadataOperation.SET, customRadioField.Operations);
            Assert.Equal(1, customRadioField.Operations.Count);
            Assert.Equal("option", customRadioField.Schema.Type);
            Assert.Equal("com.atlassian.jira.plugin.system.customfieldtypes:radiobuttons", customRadioField.Schema.Custom);
            Assert.Equal(10307, customRadioField.Schema.CustomId);

            // assert: allowed values
            // warning : AllowedValues on IssueFieldEditMetadata could be various kind of objects. One can determine the kind of object
            // by looking at it's properties. Issue TST-1 and it's field "Custom Radio Field" used for this test has
            // AllowedValues elements are objects of type CustomFieldOption
            var options = customRadioField.AllowedValuesAs<IssueFieldMetadataCustomFieldOption>();
            IssueFieldMetadataCustomFieldOption option1 = options.FirstOrDefault(x => x.Value == "option1");
            IssueFieldMetadataCustomFieldOption option2 = options.FirstOrDefault(x => x.Value == "option2");
            IssueFieldMetadataCustomFieldOption option3 = options.FirstOrDefault(x => x.Value == "option3");
            AssertCustomFieldOption(option1, 10103, "option1", @".*/rest/api/2/customFieldOption/10103");
            AssertCustomFieldOption(option2, 10104, "option2", @".*/rest/api/2/customFieldOption/10104");
            AssertCustomFieldOption(option3, 10105, "option3", @".*/rest/api/2/customFieldOption/10105");
        }

        private void AssertCustomFieldOption(IssueFieldMetadataCustomFieldOption option, int id, string value, string selfRegex)
        {
            Assert.NotNull(option);
            Assert.Equal(id, option.Id);
            Assert.Equal(value, option.Value);
            Regex regex = new Regex(selfRegex);
            Assert.Equal(true, regex.Match(option.Self).Success);
        }
#endif
    }
}
