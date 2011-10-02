using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    public class CustomField
    {
        private readonly string _id;
        private readonly string _name;

        public CustomField(string id, string name)
        {
            _id = id;
            _name = name;
        }

        public string[] Values { get; set; }

        public string Id
        {
            get { return _id; }
        }


        public string Name
        {
            get { return _name; }
        } 
    }
}
