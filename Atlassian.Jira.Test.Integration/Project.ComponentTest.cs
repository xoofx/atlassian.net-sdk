using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class ProjectComponentTest : BaseIntegrationTest
    {
        [Fact]
        public void AddAndRemoveProjectComponent()
        {
            var componentName = "New Component " + _random.Next(int.MaxValue);
            var projectInfo = new ProjectComponentCreationInfo(componentName);
            var project = _jira.Projects.GetProjectsAsync().Result.First();

            // Add a project component.
            var component = project.AddComponentAsync(projectInfo).Result;
            Assert.Equal(componentName, component.Name);

            // Retrive project components.
            Assert.True(project.GetComponetsAsync().Result.Any(p => p.Name == componentName));

            // Delete project component
            project.DeleteComponentAsync(component.Name).Wait();
            Assert.False(project.GetComponetsAsync().Result.Any(p => p.Name == componentName));
        }

        [Fact]
        public void GetProjectComponents()
        {
            var components = _jira.Components.GetComponentsAsync("TST").Result;
            Assert.Equal(2, components.Count());
        }
    }
}
