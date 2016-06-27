using Atlassian.Jira.Remote;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    public class GroupEditableResource : BaseEditableResource, IGroupResource
    {
        public GroupEditableResource(Jira jira) : base(jira)
        {
        }

        public IPagedQueryResult<JiraUser> GetGroupMembers(string groupname, bool includeInactiveUsers = false, int startAt = 0, int maxResults = 50)
        {
            return ExecuteAndGuard(() => GetGroupMembersAsync(groupname, includeInactiveUsers, startAt, maxResults).Result);
        }
        
        public void AddUser(string groupname, string username)
        {
            ExecuteAndGuard(() => AddUserAsync(groupname, username).Wait());
        }

        public void RemoveUser(string groupname, string username)
        {
            ExecuteAndGuard(() => RemoveUserAsync(groupname, username).Wait());
        }

        public Task<IPagedQueryResult<JiraUser>> GetGroupMembersAsync(string groupname, bool includeInactiveUsers = false, int startAt = 0, int maxResults = 50, CancellationToken token = default(CancellationToken))
        {
            var resource = BuildResourceUri(
                "rest/api/2/group/member?groupname={0}&includeInactiveUsers={1}&startAt={2}&maxResults={3}"
                , groupname
                , includeInactiveUsers
                , startAt
                , maxResults);

            return Jira.RestClient
                            .ExecuteRequestAsync(Method.GET, resource, null, token)
                            .ContinueWith<IPagedQueryResult<JiraUser>>(task =>
            {
                var values = task.Result["values"]
                    .Cast<JObject>()
                    .Select(valuesJson =>
                    {
                        var jiraUser = JsonConvert.DeserializeObject<JiraUser>(valuesJson.ToString(), SerializerSettings);
                        return jiraUser;
                    });

                return PagedQueryResult<JiraUser>.FromJson((JObject)task.Result, values);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// https://docs.atlassian.com/jira/REST/latest/#api/2/group-addUserToGroup
        /// </summary>
        public Task AddUserAsync(string groupname, string username, CancellationToken token = default(CancellationToken))
        {
            var resource = BuildResourceUri("rest/api/2/group/user?groupname={0}", groupname);
            var requestBody = ToJToken(new {name = username});
            return Jira.RestClient.ExecuteRequestAsync(Method.POST, resource, requestBody, token);
        }

        public Task RemoveUserAsync(string groupname, string username, CancellationToken token = default(CancellationToken))
        {
            var resource = BuildResourceUri("rest/api/2/group/user?groupname={0}&username={1}", groupname, username);
            return Jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token);
        }
    }
}
