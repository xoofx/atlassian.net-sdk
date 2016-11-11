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
            var startDate = new DateTime(2000, 11, 1);
            var versions = _jira.Versions.GetVersionsAsync("TST").Result;
            Assert.True(versions.Count() >= 3);

            var version = versions.First(v => v.Name == "1.0");
            var newDescription = "1.0 Release " + _random.Next(int.MaxValue);
            version.Description = newDescription;
            version.StartDate = startDate;
            version.SaveChanges();

            Assert.Equal(newDescription, version.Description);
            version = _jira.Versions.GetVersionsAsync("TST").Result.First(v => v.Name == "1.0");
            Assert.Equal(newDescription, version.Description);
            Assert.Equal(version.StartDate, startDate);
        }

        [Fact]
        public void AddAndRemoveProjectVersions()
        {
            var versionName = "New Version " + _random.Next(int.MaxValue);
            var project = _jira.Projects.GetProjectsAsync().Result.First();
            var projectInfo = new ProjectVersionCreationInfo(versionName);
            projectInfo.StartDate = new DateTime(2000, 11, 1);

            // Add a project version.
            var version = project.AddVersionAsync(projectInfo).Result;
            Assert.Equal(versionName, version.Name);
            Assert.Equal(version.StartDate, projectInfo.StartDate);

            // Retrive project versions.
            Assert.True(project.GetPagedVersionsAsync().Result.Any(p => p.Name == versionName));

            // Delete project version
            project.DeleteVersionAsync(version.Name).Wait();
            Assert.False(project.GetPagedVersionsAsync().Result.Any(p => p.Name == versionName));
        }
    }
}
