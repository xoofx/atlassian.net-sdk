using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class ServerInfoTest
    {
        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetServerInfoWithoutHealthCheck(Jira jira)
        {
            var serverInfo = await jira.ServerInfo.GetServerInfoAsync();

            Assert.Equal("Server", serverInfo.DeploymentType);
            Assert.Equal("Your Company Jira", serverInfo.ServerTitle);
            Assert.Null(serverInfo.HealthChecks);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetServerInfoWithHealthCheck(Jira jira)
        {
            var serverInfo = await jira.ServerInfo.GetServerInfoAsync(true);

            Assert.Equal("Server", serverInfo.DeploymentType);
            Assert.Equal("Your Company Jira", serverInfo.ServerTitle);
            Assert.NotNull(serverInfo.HealthChecks);
            Assert.NotEmpty(serverInfo.HealthChecks);
        }
    }
}
