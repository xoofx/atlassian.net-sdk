using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class RestTest : BaseIntegrationTest
    {
        [Fact]
        public void ExecuteRestRequest()
        {
            var users = _jira.RestClient.ExecuteRequest<JiraNamedResource[]>(Method.GET, "rest/api/2/user/assignable/multiProjectSearch?projectKeys=TST");

            Assert.True(users.Length >= 2);
            Assert.True(users.Any(u => u.Name == "admin"));
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
            var json = _jira.RestClient.ExecuteRequest(Method.POST, "rest/api/2/search", rawBody);

            Assert.Equal(issue.Key.Value, json["issues"][0]["key"].ToString());
        }
    }
}
