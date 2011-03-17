using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Atlassian.Jira.Linq
{
    public class JiraRemoteService: IJiraRemoteService
    {
        public IList<Issue> GetIssuesFromJql(string jql)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            binding.TransferMode = TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            var endpoint = new EndpointAddress("http://farmas-pc:8080/rpc/soap/jirasoapservice-v2");

            var client = new JiraSoapServiceClient(binding, endpoint);

            var token = client.login("admin", "admin");

            IList<Issue> issues = new List<Issue>();
            foreach (RemoteIssue issue in client.getIssuesFromJqlSearch(token, jql, 10))
            {
                issues.Add(new Issue()
                {
                    Summary = issue.summary,
                });
            }

            return issues;
        }
    }
}
