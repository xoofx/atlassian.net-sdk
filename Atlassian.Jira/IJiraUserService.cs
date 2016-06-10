using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the operations on the users of jira.
    /// </summary>
    public interface IJiraUserService
    {
        /// <summary>
        /// Retrieve user specified by username.
        /// </summary>
        /// <param name="username">The username of the user to get.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<JiraUser> GetUserAsync(string username, CancellationToken token = default(CancellationToken));
    }
}
