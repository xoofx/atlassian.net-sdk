using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// The server info health check.
    /// </summary>
    public class HealthCheck
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheck"/> class.
        /// </summary>
        /// <param name="remoteHealthCheck">The remote health check.</param>
        public HealthCheck(RemoteHealthCheck remoteHealthCheck)
        {
            Name = remoteHealthCheck.name;
            Description = remoteHealthCheck.description;
            Passed = remoteHealthCheck.passed;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="HealthCheck"/> is passed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if passed; otherwise, <c>false</c>.
        /// </value>
        public bool Passed { get; }
    }
}
