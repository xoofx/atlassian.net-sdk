using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueFieldMetadataTest
    {
        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TestNonCustomFieldOption(Jira jira)
        {
            // prepare
            Issue iss = jira.Issues.GetIssueAsync("TST-1").Result;

            // exercise
            var issueFields = await iss.GetIssueFieldsEditMetadataAsync();

            IssueFieldEditMetadata customRadioField = issueFields["Component/s"];
            Assert.False(customRadioField.IsCustom);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TestCustomFieldOptions(Jira jira)
        {
            // prepare
            Issue iss = jira.Issues.GetIssueAsync("TST-1").Result;

            // exercise
            var issueFields = await iss.GetIssueFieldsEditMetadataAsync();

            //assert: IssueFieldEditMetadata of issue
            Assert.True(issueFields.Count() >= 34);
            IssueFieldEditMetadata customRadioField = issueFields["Custom Radio Field"];
            Assert.True(customRadioField.IsCustom);
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
            var options = customRadioField.AllowedValuesAs<IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption>();
            IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption option1 = options.FirstOrDefault(x => x.Value == "option1");
            IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption option2 = options.FirstOrDefault(x => x.Value == "option2");
            IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption option3 = options.FirstOrDefault(x => x.Value == "option3");
            AssertCustomFieldOption(option1, 10103, "option1", @".*/rest/api/2/customFieldOption/10103");
            AssertCustomFieldOption(option2, 10104, "option2", @".*/rest/api/2/customFieldOption/10104");
            AssertCustomFieldOption(option3, 10105, "option3", @".*/rest/api/2/customFieldOption/10105");
        }

        private void AssertCustomFieldOption(IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption option, int id, string value, string selfRegex)
        {
            Assert.NotNull(option);
            Assert.Equal(id, option.Id);
            Assert.Equal(value, option.Value);
            Regex regex = new Regex(selfRegex);
            Assert.True(regex.Match(option.Self).Success);
        }
    }
}
