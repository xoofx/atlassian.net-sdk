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
            foreach (RemoteIssue i in client.getIssuesFromJqlSearch(token, jql, 10))
            {
                issues.Add(new Issue()
                {
                    Summary = i.summary,
                    Assignee = i.assignee,
                    Description = i.description,
                    Environment = i.environment,
                    Key = new ComparableTextField(i.key),
                    Priority = new ComparableTextField(i.priority),
                    Project = i.project,
                    Reporter = i.reporter,
                    Resolution = new ComparableTextField(i.resolution),
                    Status = i.status,
                    Type = i.type,
                    Votes = i.votes.Value
                    

                });
            }

            return issues;
        }
    }
}
