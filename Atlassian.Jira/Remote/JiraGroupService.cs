using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Atlassian.Jira.Remote
{
    internal class JiraGroupService : IJiraGroupService
    {
        private readonly Jira _jira;

        public JiraGroupService(Jira jira)
        {
            _jira = jira;
        }

        public Task AddUserAsync(string groupname, string username, CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format("rest/api/2/group/user?groupname={0}", Uri.EscapeUriString(groupname));
            object body = new { name = username };
            if (_jira.RestClient.Settings.EnableUserPrivacyMode)
            {
                body = new { accountId = username };
            }

            var requestBody = JToken.FromObject(body);
            return _jira.RestClient.ExecuteRequestAsync(Method.POST, resource, requestBody, token);
        }

        public Task CreateGroupAsync(string groupName, CancellationToken token = default(CancellationToken))
        {
            var resource = "rest/api/2/group";
            var requestBody = JToken.FromObject(new { name = groupName });

            return _jira.RestClient.ExecuteRequestAsync(Method.POST, resource, requestBody, token);
        }

        public Task DeleteGroupAsync(string groupName, string swapGroupName = null, CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format("rest/api/2/group?groupname={0}", Uri.EscapeUriString(groupName));

            if (!String.IsNullOrEmpty(swapGroupName))
            {
                resource += String.Format("&swapGroup={0}", Uri.EscapeUriString(swapGroupName));
            }

            return _jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token);
        }

        public async Task<IPagedQueryResult<JiraUser>> GetUsersAsync(string groupname, bool includeInactiveUsers = false, int maxResults = 50, int startAt = 0, CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format(
                "rest/api/2/group/member?groupname={0}&includeInactiveUsers={1}&startAt={2}&maxResults={3}",
                Uri.EscapeUriString(groupname),
                includeInactiveUsers,
                startAt,
                maxResults);

            var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resource, null, token).ConfigureAwait(false);
            var serializerSetting = _jira.RestClient.Settings.JsonSerializerSettings;
            var users = response["values"]
                .Cast<JObject>()
                .Select(valuesJson => JsonConvert.DeserializeObject<JiraUser>(valuesJson.ToString(), serializerSetting));

            return PagedQueryResult<JiraUser>.FromJson((JObject)response, users);
        }

        public Task RemoveUserAsync(string groupname, string username, CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format("rest/api/2/group/user?groupname={0}&{1}={2}",
                Uri.EscapeUriString(groupname),
                _jira.RestClient.Settings.EnableUserPrivacyMode ? "accountId" : "username",
                Uri.EscapeUriString(username));

            return _jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token);

        }
    }
}
