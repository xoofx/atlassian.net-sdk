using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// A component associated with a project
    /// </summary>
    public class ProjectComponent: JiraNamedEntity
    {
        private readonly RemoteComponent _remoteComponent;

        internal ProjectComponent(RemoteComponent remoteComponent)
            :base(remoteComponent)
        {
            _remoteComponent = remoteComponent;
        }

        internal RemoteComponent RemoteComponent
        {
            get
            {
                return _remoteComponent;
            }
        }
    }
}
