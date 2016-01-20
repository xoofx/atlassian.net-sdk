using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;
using System.Threading;

namespace Atlassian.Jira
{
    /// <summary>
    /// A version associated with a project
    /// </summary>
    public class ProjectVersion: JiraNamedEntity
    {
        private readonly RemoteVersion _remoteVersion;

        internal ProjectVersion(Jira jira, RemoteVersion remoteVersion)
            :base(jira, remoteVersion.id)
        {
            _name = remoteVersion.name;
            _remoteVersion = remoteVersion;
        }

        internal RemoteVersion RemoteVersion
        {
            get
            {
                return _remoteVersion;
            }
        }

        /// <summary>
        /// Whether this version has been archived
        /// </summary>
        public bool IsArchived
        {
            get
            {
                return _remoteVersion.archived;
            }
            set
            {
                _remoteVersion.archived = value;
            }
        }

        /// <summary>
        /// Whether this version has been released
        /// </summary>
        public bool IsReleased
        {
            get
            {
                return _remoteVersion.released;
            }
			set
			{
				_remoteVersion.released = value;
			}
        }

        /// <summary>
        /// The released date for this version (null if not yet released)
        /// </summary>
        public DateTime? ReleasedDate
        {
            get
            {
                return _remoteVersion.releaseDate;
            }
            set
            {
                _remoteVersion.releaseDate = value;
            }
        }

        /// <summary>
        /// The release description for this version (null if not available)
        /// </summary>
        public string Description
        {
            get
            {
                return _remoteVersion.description;
            }
            set
            {
                _remoteVersion.description = value;
            }
        }

        public void SaveChanges(CancellationToken token = default(CancellationToken))
        {
            if (Jira.RestClient == null)
            {
                throw new NotImplementedException();
            }

            Jira.RestClient.UpdateVersionAsync(_remoteVersion, token);
        }
    }
}