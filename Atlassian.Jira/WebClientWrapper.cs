using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    internal class WebClientWrapper : IWebClient
    {
        private readonly WebClient _webClient;

        public WebClientWrapper()
        {
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

        public void AddQueryString(string key, string value)
        {
            if (!_webClient.QueryString.AllKeys.Contains(key))
            {
                _webClient.QueryString.Add(key, value);
            }
        }

        public void Download(string url, string fileName)
        {
            _webClient.DownloadFile(url, fileName);
        }

        public Task DownloadAsync(string url, string fileName)
        {
            _webClient.CancelAsync();

            var completionSource = new TaskCompletionSource<object>();
            _webClient.DownloadFileAsync(new Uri(url), fileName, completionSource);

            return completionSource.Task;
        }
    }
}
