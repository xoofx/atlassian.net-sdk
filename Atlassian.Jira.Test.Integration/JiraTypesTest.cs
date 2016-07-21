using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class JiraTypesTest : BaseIntegrationTest
    {
        [Fact]
        public void GetFilters()
        {
            var filters = _jira.Filters.GetFavouritesAsync().Result;

            Assert.True(filters.Count() >= 1);
            Assert.True(filters.Any(f => f.Name == "One Issue Filter"));
        }

        [Fact]
        public void RetrieveNamedEntities()
        {
            var issue = _jira.Issues.GetIssueAsync("TST-1").Result;

            Assert.Equal("Bug", issue.Type.Name);
            Assert.Equal("Major", issue.Priority.Name);
            Assert.Equal("Open", issue.Status.Name);
            Assert.Null(issue.Resolution);
        }

        [Fact]
        public void GetIssueTypes()
        {
            var issueTypes = _jira.IssueTypes.GetIssueTypesAsync().Result;

            // In addition, rest API contains "Sub-Task" as an issue type.
            Assert.True(issueTypes.Count() >= 5);
            Assert.True(issueTypes.Any(i => i.Name == "Bug"));
        }

        [Fact]
        public void GetIssuePriorities()
        {
            var priorities = _jira.Priorities.GetPrioritiesAsync().Result;

            Assert.True(priorities.Any(i => i.Name == "Blocker"));
        }

        [Fact]
        public void GetIssueResolutions()
        {
            var resolutions = _jira.Resolutions.GetResolutionsAsync().Result;

            Assert.True(resolutions.Any(i => i.Name == "Fixed"));
        }

        [Fact]
        public void GetIssueStatuses()
        {
            var statuses = _jira.Statuses.GetStatusesAsync().Result;

            Assert.True(statuses.Any(i => i.Name == "Open"));
        }

        [Fact]
        public void GetCustomFields()
        {
            var fields = _jira.Fields.GetCustomFieldsAsync().Result;
            Assert.True(fields.Count() >= 19);
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
        public void GetProjectComponents()
        {
            var components = _jira.Components.GetComponentsAsync("TST").Result;
            Assert.Equal(2, components.Count());
        }

        [Fact]
        public void GetProjects()
        {
            var projects = _jira.Projects.GetProjectsAsync().Result;
            Assert.True(projects.Count() > 0);
            Assert.Equal("admin", projects.First().Lead);
        }

        [Fact]
        public void GetIssueLinkTypes()
        {
            var linkTypes = _jira.Links.GetLinkTypesAsync().Result;
            Assert.True(linkTypes.Any(l => l.Name.Equals("Duplicate")));
        }

        [Fact]
        public async Task GetIssueStatusesAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.Statuses.GetStatusesAsync();
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.Statuses.GetStatusesAsync();
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async Task GetIssueTypesAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.IssueTypes.GetIssueTypesAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.IssueTypes.GetIssueTypesAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async Task GetIssuePrioritiesAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.Priorities.GetPrioritiesAsync();
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.Priorities.GetPrioritiesAsync();
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async Task GetIssueResolutionsAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.Resolutions.GetResolutionsAsync();
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.Resolutions.GetResolutionsAsync();
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async Task GetFavouriteFiltersAsync()
        {
            var jira = CreateJiraClient();
            var result1 = await _jira.Filters.GetFavouritesAsync();
            Assert.NotEmpty(result1);
        }
    }
}
