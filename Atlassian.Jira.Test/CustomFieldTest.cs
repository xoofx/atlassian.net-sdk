using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;
using Xunit;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Atlassian.Jira.Test
{
    public class CustomFieldTest
    {
        [Fact]
        public void Name_ShouldRetriveValueFromRemote()
        {
            //arrange
            var jira = TestableJira.Create();
            var customField = new CustomField(new RemoteField() { id = "123", name = "CustomField" });
            jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
                .Returns(Task.FromResult(Enumerable.Repeat<CustomField>(customField, 1)));

            var issue = new RemoteIssue()
            {
                project = "projectKey",
                key = "issueKey",
                customFieldValues = new RemoteCustomFieldValue[]{
                                new RemoteCustomFieldValue(){
                                    customfieldId = "123",
                                    values = new string[] {"abc"}
                                }
                            }
            }.ToLocal(jira);

            //assert
            Assert.Equal("CustomField", issue.CustomFields[0].Name);
        }

        [Fact]
        public void WhenAddingArrayOfValues_CanSerializeAsStringArrayWhenNoSerializerIsFound()
        {
            // arrange issue
            var jira = TestableJira.Create();
            var remoteField = new RemoteField() { id = "remotefield_id", Schema = new RemoteFieldSchema() { Custom = "remotefield_type" }, IsCustomField = true, name = "Custom Field" };
            var customField = new CustomField(remoteField);
            var issue = new RemoteIssue() { project = "projectKey", key = "issueKey" }.ToLocal(jira);

            jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
                .Returns(Task.FromResult(Enumerable.Repeat(customField, 1)));

            issue.CustomFields.AddArray("Custom Field", "val1", "val2");

            // arrange serialization
            var remoteIssue = issue.ToRemote();
            var converter = new RemoteIssueJsonConverter(new List<RemoteField> { remoteField }, new Dictionary<string, ICustomFieldValueSerializer>());
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            serializerSettings.Converters.Add(converter);
            var issueWrapper = new RemoteIssueWrapper(remoteIssue);

            // act
            var issueJson = JsonConvert.SerializeObject(issueWrapper, serializerSettings);

            // assert
            var jObject = JObject.Parse(issueJson);
            var remoteFieldValue = jObject["fields"]["remotefield_id"];
            var valueArray = remoteFieldValue.ToObject<string[]>();
            Assert.Equal(2, valueArray.Length);
            Assert.Contains("val1", valueArray);
            Assert.Contains("val2", valueArray);
        }

        [Fact]
        public void CanDeserializeArrayOfStrings_WhenCustomFieldValueIsArrayAndNoSerializerIsRegistered()
        {
            // arrange issue
            var remoteField = new RemoteField() { id = "customfield_id", Schema = new RemoteFieldSchema() { Custom = "customfield_type" }, IsCustomField = true, name = "Custom Field" };
            var jObject = JObject.FromObject(new
            {
                fields = new
                {
                    //project = "projectKey",
                    key = "issueKey",
                    customfield_id = new string[] { "val1", "val2" }
                }
            });

            // arrange serialization
            var converter = new RemoteIssueJsonConverter(new List<RemoteField> { remoteField }, new Dictionary<string, ICustomFieldValueSerializer>());
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            serializerSettings.Converters.Add(converter);

            // act
            var remoteIssue = JsonConvert.DeserializeObject<RemoteIssueWrapper>(jObject.ToString(), serializerSettings).RemoteIssue;

            // assert
            var customFieldValues = remoteIssue.customFieldValues.First().values;
            Assert.Equal(2, customFieldValues.Length);
            Assert.Contains("val1", customFieldValues);
            Assert.Contains("val2", customFieldValues);
        }
    }
}
