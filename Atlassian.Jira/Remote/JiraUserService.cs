using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira.Remote
{
    internal class JiraUserService : IJiraUserService
    {
        private readonly Jira _jira;

        public JiraUserService(Jira jira)
        {
            _jira = jira;
        }

        public Task<JiraUser> GetUserAsync(string username, CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format("rest/api/2/user?username={0}", Uri.EscapeDataString(username));
            return _jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.GET, resource, null, token);
        }
    }
}
