using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class RestClientAsyncTests
    {
        private const string HOST = "http://40.118.246.128:8080/";

        private readonly Jira _jira;
        private readonly Random _random;

        public RestClientAsyncTests()
        {
            _jira = CreateJiraClient();
            _random = new Random();
        }

        public Jira CreateJiraClient()
        {
            return Jira.CreateRestClient(HOST, "admin", "admin");
        }

        [Fact]
        public async void GetIssueStatusesAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.GetIssueStatusesAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.GetIssueStatusesAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async void GetIssueTypesAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.GetIssueTypesAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.GetIssueTypesAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async void GetIssuePrioritiesAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.GetIssuePrioritiesAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.GetIssuePrioritiesAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async void GetIssueResolutionsAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.GetIssueResolutionsAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.GetIssueResolutionsAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async void GetFavouriteFiltersAsync()
        {
            // First request.
            var jira = CreateJiraClient();
            var result1 = await _jira.RestClient.GetFavouriteFiltersAsync(CancellationToken.None);
            Assert.NotEmpty(result1);

            // Cached
            var result2 = await _jira.RestClient.GetFavouriteFiltersAsync(CancellationToken.None);
            Assert.Equal(result1.Count(), result2.Count());
        }

        [Fact]
        public async void GetIssuesFromJqlAsync()
        {
            var issues = await _jira.GetIssuesFromJqlAsync("key = TST-1");
            Assert.Equal(issues.Count(), 1);
        }
    }
}
