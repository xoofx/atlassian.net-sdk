using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Abstracts a web client.
    /// </summary>
    public interface IWebClient
    {
        /// <summary>
        /// Adds a query string to the request
        /// </summary>
        void AddQueryString(string key, string value);

        /// <summary>
        /// Downloads a file from the server.
        /// </summary>
        void Download(string url, string fileName);

        /// <summary>
        /// Downloads a file from the server.
        /// </summary>
        Task DownloadAsync(string url, string fileName);
    }
}
