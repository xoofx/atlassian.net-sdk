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
            var filters = _jira.GetFilters();

            Assert.True(filters.Count() >= 1);
            Assert.True(filters.Any(f => f.Name == "One Issue Filter"));
        }

        [Fact]
        public void RetrieveNamedEntities()
        {
            var issue = _jira.GetIssue("TST-1");

            Assert.Equal("Bug", issue.Type.Name);
            Assert.Equal("Major", issue.Priority.Name);
            Assert.Equal("Open", issue.Status.Name);
            Assert.Null(issue.Resolution);
        }

        [Fact]
        public void GetIssueTypes()
        {
            var issueTypes = _jira.GetIssueTypes("TST");

#if SOAP
            Assert.Equal(4, issueTypes.Count());
#else
            // In addition, rest API contains "Sub-Task" as an issue type.
            Assert.True(issueTypes.Count() >= 5);
#endif
            Assert.True(issueTypes.Any(i => i.Name == "Bug"));
        }

        [Fact]
        public void GetIssuePriorities()
        {
            var priorities = _jira.GetIssuePriorities();

            Assert.True(priorities.Any(i => i.Name == "Blocker"));
        }

        [Fact]
        public void GetIssueResolutions()
        {
            var resolutions = _jira.GetIssueResolutions();

            Assert.True(resolutions.Any(i => i.Name == "Fixed"));
        }

        [Fact]
        public void GetIssueStatuses()
        {
            var statuses = _jira.GetIssueStatuses();

            Assert.True(statuses.Any(i => i.Name == "Open"));
        }

        [Fact]
        public void GetCustomFields()
        {
            var fields = _jira.GetCustomFields();
            Assert.True(fields.Count() >= 19);
        }

        [Fact]
        public void GetAndUpdateProjectVersions()
        {
            var versions = _jira.GetProjectVersions("TST");
            Assert.Equal(3, versions.Count());

            var version1 = versions.First(v => v.Name == "1.0");
            var newDescription = "1.0 Release " + _random.Next(int.MaxValue);
            version1.Description = newDescription;
            version1.SaveChanges();

            Assert.Equal(newDescription, version1.Description);
            version1 = _jira.GetProjectVersions("TST").First(v => v.Name == "1.0");
            Assert.Equal(newDescription, version1.Description);
        }

        [Fact]
        public void GetProjectComponents()
        {
            var components = _jira.GetProjectComponents("TST");
            Assert.Equal(2, components.Count());
        }

        [Fact]
        public void GetProjects()
        {
            Assert.Equal(1, _jira.GetProjects().Count());
        }

        [Fact]
        public void GetIssueLinkTypes()
        {
            var linkTypes = _jira.GetIssueLinkTypes();
            Assert.True(linkTypes.Any(l => l.Name.Equals("Duplicate")));
        }

        [Fact]
        public async Task GetProjectsAsync()
        {
            var projects = await _jira.GetProjectsAsync(CancellationToken.None);

            Assert.Equal(1, projects.Count());
        }

        [Fact]
        public async Task GetIssueStatusesAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.RestClient.GetIssueStatusesAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.RestClient.GetIssueStatusesAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async Task GetIssueTypesAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.RestClient.GetIssueTypesAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.RestClient.GetIssueTypesAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async Task GetIssuePrioritiesAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.RestClient.GetIssuePrioritiesAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.RestClient.GetIssuePrioritiesAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async Task GetIssueResolutionsAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.RestClient.GetIssueResolutionsAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.RestClient.GetIssueResolutionsAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async Task GetFavouriteFiltersAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.RestClient.GetFavouriteFiltersAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.RestClient.GetFavouriteFiltersAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }
    }
}
