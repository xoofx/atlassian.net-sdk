using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the operations on the issue types of jira.
    /// Maps to https://docs.atlassian.com/jira/REST/latest/#api/2/issuetype.
    /// </summary>
    public interface IIssueTypeService
    {
        /// <summary>
        /// Returns all the issue types within JIRA.
        /// </summary>
        Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken token = default(CancellationToken));
    }
}
