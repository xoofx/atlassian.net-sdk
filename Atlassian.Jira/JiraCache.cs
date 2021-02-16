using System.Collections.Concurrent;

namespace Atlassian.Jira
{
    /// <summary>
    /// Cache for frequently retrieved server items from JIRA.
    /// </summary>
    public class JiraCache
    {
        public JiraUser CurrentUser { get; set; }
        public JiraEntityDictionary<IssueType> IssueTypes { get; } = new JiraEntityDictionary<IssueType>();
        public JiraEntityDictionary<ProjectComponent> Components { get; } = new JiraEntityDictionary<ProjectComponent>();
        public JiraEntityDictionary<ProjectVersion> Versions { get; } = new JiraEntityDictionary<ProjectVersion>();
        public JiraEntityDictionary<IssuePriority> Priorities { get; } = new JiraEntityDictionary<IssuePriority>();
        public JiraEntityDictionary<IssueStatus> Statuses { get; } = new JiraEntityDictionary<IssueStatus>();
        public JiraEntityDictionary<IssueResolution> Resolutions { get; } = new JiraEntityDictionary<IssueResolution>();
        public JiraEntityDictionary<Project> Projects { get; } = new JiraEntityDictionary<Project>();
        public JiraEntityDictionary<CustomField> CustomFields { get; } = new JiraEntityDictionary<CustomField>();
        public JiraEntityDictionary<IssueLinkType> LinkTypes { get; } = new JiraEntityDictionary<IssueLinkType>();

        public ConcurrentDictionary<string, JiraEntityDictionary<CustomField>> ProjectCustomFields { get; } = new ConcurrentDictionary<string, JiraEntityDictionary<CustomField>>();
        public ConcurrentDictionary<string, JiraEntityDictionary<IssueType>> ProjectIssueTypes { get; } = new ConcurrentDictionary<string, JiraEntityDictionary<IssueType>>();
    }
}
