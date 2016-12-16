using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class ProjectTest : BaseIntegrationTest
    {
        [Fact]
        public async Task GetIssueTypes()
        {
            var project = await _jira.Projects.GetProjectAsync("TST");
            var issueTypes = await project.GetIssueTypesAsync();

            Assert.True(issueTypes.Any());
        }
    }
}
