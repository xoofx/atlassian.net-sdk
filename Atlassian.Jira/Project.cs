using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// A JIRA project
    /// </summary>
    public class Project: JiraNamedEntity
    {
        private readonly RemoteProject _remoteProject;

        internal Project(RemoteProject remoteProject)
            : base(remoteProject)
        {
            _remoteProject = remoteProject;
        }

        internal RemoteProject RemoteProject
        {
            get
            {
                return _remoteProject;
            }
        }

        public string Key 
        {
            get
            {
                return _remoteProject.key;
            }
        }

        public string Lead
        {
            get
            {
                return _remoteProject.lead;
            }
        }
    }
}
