using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Atlassian.Jira
{
    /// <summary>
    /// TODO
    /// </summary>
    public class VersionList: ReadOnlyCollection<Version>
    {
        private readonly Func<string, Version> _versionProvider;
        
        public VersionList(IList<Version> list, Func<string, Version> versionProvider)
            : base(list)
        {
            _versionProvider = versionProvider;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="versionName"></param>
        public void Add(string versionName)
        {
            this.Items.Add(_versionProvider.Invoke(versionName));
        }

    }
}
