using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a named entity within JIRA. Abstracts the Version and Component used on issues
    /// </summary>
    /// <remarks>http://docs.atlassian.com/rpc-jira-plugin/latest/com/atlassian/jira/rpc/soap/beans/AbstractNamedRemoteEntity.html</remarks>
    public class JiraNamedEntity
    {
        private Jira _jira;
        private string _id;
        protected string _name;

        internal JiraNamedEntity(AbstractNamedRemoteEntity remoteEntity)
        {
            _id = remoteEntity.id;
            _name = remoteEntity.name;
        }

        internal JiraNamedEntity(Jira jira, string id)
        {
            _jira = jira;
            _id = id;
        }

        internal JiraNamedEntity(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Id of the entity
        /// </summary>
        public string Id 
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Name of the entity
        /// </summary>
        public string Name 
        {
            get
            {
                if (String.IsNullOrEmpty(_name))
                {
                    _name = GetEntities(_jira).First(e => e.Id.Equals(_id, StringComparison.OrdinalIgnoreCase)).Name;
                }

                return _name;
            }
        }

        protected Jira Jira
        {
            get { return _jira; }
        }

        internal JiraNamedEntity LoadByName(Jira jira, string projectKey)
        {
            var entity = GetEntities(jira, projectKey).FirstOrDefault(e => e.Name.Equals(_name, StringComparison.OrdinalIgnoreCase));

            if (entity != null)
            {
                _id = entity._id;
                _name = entity._name;
            }
            return this;
        }

        protected virtual IEnumerable<JiraNamedEntity> GetEntities(Jira jira, string projectKey = null)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(_name))
            {
                return _name;
            }
            else
            {
                return _id;
            }
        }
    }
}
