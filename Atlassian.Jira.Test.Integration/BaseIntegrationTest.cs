using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Jira.Test.Integration
{
    public class BaseIntegrationTest
    {
        public const string HOST = "http://localhost:2990/jira";

        protected readonly Jira _jira;
        protected readonly Random _random;

        public BaseIntegrationTest()
        {
            _jira = CreateJiraClient();
            _random = new Random();
        }

        public Jira CreateJiraClient()
        {
#if SOAP
            return Jira.CreateSoapClient(HOST, "admin", "admin");
#else
            return Jira.CreateRestClient(HOST, "admin", "admin");
#endif
        }
    }
}
