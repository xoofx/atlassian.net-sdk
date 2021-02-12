namespace Atlassian.Jira
{
    /// <summary>
    /// Represents special fields to be included on the payload when creating an issue.
    /// </summary>
    public class CreateIssueFields
    {
        /// <summary>
        /// Creates a new instance of the CreateIssueFields type.
        /// </summary>
        /// <param name="projectKey">Project key to which the issue belongs to.</param>
        public CreateIssueFields(string projectKey)
        {
            this.ProjectKey = projectKey;
        }

        /// <summary>
        /// Project key to which the issue belongs to (required).
        /// </summary>
        public string ProjectKey { get; set; }

        /// <summary>
        /// Parent issue key if this issue is a sub task.
        /// </summary>
        public string ParentIssueKey { get; set; }

        /// <summary>
        /// Work log estimates to set for the issue.
        /// </summary>
        public IssueTimeTrackingData TimeTrackingData { get; set; }
    }
}
