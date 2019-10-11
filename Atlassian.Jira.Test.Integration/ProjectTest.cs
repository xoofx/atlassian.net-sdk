using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class ProjectTest
    {
        private readonly Random _random = new Random();

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetIssueTypes(Jira jira)
        {
            var project = await jira.Projects.GetProjectAsync("TST");
            var issueTypes = await project.GetIssueTypesAsync();

            Assert.True(issueTypes.Any());
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void AddAndRemoveProjectComponent(Jira jira)
        {
            var componentName = "New Component " + _random.Next(int.MaxValue);
            var projectInfo = new ProjectComponentCreationInfo(componentName);
            var project = jira.Projects.GetProjectsAsync().Result.First();

            // Add a project component.
            var component = project.AddComponentAsync(projectInfo).Result;
            Assert.Equal(componentName, component.Name);

            // Retrive project components.
            Assert.Contains(project.GetComponentsAsync().Result, p => p.Name == componentName);

            // Delete project component
            project.DeleteComponentAsync(component.Name).Wait();
            Assert.DoesNotContain(project.GetComponentsAsync().Result, p => p.Name == componentName);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void GetProjectComponents(Jira jira)
        {
            var components = jira.Components.GetComponentsAsync("TST").Result;
            Assert.Equal(2, components.Count());
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void GetAndUpdateProjectVersions(Jira jira)
        {
            var startDate = new DateTime(2000, 11, 1);
            var versions = jira.Versions.GetVersionsAsync("TST").Result;
            Assert.True(versions.Count() >= 3);

            var version = versions.First(v => v.Name == "1.0");
            var newDescription = "1.0 Release " + _random.Next(int.MaxValue);
            version.Description = newDescription;
            version.StartDate = startDate;
            version.SaveChanges();

            Assert.Equal(newDescription, version.Description);
            version = jira.Versions.GetVersionsAsync("TST").Result.First(v => v.Name == "1.0");
            Assert.Equal(newDescription, version.Description);
            Assert.Equal(version.StartDate, startDate);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void AddAndRemoveProjectVersions(Jira jira)
        {
            var versionName = "New Version " + _random.Next(int.MaxValue);
            var project = jira.Projects.GetProjectsAsync().Result.First();
            var projectInfo = new ProjectVersionCreationInfo(versionName);
            projectInfo.StartDate = new DateTime(2000, 11, 1);

            // Add a project version.
            var version = project.AddVersionAsync(projectInfo).Result;
            Assert.Equal(versionName, version.Name);
            Assert.Equal(version.StartDate, projectInfo.StartDate);

            // Retrive project versions.
            Assert.Contains(project.GetPagedVersionsAsync().Result, p => p.Name == versionName);

            // Delete project version
            project.DeleteVersionAsync(version.Name).Wait();
            Assert.DoesNotContain(project.GetPagedVersionsAsync().Result, p => p.Name == versionName);
        }
    }
}
