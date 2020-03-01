using Newtonsoft.Json;

namespace Atlassian.Jira.Remote
{
    internal class GdprRemoteWorklog : RemoteWorklog
    {
        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string author { get; set; }

        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string updateAuthor { get; set; }
    }

    internal class GdprRemoteAttachment : RemoteAttachment
    {
        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string author { get; set; }
    }
}
