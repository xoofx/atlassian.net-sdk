using Newtonsoft.Json;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the schema of an issue field.
    /// </summary>
    public class IssueFieldEditMetadataSchema
    {
        /// <summary>
        /// Creates a new instance of IssueFieldSchema based on a remote Entity
        /// </summary>
        /// <param name="remoteEntity">The remote field schema entity</param>
        public IssueFieldEditMetadataSchema(RemoteFieldSchema remoteEntity)
        {
            Type = remoteEntity.Type;
            Items = remoteEntity.Items;
            System = remoteEntity.System;
            Custom = remoteEntity.Custom;
            if (int.TryParse(remoteEntity.CustomId, out int value))
            {
                CustomId = value;
            }
        }

        /// <summary>
        /// Type of the field ( for example array ).
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; private set; }

        /// <summary>
        /// Type of individual values ( for example user, string, ... ).
        /// </summary>
        [JsonProperty("items")]
        public string Items { get; private set; }

        /// <summary>
        /// System name of the field.
        /// </summary>
        [JsonProperty("system")]
        public string System { get; private set; }

        /// <summary>
        /// The JIRA internal custom type of this field.
        /// Example: custom="com.atlassian.jira.plugin.system.customfieldtypes:select"
        /// </summary>
        [JsonProperty("custom")]
        public string Custom { get; private set; }

        /// <summary>
        /// Id of the custom field.
        /// </summary>
        [JsonProperty("customId")]
        public int CustomId { get; private set; }
    }
}
