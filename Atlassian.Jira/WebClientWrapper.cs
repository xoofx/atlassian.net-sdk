using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    internal class WebClientWrapper : IWebClient
    {
        private readonly WebClient _webClient;
        private readonly JiraCredentials _credentials;

        public WebClientWrapper(JiraCredentials credentials)
        {
            _credentials = credentials;
            _webClient = new WebClient();
            _webClient.DownloadFileCompleted += _webClient_DownloadFileCompleted;
        }

        void _webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            var completionSource = e.UserState as TaskCompletionSource<object>;

            if (completionSource != null)
            {
                if (e.Cancelled)
                {
                    completionSource.TrySetCanceled();
                }
                else if (e.Error != null)
                {
                    completionSource.TrySetException(e.Error);
                }
                else
                {
                    completionSource.TrySetResult(null);
                }
            }
        }

        public Task DownloadAsync(string url, string fileName)
        {
            _webClient.CancelAsync();

            var completionSource = new TaskCompletionSource<object>();
            _webClient.Headers.Remove(HttpRequestHeader.Authorization);
            _webClient.DownloadFileAsync(new Uri(url), fileName, completionSource);

            return completionSource.Task;
        }

        public Task<byte[]> DownloadDataWithAuthenticationAsync(string url)
        {
            _webClient.CancelAsync();

            SetAuthenticationHeader();

            return _webClient.DownloadDataTaskAsync(url);
        }

        public Task DownloadWithAuthenticationAsync(string url, string fileName)
        {
            _webClient.CancelAsync();

            SetAuthenticationHeader();

            var completionSource = new TaskCompletionSource<object>();
            _webClient.DownloadFileAsync(new Uri(url), fileName, completionSource);

            return completionSource.Task;
        }

        private void SetAuthenticationHeader()
        {
            if (_credentials == null || String.IsNullOrEmpty(_credentials.UserName) || String.IsNullOrEmpty(_credentials.Password))
            {
                throw new InvalidOperationException("Unable to download file, user credentials have not been set.");
            }

            var authHeader = JiraHttpBasicAuthenticator.GetAuthorizationHeader(_credentials.UserName, _credentials.Password);

            _webClient.Headers.Remove(HttpRequestHeader.Authorization);
            _webClient.Headers.Add(HttpRequestHeader.Authorization, authHeader);
        }
    }
}
