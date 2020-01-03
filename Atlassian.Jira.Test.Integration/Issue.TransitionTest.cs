using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IssueTransitionTest
    {
        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TestTransitionProperties(Jira jira)
        {
            // exercise
            IEnumerable<IssueTransition> transitions = await jira.Issues.GetActionsAsync("TST-1");
            var resolveTransition = transitions.ElementAt(1);

            // assert
            Assert.Equal(3, transitions.Count());
            Assert.Equal("5", resolveTransition.Id);
            Assert.Equal("Resolve Issue", resolveTransition.Name);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TestTransitionToIssueStatus(Jira jira)
        {
            // exercise
            IEnumerable<IssueTransition> transitions = await jira.Issues.GetActionsAsync("TST-1");
            var resolveTransition = transitions.ElementAt(1);
            var resolveIssueStatus = resolveTransition.To;

            // assert
            Assert.Equal("Resolved", resolveIssueStatus.Name);
            Assert.Equal("5", resolveIssueStatus.Id);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TestTransitionWithFields(Jira jira)
        {
            // exercise
            IEnumerable<IssueTransition> transitions = await jira.Issues.GetActionsAsync("TST-1", true);
            var resolveTransition = transitions.ElementAt(1);
            var fields = resolveTransition.Fields;
            var resolution = fields["resolution"];
            var allowedValues = resolution.AllowedValues.ToString();

            // assert
            Assert.Equal(3, fields.Count());
            Assert.Equal("Resolution", resolution.Name);
            Assert.True(resolution.IsRequired);
            Assert.Single(resolution.Operations);
            Assert.Equal(IssueFieldEditMetadataOperation.SET, resolution.Operations.ElementAt(0));
            Assert.Contains("Fixed", allowedValues);
            Assert.Contains("Won't Fix", allowedValues);
            Assert.Contains("Duplicate", allowedValues);
            Assert.Contains("Incomplete", allowedValues);
            Assert.Contains("Cannot Reproduce", allowedValues);
            Assert.Contains("Done", allowedValues);
            Assert.Contains("Won't Do", allowedValues);
            Assert.False(resolution.HasDefaultValue);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task TestTransitionWithoutFields(Jira jira)
        {
            // exercise
            IEnumerable<IssueTransition> transitions = await jira.Issues.GetActionsAsync("TST-1", false);
            var resolveTransition = transitions.ElementAt(1);

            // assert
            Assert.Null(resolveTransition.Fields);
        }
    }
}
