using Atlassian.Jira.Remote;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    /// <summary>
    /// Tests that verify that an end user can use converters to modify the way that the library
    /// deserializes remote types.
    /// </summary>
    public class JsonConverterTest
    {
        [Fact]
        public async Task CanUseConverterToDeserializeReporterEmail()
        {
            var settings = new JiraRestClientSettings();
            settings.JsonSerializerSettings.Converters.Add(new JiraRemoteTypeConverter<RemoteIssue, CustomRemoteIssue>());

            var jira = Jira.CreateRestClient(JiraProvider.HOST, JiraProvider.USERNAME, JiraProvider.PASSWORD, settings);

            var issue = await jira.Issues.GetIssueAsync("TST-1");
            Assert.Equal("admin@example.com", issue.Reporter);
        }
    }

    public class CustomRemoteIssue : RemoteIssue
    {
        [JsonConverter(typeof(NestedValueJsonConverter), "emailAddress")]
        public new string reporter
        {
            get;
            set;
        }
    }

    public class JiraRemoteTypeConverter<T, R> : JsonConverter
        where T : new()
        where R : T
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var customObj = serializer.Deserialize<R>(reader);
            var remoteObj = new T();

            Copy(customObj, remoteObj);

            return remoteObj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public void Copy(object source, object target)
        {

            var propInfo = source.GetType().GetProperties();
            foreach (var item in propInfo)
            {
                target.GetType().GetProperty(item.Name).SetValue(target, item.GetValue(source, null), null);
            }
        }
    }
}
