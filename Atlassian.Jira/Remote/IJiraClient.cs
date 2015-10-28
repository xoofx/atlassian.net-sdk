using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Remote
{
    public interface IJiraClient : IJiraRestClient, IJiraSoapClient
    {
    }
}
