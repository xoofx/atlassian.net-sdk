using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Atlassian.Jira
{
    /// <summary>
    /// List of JIRA versions
    /// </summary>
    public class VersionList: ReadOnlyCollection<Version>
    {
        private List<Version> _newVersions = new List<Version>();

        internal VersionList(IList<Version> list)
            : base(list)
        {
        }

        /// <summary>
        /// Add a vesion to this issue
        /// </summary>
        /// <param name="version"></param>
        public void Add(Version version)
        {
            this.Items.Add(version);
            this._newVersions.Add(version);
        }

        internal IList<Version> GetNewVersions()
        {
            return _newVersions;
        }

        public static bool operator ==(VersionList list, string value)
        {
            return (object) list == null ? value == null : list.Any(v => v.Name == value);
        }

        public static bool operator !=(VersionList list, string value)
        {
            return (object)list == null ? value == null : !list.Any(v => v.Name == value);
        }
    }
}