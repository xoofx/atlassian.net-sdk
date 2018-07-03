using System;

namespace Atlassian.Jira.Test.Integration
{
    public class BaseIntegrationTest
    {
        public const string HOST = "http://localhost:2990/jira";
        public const string USERNAME = "admin";
        public const string PASSWORD = "admin";

        protected readonly Jira _jira;
        protected readonly Random _random;

        public BaseIntegrationTest()
        {
            _jira = CreateJiraClient();
            _random = new Random();
        }

        public Jira CreateJiraClient()
        {
            return Jira.CreateRestClient(HOST, USERNAME, PASSWORD);
        }
    }
}
