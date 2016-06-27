using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the operations on the user groups of jira.
    /// </summary>
    public interface IJiraGroupService
    {
        /// <summary>
        /// Returns users that are members of the group speficied.
        /// </summary>
        /// <param name="groupname">The name of group to return users for.</param>
        /// <param name="includeInactiveUsers">Whether to include inactive users.</param>
        /// <param name="maxResults">the maximum number of users to return.</param>
        /// <param name="startAt">Index of the first user in group to return (0 based).</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IPagedQueryResult<JiraUser>> GetUsersAsync(string groupname, bool includeInactiveUsers = false, int maxResults = 50, int startAt = 0, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Adds a user to a the group specified.
        /// </summary>
        /// <param name="groupname">Name of group to add the user to.</param>
        /// <param name="username">Name of user to add.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task AddUserAsync(string groupname, string username, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Removes a user from the group specified.
        /// </summary>
        /// <param name="groupname">Name of the group to remove the user from.</param>
        /// <param name="username">Name of user to remove.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task RemoveUserAsync(string groupname, string username, CancellationToken token = default(CancellationToken));
    }
}
