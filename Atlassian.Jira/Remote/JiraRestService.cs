using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace Atlassian.Jira.Remote
{
    public class JiraRestService
    {
        public IEnumerable<string> GetIssuesFromJql(string url, string username, string password, string jql, int startAt, int maxResults)
        {
            var restClient = new RestClient(url);
            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                restClient.Authenticator = new HttpBasicAuthenticator(username, password);
            }

            var request = new RestRequest();
            request.Method = Method.POST;
            request.Resource = "rest/api/latest/search";
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { jql = jql, startAt = startAt, maxResults = maxResults });

            var response = restClient.Execute(request);

            var json = JObject.Parse(response.Content);

            return from i in (JArray)json["issues"]
                   select i.ToString();
        }
    }
}
