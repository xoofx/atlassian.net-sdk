﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            var resource = String.Format("rest/api/2/group/user?groupname={0}", Uri.EscapeDataString(groupname));
            var requestBody = JToken.FromObject(new { name = username });
            return _jira.RestClient.ExecuteRequestAsync(Method.POST, resource, requestBody, token);
        }

        public async Task<IPagedQueryResult<JiraUser>> GetUsersAsync(string groupname, bool includeInactiveUsers = false, int maxResults = 50, int startAt = 0, CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format(
                "rest/api/2/group/member?groupname={0}&includeInactiveUsers={1}&startAt={2}&maxResults={3}",
                Uri.EscapeDataString(groupname),
                includeInactiveUsers,
                startAt,
                maxResults);

            var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resource, null, token).ConfigureAwait(false);
            var serializerSetting = await _jira.RestClient.GetSerializerSettingsAsync(token).ConfigureAwait(false);
            var users = response["values"]
                .Cast<JObject>()
                .Select(valuesJson => JsonConvert.DeserializeObject<JiraUser>(valuesJson.ToString(), serializerSetting));

            return PagedQueryResult<JiraUser>.FromJson((JObject)response, users);
        }

        public Task RemoveUserAsync(string groupname, string username, CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format("rest/api/2/group/user?groupname={0}&username={1}",
                Uri.EscapeDataString(groupname),
                Uri.EscapeDataString(username));

            return _jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token);

        }
    }
}