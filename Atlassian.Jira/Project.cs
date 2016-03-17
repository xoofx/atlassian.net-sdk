using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// A JIRA project
    /// </summary>
    public class Project : JiraNamedEntity
    {
        private readonly RemoteProject _remoteProject;
        private readonly Jira _jira;
        private readonly ProjectVersionsEditableResource _remoteResource;

        /// <summary>
        /// Createa a new Project instance using a remote project.
        /// </summary>
        /// <param name="jira">Instance of the Jira client.</param>
        /// <param name="remoteProject">Remote project.</param>
        public Project(Jira jira, RemoteProject remoteProject)
            : base(jira, remoteProject)
        {
            _remoteProject = remoteProject;
            _remoteResource = new ProjectVersionsEditableResource(jira, this);
        }

        internal RemoteProject RemoteProject
        {
            get
            {
                return _remoteProject;
            }
        }

        /// <summary>
        /// The unique identifier of the project.
        /// </summary>
        public string Key
        {
            get
            {
                return _remoteProject.key;
            }
        }

        /// <summary>
        /// Username of the project lead.
        /// </summary>
        public string Lead
        {
            get
            {
                return _remoteProject.lead;
            }
        }

        /// <summary>
        /// Gets an object to interact with the versions of this project.
        /// </summary>
        public ProjectVersionsEditableResource Versions
        {
            get
            {
                return _remoteResource;
            }
        }
    }
}
