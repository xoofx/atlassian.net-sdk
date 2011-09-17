using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Atlassian.Jira
{
    /// <summary>
    /// Collection of project versions
    /// </summary>
    public class VersionList: ReadOnlyCollection<Version>
    {
        private List<Version> _newVersions = new List<Version>();
        private readonly string _issueKey;

        internal VersionList(string issueKey, IList<Version> list)
            : base(list)
        {
            _issueKey = issueKey;
        }

        /// <summary>
        /// Register a new version for addition when the issue is updated
        /// </summary>
        /// <param name="version">Version to add</param>
        public void Add(Version version)
        {
            if (String.IsNullOrEmpty(_issueKey))
            {
                throw new InvalidOperationException("Unable to add version, issue has not been created.");
            }

            this.Items.Add(version);
            this._newVersions.Add(version);
        }

        internal IList<Version> GetNewVersions()
        {
            return _newVersions;
        }
    }
}
