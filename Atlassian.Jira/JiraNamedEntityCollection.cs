using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    public class JiraNamedEntityCollection<T> : ReadOnlyCollection<T>, IRemoteIssueFieldProvider where T: JiraNamedEntity
    {
        private List<T> _newElements = new List<T>();

        internal JiraNamedEntityCollection()
            : base(new List<T>())
        {
        }

        internal JiraNamedEntityCollection(IList<T> list)
            : base(list)
        {
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

        RemoteFieldValue[] IRemoteIssueFieldProvider.GetRemoteFields(string fieldName)
        {
            var fields = new List<RemoteFieldValue>();

            if (_newElements.Count > 0)
            {
                var field = new RemoteFieldValue()
                {
                    id = fieldName,
                    values = _newElements.Select(e => e.Id).ToArray()
                };
                fields.Add(field);
            }

            return fields.ToArray();
        }
    }
}
