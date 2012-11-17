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
        private readonly string _id;
        private readonly Issue _issue;
        private string _name;

        internal CustomField(string id, Issue issue)
        {
            _id = id;
            _issue = issue;
        }

        internal CustomField(string id, string name, Issue issue)
            : this(id, issue)
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
                    _name = _issue.Jira.GetFieldsForEdit(_issue.Key.Value, _issue.Project).First(f => f.Id == _id).Name;
                }

                return _name; 
            }
        } 
    }
}
