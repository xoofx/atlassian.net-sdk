using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// A version associated with a project
    /// </summary>
    public class Version: JiraNamedEntity
    {
        private readonly RemoteVersion _remoteVersion;

        internal Version(RemoteVersion remoteVersion)
            :base(remoteVersion)
        {
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
        }

    }
}
