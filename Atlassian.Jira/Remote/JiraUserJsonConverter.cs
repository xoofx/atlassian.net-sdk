using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// JsonConverter that deserializes a JSON user into a JiraUser object and serializes a JiraUser object
    /// into a single identifier.
    /// </summary>
    public class JiraUserJsonConverter : JsonConverter
    {
        /// <summary>
        /// Whether user privacy mode is enabled (uses 'accountId' insead of 'name' for serialization).
        /// </summary>
        public bool UserPrivacyEnabled { get; set; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JiraUser);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var remoteUser = serializer.Deserialize<RemoteJiraUser>(reader);
            return new JiraUser(remoteUser, UserPrivacyEnabled);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var user = value as JiraUser;

            if (user != null)
            {
                var outerObject = new JObject(new JProperty(
                    UserPrivacyEnabled ? "accountId" : "name",
                    user.InternalIdentifier));

                outerObject.WriteTo(writer);
            }
        }
    }
}
