using Atlassian.Jira.Remote;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    public class UserEditableResource : BaseEditableResource, IUserResource
    {
        public UserEditableResource(Jira jira) : base(jira)
        {
        }

        public IEnumerable<JiraUser> Search(string username, bool includeActive = true, bool includeInactive = false, int startAt = 0, int maxResults = 50)
        {
            return ExecuteAndGuard(() => SearchAsync(username, includeActive, includeInactive, startAt, maxResults).Result);
        }

        public JiraUser Get(string username)
        {
            return ExecuteAndGuard(() => GetAsync(username).Result);
        }

        public JiraUser Create(CreateUserRequest user)
        {
            return ExecuteAndGuard(() => CreateAsync(user).Result);
        }

        public void Delete(string username)
        {
            ExecuteAndGuard(() => DeleteAsync(username).Wait());
        }

        public Task<IEnumerable<JiraUser>> SearchAsync(
            string username
            , bool includeActive = true
            , bool includeInactive = false
            , int startAt = 0
            , int maxResults = 50
            , CancellationToken token = default(CancellationToken))
        {
            var resource = BuildResourceUri(
                "rest/api/2/user/search?username={0}&includeActive={1}&includeInActive={2}&startAt={3}&maxResults={4}"
                , username
                , includeActive
                , includeInactive
                , startAt
                , maxResults);

            return Jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraUser>>(Method.GET, resource, null, token);
        }

        public Task<JiraUser> GetAsync(string username, CancellationToken token = default(CancellationToken))
        {
            var resource = BuildResourceUri("rest/api/2/user?username={0}", username);
            var task = Jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.GET, resource, null, token);
            return task;
        }

        public Task<JiraUser> CreateAsync(CreateUserRequest user, CancellationToken token = default(CancellationToken))
        {
            var resource = "rest/api/2/user";
            var requestBody = ToJToken(user);
            return Jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.POST, resource, requestBody, token);
        }

        public Task DeleteAsync(string username, CancellationToken token = default(CancellationToken))
        {
            var resource = BuildResourceUri("rest/api/2/user?username={0}", username);
            var task = Jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token);
            return task;
        }
    }
}
