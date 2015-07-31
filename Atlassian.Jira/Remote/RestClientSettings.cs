using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Settings to configure a REST client.
    /// </summary>
    public class RestClientSettings
    {
        /// <summary>
        /// Whether to trace each request.
        /// </summary>
        public bool EnableRequestTrace { get; set; }
    }
}
