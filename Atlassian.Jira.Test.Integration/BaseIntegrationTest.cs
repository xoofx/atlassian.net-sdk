using System;
using System.Threading.Tasks;

namespace Atlassian.Jira.Test.Integration
{
    public class BaseIntegrationTest
    {
        protected readonly Random _random;

        public BaseIntegrationTest()
        {
            _random = new Random();
        }
    }
}
