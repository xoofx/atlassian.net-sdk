using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Atlassian.Jira
{
    internal class WebClientWrapper: IWebClient
    {
        private readonly WebClient _webClient;

        public WebClientWrapper()
        {
            _webClient = new WebClient();
        }

        public void AddQueryString(string key, string value)
        {
            _webClient.QueryString.Add(key, value);
        }

        public void Download(string url, string fileName)
        {
            _webClient.DownloadFile(url, fileName);
        }
    }
}
