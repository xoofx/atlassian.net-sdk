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
    public class ComponentList: JiraNamedEntityCollection<Component>
    {
        internal ComponentList()
        {
        }

        internal ComponentList(IList<Component> list)
            : base(list)
        {
        }
   }
}