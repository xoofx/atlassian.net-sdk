using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// A custom field associated with an issue
    /// </summary>
    public class CustomField
    {
        private readonly Jira _jira;
        private readonly string _id;
        private readonly string _projectKey;

        private string _name;

        internal CustomField(string id, string projectKey, Jira jira)
        {
            _id = id;
            _jira = jira;
            _projectKey = projectKey;
        }

        internal CustomField(string id, string name, string projectKey, Jira jira)
            : this(id, projectKey, jira)
        {
            _name = name;
        }

        /// <summary>
        /// The values of the custom field
        /// </summary>
        public string[] Values 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Id of the custom field as defined in JIRA
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Name of the custom field as defined in JIRA
        /// </summary>
        public string Name
        {
            get 
            {
                if (String.IsNullOrEmpty(_name)) 
                {
                    _name = _jira.GetFieldsForEdit(_projectKey).First(f => f.Id == _id).Name;
                }

                return _name; 
            }
        } 
    }
}
