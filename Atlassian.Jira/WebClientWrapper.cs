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
        private TaskCompletionSource<object> _completionSource;

        public WebClientWrapper()
        {
            _completionSource = new TaskCompletionSource<object>();
            _webClient = new WebClient();
            _webClient.DownloadFileCompleted += _webClient_DownloadFileCompleted;
        }

        void _webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                _completionSource.SetCanceled();
            }
            else if (e.Error != null)
            {
                _completionSource.SetException(e.Error);
            }
            else
            {
                _completionSource.SetResult(null);
            }
        }

        public void AddQueryString(string key, string value)
        {
            _webClient.QueryString.Add(key, value);
        }

        public void Download(string url, string fileName)
        {
            _webClient.DownloadFile(url, fileName);
        }

        public Task DownloadAsync(string url, string fileName)
        {
            _completionSource.SetCanceled();
            _completionSource = new TaskCompletionSource<object>();

            _webClient.DownloadFileAsync(new Uri(url), fileName);

            return _completionSource.Task;
        }
    }
}
