using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    public interface IWebClient
    {
        void AddQueryString(string key, string value);
        void Download(string url, string fileName);
    }
}
