using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    public class JiraCredentials
    {
        private readonly string _username;
        private readonly string _password;

        public JiraCredentials(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public string UserName
        {
            get { return _username; }
        }

        public string Password
        {
            get { return _password; }
        } 
    }
}
