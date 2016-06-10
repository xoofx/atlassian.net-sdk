using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// A component associated with a project
    /// </summary>
    public class ProjectComponent : JiraNamedEntity
    {
        private readonly RemoteComponent _remoteComponent;

        /// <summary>
        /// Creates a new instance of ProjectComponent.
        /// </summary>
        /// <param name="remoteComponent">The remote component.</param>
        public ProjectComponent(RemoteComponent remoteComponent)
            : base(remoteComponent)
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

        /// <summary>
        /// Gets the project key associated with this component.
        /// </summary>
        public string ProjectKey
        {
            get
            {
                return _remoteComponent.ProjectKey;
            }
        }
    }
}
