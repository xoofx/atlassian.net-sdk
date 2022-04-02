using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the operations on the filters of jira.
    /// </summary>
    public interface IIssueFilterService
    {
        /// <summary>
        /// Returns a filter with the specified id.
        /// </summary>
        /// <param name="filterId">Identifier of the filter to fetch.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<JiraFilter> GetFilterAsync(string filterId, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Returns the favourite filters for the user.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<JiraFilter>> GetFavouritesAsync(CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Returns issues that match the specified favorite filter.
        /// </summary>
        /// <param name="filterName">The name of the filter used for the search</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <param name="token">Cancellation token for this operation.</param>
        /// <remarks>Includes basic fields.</remarks>
        Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteAsync(string filterName, int? maxIssues = null, int startAt = 0, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Returns issues that match the specified favorite filter.
        /// </summary>
        /// <param name="filterName">The name of the filter used for the search</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <param name="fields">A list of specific fields to fetch. Empty or <see langword="null"/> will fetch all fields.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteWithFieldsAsync(string filterName, int? maxIssues = null, int startAt = 0, IList<string> fields = default, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Returns issues that match the filter with the specified id.
        /// </summary>
        /// <param name="filterId">Identifier of the filter to fetch.</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <param name="token">Cancellation token for this operation.</param>
        /// <remarks>Includes basic fields.</remarks>
        Task<IPagedQueryResult<Issue>> GetIssuesFromFilterAsync(string filterId, int? maxIssues = null, int startAt = 0, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Returns issues that match the filter with the specified id.
        /// </summary>
        /// <param name="filterId">Identifier of the filter to fetch.</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <param name="fields">A list of specific fields to fetch. Empty or <see langword="null"/> will fetch all fields.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IPagedQueryResult<Issue>> GetIssuesFromFilterWithFieldsAsync(string filterId, int? maxIssues = null, int startAt = 0, IList<string> fields = default, CancellationToken token = default(CancellationToken));
    }
}
