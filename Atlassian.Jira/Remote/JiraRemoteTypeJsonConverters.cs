using Newtonsoft.Json;

namespace Atlassian.Jira.Remote
{
    internal class GdprRemoteIssue : RemoteIssue
    {
        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string reporter { get; set; }

        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string assignee { get; set; }
    }

    internal class GdprRemoteComment : RemoteComment
    {
        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string author { get; set; }

        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string updateAuthor { get; set; }
    }

    internal class GdprRemoteWorklog : RemoteWorklog
    {
        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string author { get; set; }

        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string updateAuthor { get; set; }
    }

    internal class GdprRemoteProject : RemoteProject
    {
        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string lead { get; set; }
    }

    internal class GdprRemoteAttachment : RemoteAttachment
    {
        [JsonConverter(typeof(NestedValueJsonConverter), "accountId")]
        public new string author { get; set; }
    }
}
