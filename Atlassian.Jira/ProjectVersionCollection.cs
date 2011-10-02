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
        internal ProjectVersionCollection(Jira jira, string projectKey)
            :base(jira, projectKey)
        {
            
        }

        internal ProjectVersionCollection(Jira jira, string projectKey, IList<ProjectVersion> list)
            : base(jira, projectKey, list)
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