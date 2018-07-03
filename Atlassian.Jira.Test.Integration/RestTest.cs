using Atlassian.Jira.Remote;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class CookiesRestClient : JiraRestClient
    {
        private readonly IAuthenticator _authenticator;

        public CookiesRestClient(string url, string user, string password)
            : base(url, user, password)
        {
            RestSharpClient.CookieContainer = new CookieContainer();
            RestSharpClient.Authenticator = null;
            _authenticator = new HttpBasicAuthenticator(user, password);
        }

        protected override async Task<IRestResponse> ExecuteRawResquestAsync(IRestRequest request, CancellationToken token)
        {
            var response = await this.RestSharpClient.ExecuteTaskAsync(request, token).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                RestSharpClient.Authenticator = _authenticator;
                response = await this.RestSharpClient.ExecuteTaskAsync(request, token).ConfigureAwait(false);
                RestSharpClient.Authenticator = null;
            }

            return response;
        }
    }

    public class RestTest : BaseIntegrationTest
    {
        [Fact]
        public async Task CanUseCustomRestClient()
        {
            var restClient = new CookiesRestClient(BaseIntegrationTest.HOST, BaseIntegrationTest.USERNAME, BaseIntegrationTest.PASSWORD);
            var jira = Jira.CreateRestClient(restClient);

            var issue = await jira.Issues.GetIssueAsync("TST-1");
            Assert.Equal("Sample bug in Test Project", issue.Summary);

            var types = await jira.IssueTypes.GetIssueTypesAsync();
            Assert.NotEmpty(types);
        }

        [Fact]
        public void ExecuteRestRequest()
        {
            var users = _jira.RestClient.ExecuteRequestAsync<JiraNamedResource[]>(Method.GET, "rest/api/2/user/assignable/multiProjectSearch?projectKeys=TST").Result;

            Assert.True(users.Length >= 2);
            Assert.Contains(users, u => u.Name == "admin");
        }

        [Fact]
        public void ExecuteRawRestRequest()
        {
            var issue = new Issue(_jira, "TST")
            {
                Type = "1",
                Summary = "Test Summary " + _random.Next(int.MaxValue),
                Assignee = "admin"
            };

            issue.SaveChanges();

            var rawBody = String.Format("{{ \"jql\": \"Key=\\\"{0}\\\"\" }}", issue.Key.Value);
            var json = _jira.RestClient.ExecuteRequestAsync(Method.POST, "rest/api/2/search", rawBody).Result;

            Assert.Equal(issue.Key.Value, json["issues"][0]["key"].ToString());
        }
    }
}
