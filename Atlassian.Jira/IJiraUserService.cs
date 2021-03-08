using System.Collections.Generic;
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
        /// <param name="usernameOrAccountId">The username or account id of the user to get.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<JiraUser> GetUserAsync(string usernameOrAccountId, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Deletes a user by the given username.
        /// </summary>
        /// <param name="usernameOrAccountId">User name or account id of user to delete.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task DeleteUserAsync(string usernameOrAccountId, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Returns a list of users that match the search string.
        /// </summary>
        /// <param name="query">String used to search username, name or e-mail address.</param>
        /// <param name="userStatus">The status(es) of users to include in the result.</param>
        /// <param name="maxResults">Maximum number of users to return (defaults to 50). The maximum allowed value is 1000. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first user to return (0-based).</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<JiraUser>> SearchUsersAsync(string query, JiraUserStatus userStatus = JiraUserStatus.Active, int maxResults = 50, int startAt = 0, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Searches assignable users for an issue.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="issueKey">The issue key.</param>
        /// <param name="startAt">Index of the first user to return (0-based).</param>
        /// <param name="maxResults">The maximum results.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<JiraUser>> SearchAssignableUsersForIssueAsync(string username, string issueKey, int startAt = 0, int maxResults = 50, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Searches assignable users for a project.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="projectKey">The project key.</param>
        /// <param name="startAt">Index of the first user to return (0-based).</param>
        /// <param name="maxResults">The maximum results.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<JiraUser>> SearchAssignableUsersForProjectAsync(string username, string projectKey, int startAt = 0, int maxResults = 50, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Searches the assignable users for a list of projects.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="projectKeys">The project keys.</param>
        /// <param name="startAt">The start at.</param>
        /// <param name="maxResults">Maximum number of users to return (defaults to 50). The
        /// maximum allowed value is 1000. If you specify a value that is higher than this number,
        /// your search results will be truncated.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<JiraUser>> SearchAssignableUsersForProjectsAsync(string username, IEnumerable<string> projectKeys, int startAt = 0, int maxResults = 50, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Creates a user.
        /// </summary>
        /// <param name="user">The information about the user to be created.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<JiraUser> CreateUserAsync(JiraUserCreationInfo user, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Retrieve user currently connected.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<JiraUser> GetMyselfAsync(CancellationToken token = default(CancellationToken));
    }
}
