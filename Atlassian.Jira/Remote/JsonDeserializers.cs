using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Remote
{
    public class JsonDeserializers
    {
        public static RemoteIssue ParseRemoteIssue(JToken jsonIssue)
        {
            var fields = jsonIssue["fields"] as JObject;
            var remoteIssue = new RemoteIssue()
            {
                id = (string)jsonIssue["id"],
                key = (string)jsonIssue["key"],
                summary = (string)fields["summary"],
                assignee = fields["assignee"].Type == JTokenType.Null ? null : (string)fields["assignee"]["name"],
                created = fields["created"].Type == JTokenType.Null ? null : (DateTime?)fields["created"],
                duedate = fields["duedate"].Type == JTokenType.Null ? null : (DateTime?)fields["duedate"],
                description = fields["description"].Type == JTokenType.Null ? null : (string)fields["description"],
                environment = fields["environment"].Type == JTokenType.Null ? null : (string)fields["environment"],
                priority = fields["priority"].Type == JTokenType.Null ? null : (string)fields["priority"]["id"],
                project = fields["project"].Type == JTokenType.Null ? null : (string)fields["project"]["key"],
                reporter = fields["reporter"].Type == JTokenType.Null ? null : (string)fields["reporter"]["name"],
                resolution = fields["resolution"].Type == JTokenType.Null ? null : (string)fields["resolution"]["id"],
                status = fields["status"].Type == JTokenType.Null ? null : (string)fields["status"]["id"],
                type = fields["issuetype"].Type == JTokenType.Null ? null : (string)fields["issuetype"]["id"],
                updated = fields["updated"].Type == JTokenType.Null ? null : (DateTime?)fields["updated"],
                votes = fields["votes"].Type == JTokenType.Null ? null : (long?)fields["votes"]["votes"]
            };

            if (fields["versions"].Type != JTokenType.Null)
            {
                remoteIssue.affectsVersions = fields["versions"].Select(o => JsonConvert.DeserializeObject<RemoteVersion>(o.ToString())).ToArray();
            }
            if (fields["fixVersions"].Type != JTokenType.Null)
            {
                remoteIssue.fixVersions = fields["fixVersions"].Select(o => JsonConvert.DeserializeObject<RemoteVersion>(o.ToString())).ToArray();
            }
            if (fields["components"].Type != JTokenType.Null)
            {
                remoteIssue.components = fields["components"].Select(o => JsonConvert.DeserializeObject<RemoteComponent>(o.ToString())).ToArray();
            }

            var customFields = new List<RemoteCustomFieldValue>();
            foreach (var p in fields)
            {
                if (p.Key.StartsWith("customfield", StringComparison.InvariantCulture) && p.Value.Type != JTokenType.Null)
                {
                    customFields.Add(new RemoteCustomFieldValue() { customfieldId = p.Key, values = new string[] { (string)p.Value } });
                }
            }
            remoteIssue.customFieldValues = customFields.Count > 0 ? customFields.ToArray() : null;

            return remoteIssue;
        }
    }
}
