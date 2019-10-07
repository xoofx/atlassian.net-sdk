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
        /// <param name="jira">The Jira instance that owns the issue.</param>
        public IssueFields(RemoteIssue remoteIssue, Jira jira)
        {
            _map = remoteIssue.fieldsReadOnly ?? new Dictionary<string, JToken>();

            if (remoteIssue.remotePagedComments != null)
            {
                var pagedResults = remoteIssue.remotePagedComments;
                var comments = pagedResults.remoteComments.Select(remoteComment => new Comment(remoteComment));
                this.Comments = new PagedQueryResult<Comment>(comments, pagedResults.startAt, pagedResults.maxResults, pagedResults.total);
            }

            if (remoteIssue.remotePagedWorklogs != null)
            {
                var pagedResults = remoteIssue.remotePagedWorklogs;
                var worklogs = pagedResults.remoteWorklogs.Select(remoteWorklog => new Worklog(remoteWorklog));
                this.Worklogs = new PagedQueryResult<Worklog>(worklogs, pagedResults.startAt, pagedResults.maxResults, pagedResults.total);
            }

            if (remoteIssue.remoteAttachments != null)
            {
                this.Attachments = remoteIssue.remoteAttachments.Select(remoteAttachment => new Attachment(jira, remoteAttachment));
            }
        }

        /// <summary>
        /// Attachments of the issue.
        /// </summary>
        public IEnumerable<Attachment> Attachments { get; private set; }

        /// <summary>
        /// Comments of the issue.
        /// </summary>
        public IPagedQueryResult<Comment> Comments { get; private set; }

        /// <summary>
        /// Worklogs of the issue.
        /// </summary>
        public IPagedQueryResult<Worklog> Worklogs { get; private set; }

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