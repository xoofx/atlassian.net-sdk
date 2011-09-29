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
        internal ProjectVersionCollection()
        {
        }

        internal ProjectVersionCollection(IList<ProjectVersion> list)
            : base(list)
        {
        }
    }
}