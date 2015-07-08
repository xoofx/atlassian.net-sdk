using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Remote
{
    [JsonConverter(typeof(RemoteIssueJsonConverter))]
    public class RemoteIssueWrapper
    {
        private readonly RemoteIssue _remoteIssue;
        private readonly string _parentIssueKey;

        public RemoteIssueWrapper(RemoteIssue remoteIssue, string parentIssueKey = null)
        {
            _remoteIssue = remoteIssue;
            _parentIssueKey = parentIssueKey;
        }

        public RemoteIssue RemoteIssue
        {
            get
            {
                return _remoteIssue;
            }
        }

        public string ParentIssueKey
        {
            get
            {
                return this._parentIssueKey;
            }
        }
    }

    public class RemoteIssueJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RemoteIssueWrapper);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var issueWrapper = value as RemoteIssueWrapper;
            if (issueWrapper == null)
            {
                throw new InvalidOperationException("value must be of type RemoteIssueWrapper.");
            }

            var issue = issueWrapper.RemoteIssue;

            // Round trip the remote issue to get a JObject that has all the fields in the proper format.
            var issueJson = JsonConvert.SerializeObject(issue, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var fields = JObject.Parse(issueJson);

            // Add the custom fields as additional JProperties.
            if (issue.customFieldValues != null)
            {
                foreach (var customField in issue.customFieldValues)
                {
                    if (customField.values != null)
                    {
                        fields.Add(customField.customfieldId, customField.values.FirstOrDefault());
                    }
                }
            }

            // Add a field for the parent issue if this is a sub-task
            if (!String.IsNullOrEmpty(issueWrapper.ParentIssueKey))
            {
                fields.Add("parent", JObject.FromObject(new 
                {
                    key = issueWrapper.ParentIssueKey
                }));
            }

            var wrapper = new JObject(new JProperty("fields", fields));
            wrapper.WriteTo(writer);
        }
    }
}
