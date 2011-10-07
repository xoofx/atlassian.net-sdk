using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Collection of project versions
    /// </summary>
    public class ProjectVersionCollection: JiraNamedEntityCollection<ProjectVersion>    
    {
        internal ProjectVersionCollection(string fieldName, Jira jira, string projectKey)
            :this(fieldName, jira, projectKey, new List<ProjectVersion>())
        {
        }

        internal ProjectVersionCollection(string fieldName, Jira jira, string projectKey, IList<ProjectVersion> list)
            : base(fieldName, jira, projectKey, list)
        {
        }

        /// <summary>
        /// Add a version by name
        /// </summary>
        /// <param name="versionName">Version name</param>
        public void Add(string versionName)
        {
            this.Add(_jira.GetProjectVersions(_projectKey).First(v => v.Name.Equals(versionName, StringComparison.OrdinalIgnoreCase)));
        }


    }
}