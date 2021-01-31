using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Atlassian.Jira.Remote
{
    internal class IssueTypeService : IIssueTypeService
    {
        private readonly Jira _jira;

        public IssueTypeService(Jira jira)
        {
            _jira = jira;
        }

        public async Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken token = default(CancellationToken))
        {
            var cache = _jira.Cache;

            if (!cache.IssueTypes.Any())
            {
                var remoteIssueTypes = await _jira.RestClient.ExecuteRequestAsync<RemoteIssueType[]>(Method.GET, "rest/api/2/issuetype", null, token).ConfigureAwait(false);
                var issueTypes = remoteIssueTypes.Select(t => new IssueType(t));
                cache.IssueTypes.TryAdd(issueTypes);
            }

            return cache.IssueTypes.Values;
        }

        public async Task<IEnumerable<IssueType>> GetIssueTypesForProjectAsync(string projectKey, CancellationToken token = default(CancellationToken))
        {
            var cache = _jira.Cache;

            if (!cache.ProjectIssueTypes.TryGetValue(projectKey, out JiraEntityDictionary<IssueType> _))
            {
                var resource = String.Format("rest/api/2/project/{0}/statuses", projectKey);
                var results = await _jira.RestClient.ExecuteRequestAsync<RemoteIssueType[]>(Method.GET, resource, null, token).ConfigureAwait(false);
                var issueTypes = results.Select(x => new IssueType(x));

                cache.ProjectIssueTypes.TryAdd(projectKey, new JiraEntityDictionary<IssueType>(issueTypes));
            }

            return cache.ProjectIssueTypes[projectKey].Values;
        }
    }
}
