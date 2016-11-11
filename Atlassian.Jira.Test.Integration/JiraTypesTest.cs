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
            Assert.NotNull(issueTypes.First().IconUrl);
        }

        [Fact]
        public void GetIssuePriorities()
        {
            var priorities = _jira.Priorities.GetPrioritiesAsync().Result;

            Assert.True(priorities.Any(i => i.Name == "Blocker"));
            Assert.NotNull(priorities.First().IconUrl);
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
            Assert.NotNull(statuses.First().IconUrl);
        }

        [Fact]
        public void GetCustomFields()
        {
            var fields = _jira.Fields.GetCustomFieldsAsync().Result;
            Assert.True(fields.Count() >= 19);
        }

        [Fact]
        public void GetProjects()
        {
            var projects = _jira.Projects.GetProjectsAsync().Result;
            Assert.True(projects.Count() > 0);
            Assert.Equal("admin", projects.First().Lead);
        }

        [Fact]
        public void GetProject()
        {
            var project = _jira.Projects.GetProjectAsync("TST").Result;
            Assert.Equal("admin", project.Lead);
            Assert.Equal("Test Project", project.Name);
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
