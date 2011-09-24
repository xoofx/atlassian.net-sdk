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
    public class VersionList: JiraNamedEntityCollection<Version>    
    {
        internal VersionList()
        {
        }

        internal VersionList(IList<Version> list)
            : base(list)
        {
        }
    }
}