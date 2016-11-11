using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class ProjectVersionTest : BaseIntegrationTest
    {
        [Fact]
        public void GetAndUpdateProjectVersions()
        {
            var versions = _jira.Versions.GetVersionsAsync("TST").Result;
            Assert.True(versions.Count() >= 3);

            var version1 = versions.First(v => v.Name == "1.0");
            var newDescription = "1.0 Release " + _random.Next(int.MaxValue);
            version1.Description = newDescription;
            version1.SaveChanges();

            Assert.Equal(newDescription, version1.Description);
            version1 = _jira.Versions.GetVersionsAsync("TST").Result.First(v => v.Name == "1.0");
            Assert.Equal(newDescription, version1.Description);
        }

        [Fact]
        public void AddAndRemoveProjectVersions()
        {
            var versionName = "New Version " + _random.Next(int.MaxValue);
            var projectInfo = new ProjectVersionCreationInfo(versionName);
            var project = _jira.Projects.GetProjectsAsync().Result.First();

            // Add a project version.
            var version = project.AddVersionAsync(projectInfo).Result;
            Assert.Equal(versionName, version.Name);

            // Retrive project versions.
            Assert.True(project.GetPagedVersionsAsync().Result.Any(p => p.Name == versionName));

            // Delete project version
            project.DeleteVersionAsync(version.Name).Wait();
            Assert.False(project.GetPagedVersionsAsync().Result.Any(p => p.Name == versionName));
        }
    }
}
