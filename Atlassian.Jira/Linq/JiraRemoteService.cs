using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Atlassian.Jira.Linq
{
    public class JiraRemoteService: IJiraRemoteService
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;

        private string _token = null;

        public JiraRemoteService(string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;
        }

        public IList<Issue> GetIssuesFromJql(string jql)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            binding.TransferMode = TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            var endpoint = new EndpointAddress(_url);

            var client = new JiraSoapServiceClient(binding, endpoint);

            if (_token == null)
            {
                _token = client.login(_username, _password);
            }

            IList<Issue> issues = new List<Issue>();
            foreach (RemoteIssue i in client.getIssuesFromJqlSearch(_token, jql, 10))
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
