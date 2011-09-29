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
        internal ProjectComponentCollection()
        {
        }

        internal ProjectComponentCollection(IList<ProjectComponent> list)
            : base(list)
        {
        }
   }
}