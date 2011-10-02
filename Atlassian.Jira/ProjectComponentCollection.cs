using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Atlassian.Jira
{
    /// <summary>
    /// Collection of project components
    /// </summary>
    public class ProjectComponentCollection: JiraNamedEntityCollection<ProjectComponent>
    {

        internal ProjectComponentCollection(Jira jira, string projectKey)
            :base(jira, projectKey)
        {
        }

        internal ProjectComponentCollection(Jira jira, string projectKey, IList<ProjectComponent> list)
            : base(jira, projectKey, list)
        {
        }

        /// <summary>
        /// Add a component by name
        /// </summary>
        /// <param name="componentName">Component name</param>
        public void Add(string componentName)
        {
            this.Add(_jira.GetProjectComponents(_projectKey).First(v => v.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase)));
        }
   }
}