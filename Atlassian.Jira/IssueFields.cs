using Atlassian.Jira.Remote;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the fields of an issue that are not represented by properties of the Issue class.
    /// </summary>
    public class IssueFields
    {
        private readonly IDictionary<string, JToken> _map;

        /// <summary>
        /// Creates a new instance of IssueFields.
        /// </summary>
        /// <param name="remoteIssue">The remote issue that contains the fields.</param>
        /// <param name="jiraUrl">The JIRA server url.</param>
        /// <param name="credentials">The credentials used to access server resources.</param>
        public IssueFields(RemoteIssue remoteIssue, string jiraUrl = null, JiraCredentials credentials = null)
        {
            _map = remoteIssue.fieldsReadOnly ?? new Dictionary<string, JToken>();

            if (remoteIssue.remoteComments != null)
            {
                this.Comments = remoteIssue.remoteComments.Select(remoteComment => new Comment(remoteComment));
            }

            if (remoteIssue.remoteWorklogs != null)
            {
                this.Worklogs = remoteIssue.remoteWorklogs.Select(remoteWorklog => new Worklog(remoteWorklog));
            }

            if (remoteIssue.remoteAttachments != null)
            {
                this.Attachments = remoteIssue.remoteAttachments.Select(remoteAttachment => new Attachment(jiraUrl, new WebClientWrapper(credentials), remoteAttachment));
            }
        }

        /// <summary>
        /// Attachments of the issue.
        /// </summary>
        public IEnumerable<Attachment> Attachments { get; private set; }

        /// <summary>
        /// Comments of the issue.
        /// </summary>
        public IEnumerable<Comment> Comments { get; private set; }

        /// <summary>
        /// Worklogs of the issue.
        /// </summary>
        public IEnumerable<Worklog> Worklogs { get; private set; }

        /// <summary>
        ///  Gets the field with the specified key.
        /// </summary>
        public JToken this[string key] => _map[key];

        /// <summary>
        /// Determines whether the issue contains a field with the specified key.
        /// </summary>
        public bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }

        /// <summary>
        /// Gets the field associated with the specified key.
        /// </summary>
        public bool TryGetValue(string key, out JToken value)
        {
            return _map.TryGetValue(key, out value);
        }
    }
}