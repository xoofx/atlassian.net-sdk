using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// The JIRA server info.
    /// </summary>
    public class ServerInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerInfo"/> class.
        /// </summary>
        /// <param name="remoteServerInfo">The remote server information.</param>
        public ServerInfo(RemoteServerInfo remoteServerInfo)
        {
            BaseUrl = remoteServerInfo.baseUrl;
            Version = remoteServerInfo.version;
            VersionNumbers = remoteServerInfo.versionNumbers;
            DeploymentType = remoteServerInfo.deploymentType;
            BuildNumber = remoteServerInfo.buildNumber;
            BuildDate = remoteServerInfo.buildDate;
            ServerTime = remoteServerInfo.serverTime;
            ScmInfo = remoteServerInfo.scmInfo;
            BuildPartnerName = remoteServerInfo.buildPartnerName;
            ServerTitle = remoteServerInfo.serverTitle;
            HealthChecks = remoteServerInfo.healthChecks?.Select(x => new HealthCheck(x)).ToArray();
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        public string BaseUrl { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the version numbers.
        /// </summary>
        public int[] VersionNumbers { get; }

        /// <summary>
        /// Gets the type of the deployment.
        /// </summary>
        public string DeploymentType { get; }

        /// <summary>
        /// Gets the build number.
        /// </summary>
        public int BuildNumber { get; }

        /// <summary>
        /// Gets the build date.
        /// </summary>
        public DateTimeOffset? BuildDate { get; }

        /// <summary>
        /// Gets the server time.
        /// </summary>
        public DateTimeOffset? ServerTime { get; }

        /// <summary>
        /// Gets the SCM information.
        /// </summary>
        public string ScmInfo { get; }

        /// <summary>
        /// Gets the name of the build partner.
        /// </summary>
        public string BuildPartnerName { get; }

        /// <summary>
        /// Gets the server title.
        /// </summary>
        public string ServerTitle { get; }

        /// <summary>
        /// Gets the health checks.
        /// </summary>
        public IEnumerable<HealthCheck> HealthChecks { get; }
    }
}
