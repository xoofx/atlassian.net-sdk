using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the operations on the Jira server info.
    /// </summary>
    public interface IServerInfoService
    {
        /// <summary>
        /// Gets the server information.
        /// </summary>
        /// <param name="doHealthCheck">if set to <c>true</c>, do a health check.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The server information.</returns>
        Task<ServerInfo> GetServerInfoAsync(bool doHealthCheck = false, CancellationToken token = default(CancellationToken));
    }
}
