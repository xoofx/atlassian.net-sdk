using System.Collections.Generic;

namespace Atlassian.Jira
{
    /// <summary>
    /// Options to configure the returned values when querying for comments.
    /// </summary>
    public class CommentQueryOptions
    {
        /// <summary>
        /// The fields of the comment object to expand with information on the server.
        /// </summary>
        public IList<string> Expand { get; } = new List<string>();
    }
}
