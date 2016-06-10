using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira.Remote
{
    internal class ProjectService : IProjectService
    {
        private readonly Jira _jira;

        public ProjectService(Jira jira)
        {
            _jira = jira;
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync(CancellationToken token = default(CancellationToken))
        {
            var cache = _jira.Cache;
            if (!cache.Projects.Any())
            {
                var remoteProjects = await _jira.RestClient.ExecuteRequestAsync<RemoteProject[]>(Method.GET, "rest/api/2/project?expand=lead", null, token).ConfigureAwait(false);
                cache.Projects.TryAdd(remoteProjects.Select(p => new Project(_jira, p)));
            }

            return cache.Projects.Values;
        }
    }
}
