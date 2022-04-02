using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace Atlassian.Jira.Remote
{
    internal class IssueFilterService : IIssueFilterService
    {
        private readonly Jira _jira;

        public IssueFilterService(Jira jira)
        {
            _jira = jira;
        }

        public Task<IEnumerable<JiraFilter>> GetFavouritesAsync(CancellationToken token = default)
        {
            return _jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraFilter>>(Method.GET, "rest/api/2/filter/favourite", null, token);
        }

        public Task<JiraFilter> GetFilterAsync(string filterId, CancellationToken token = default)
        {
            return _jira.RestClient.ExecuteRequestAsync<JiraFilter>(Method.GET, $"rest/api/2/filter/{filterId}", null, token);
        }

        public async Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteAsync(string filterName, int? maxIssues = null, int startAt = 0, CancellationToken token = default)
        {
            var jql = await GetFilterJqlByNameAsync(filterName, token).ConfigureAwait(false);

            return await _jira.Issues.GetIssuesFromJqlAsync(jql, maxIssues, startAt, token).ConfigureAwait(false);
        }

        public async Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteWithFieldsAsync(string filterName, int? maxIssues = default, int startAt = 0, IList<string> fields = default, CancellationToken token = default)
        {
            var jql = await GetFilterJqlByNameAsync(filterName, token).ConfigureAwait(false);

            var searchOptions = new IssueSearchOptions(jql)
            {
                MaxIssuesPerRequest = maxIssues,
                StartAt = startAt,
                AdditionalFields = fields,
                FetchBasicFields = false
            };
            return await _jira.Issues.GetIssuesFromJqlAsync(searchOptions, token).ConfigureAwait(false);
        }

        public async Task<IPagedQueryResult<Issue>> GetIssuesFromFilterAsync(string filterId, int? maxIssues = null, int startAt = 0, CancellationToken token = default)
        {
            var jql = await GetFilterJqlByIdAsync(filterId, token).ConfigureAwait(false);

            return await _jira.Issues.GetIssuesFromJqlAsync(jql, maxIssues, startAt, token).ConfigureAwait(false);
        }

        public async Task<IPagedQueryResult<Issue>> GetIssuesFromFilterWithFieldsAsync(string filterId, int? maxIssues = default, int startAt = 0, IList<string> fields = default, CancellationToken token = default)
        {
            var jql = await GetFilterJqlByIdAsync(filterId, token).ConfigureAwait(false);

            var searchOptions = new IssueSearchOptions(jql)
            {
                MaxIssuesPerRequest = maxIssues,
                StartAt = startAt,
                AdditionalFields = fields,
                FetchBasicFields = false
            };
            return await _jira.Issues.GetIssuesFromJqlAsync(searchOptions, token).ConfigureAwait(false);
        }

        private async Task<string> GetFilterJqlByNameAsync(string filterName, CancellationToken token = default)
        {
            var filters = await this.GetFavouritesAsync(token).ConfigureAwait(false);
            var filter = filters.FirstOrDefault(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase));

            if (filter == null)
            {
                throw new InvalidOperationException($"Filter with name '{filterName}' was not found.");
            }

            return filter.Jql;
        }

        private async Task<string> GetFilterJqlByIdAsync(string filterId, CancellationToken token = default)
        {
            var filter = await GetFilterAsync(filterId, token);

            if (filter == null)
            {
                throw new InvalidOperationException($"Filter with ID '{filterId}' was not found.");
            }

            return filter.Jql;
        }
    }
}
