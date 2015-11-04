using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util.DoubleKeyDictionary;

namespace Atlassian.Jira
{
    /// <summary>
    /// Cache for frequently retrieved server items from JIRA.
    /// </summary>
    public class JiraCache
    {
        // By-project caches.
        private JiraEntityDictionary<JiraEntityDictionary<ProjectVersion>> _cachedVersions = new JiraEntityDictionary<JiraEntityDictionary<ProjectVersion>>();
        private JiraEntityDictionary<JiraEntityDictionary<ProjectComponent>> _cachedComponents = new JiraEntityDictionary<JiraEntityDictionary<ProjectComponent>>();
        private JiraEntityDictionary<JiraEntityDictionary<IssueType>> _cachedIssueTypes = new JiraEntityDictionary<JiraEntityDictionary<IssueType>>();
        private JiraEntityDictionary<JiraEntityDictionary<IssueType>> _cachedSubTaskIssueTypes = new JiraEntityDictionary<JiraEntityDictionary<IssueType>>();

        // General caches
        private JiraEntityDictionary<CustomField> _cachedCustomFields = new JiraEntityDictionary<CustomField>();
        private JiraEntityDictionary<IssuePriority> _cachedPriorities = new JiraEntityDictionary<IssuePriority>();
        private JiraEntityDictionary<IssueStatus> _cachedStatuses = new JiraEntityDictionary<IssueStatus>();
        private JiraEntityDictionary<IssueResolution> _cachedResolutions = new JiraEntityDictionary<IssueResolution>();
        private JiraEntityDictionary<Project> _cachedProjects = new JiraEntityDictionary<Project>();

        public JiraEntityDictionary<JiraEntityDictionary<ProjectVersion>> Versions { get { return this._cachedVersions; } }
        public JiraEntityDictionary<JiraEntityDictionary<ProjectComponent>> Components { get { return this._cachedComponents; } }
        public JiraEntityDictionary<JiraEntityDictionary<IssueType>> IssueTypes { get { return this._cachedIssueTypes; } }
        public JiraEntityDictionary<JiraEntityDictionary<IssueType>> SubTaskIssueTypes { get { return this._cachedSubTaskIssueTypes; } }

        public JiraEntityDictionary<IssuePriority> Priorities { get { return this._cachedPriorities; } }
        public JiraEntityDictionary<IssueStatus> Statuses { get { return this._cachedStatuses; } }
        public JiraEntityDictionary<IssueResolution> Resolutions { get { return this._cachedResolutions; } }
        public JiraEntityDictionary<Project> Projects { get { return this._cachedProjects; } }
        public JiraEntityDictionary<CustomField> CustomFields { get { return this._cachedCustomFields; } }
    }
}
