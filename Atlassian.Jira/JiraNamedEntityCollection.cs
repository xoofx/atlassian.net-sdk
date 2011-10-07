using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    public class JiraNamedEntityCollection<T> : ReadOnlyCollection<T>, IRemoteIssueFieldProvider where T: JiraNamedEntity
    {
        protected readonly Jira _jira;
        protected readonly string _projectKey;
        protected readonly string _fieldName;

        private List<T> _newElements = new List<T>();

        internal JiraNamedEntityCollection(string fieldName, Jira jira, string projectKey, IList<T> list)
            : base(list)
        {
            _fieldName = fieldName;
            _jira = jira;
            _projectKey = projectKey;
        }

        /// <summary>
        /// Associate a JiraNamedEntity to this issue
        /// </summary>
        /// <param name="element">JiraNamedEntity to add</param>
        public void Add(T element)
        {
            this.Items.Add(element);
            this._newElements.Add(element);
        }

        public static bool operator ==(JiraNamedEntityCollection<T> list, string value)
        {
            return (object)list == null ? value == null : list.Any(v => v.Name == value);
        }

        public static bool operator !=(JiraNamedEntityCollection<T> list, string value)
        {
            return (object)list == null ? value == null : !list.Any(v => v.Name == value);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Items.GetHashCode();
        }

        RemoteFieldValue[] IRemoteIssueFieldProvider.GetRemoteFields()
        {
            var fields = new List<RemoteFieldValue>();

            if (_newElements.Count > 0)
            {
                var field = new RemoteFieldValue()
                {
                    id = _fieldName,
                    values = _newElements.Select(e => e.Id).ToArray()
                };
                fields.Add(field);
            }

            return fields.ToArray();
        }
    }
}
