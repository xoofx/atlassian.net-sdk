using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// The priority of the issue as defined in JIRA
    /// </summary>
    public class IssuePriority : JiraNamedEntity
    {
         internal IssuePriority(AbstractNamedRemoteEntity remoteEntity)
             : base(remoteEntity)
        {
        }

        internal IssuePriority(Jira jira, string id)
            : base(jira, id)
        {
        }

        internal IssuePriority(string name)
            : base(name)
        {
        }

        protected override IEnumerable<JiraNamedEntity> GetEntities(Jira jira, string projectKey = null)
        {
            return jira.GetIssuePriorities();
        }

        /// <summary>
        /// Allows assignation by name
        /// </summary>
        public static implicit operator IssuePriority(string name)
        {
            if (name != null)
            {
                int id;
                if (int.TryParse(name, out id))
                {
                    return new IssuePriority(null, name /*as id*/);
                }
                else
                {
                    return new IssuePriority(name);                    
                }
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Operator overload to simplify LINQ queries
        /// </summary>
        /// <remarks>
        /// Allows calls in the form of issue.Priority == "High"
        /// </remarks>
        public static bool operator ==(IssuePriority entity, string name)
        {
            if ((object)entity == null)
            {
                return name == null;
            }
            else if (name == null)
            {
                return false;
            }
            else
            {
                return entity._name == name;
            }
        }

        /// <summary>
        /// Operator overload to simplify LINQ queries
        /// </summary>
        /// <remarks>
        /// Allows calls in the form of issue.Priority != "High"
        /// </remarks>
        public static bool operator !=(IssuePriority entity, string name)
        {
            if ((object)entity == null)
            {
                return name != null;
            }
            else if (name == null)
            {
                return true;
            }
            else
            {
                return entity._name != name;
            }
        }

        public static bool operator >(IssuePriority field, string value)
        {
            throw new NotImplementedException();
        }

        public static bool operator <(IssuePriority field, string value)
        {
            throw new NotImplementedException();
        }

        public static bool operator <=(IssuePriority field, string value)
        {
            throw new NotImplementedException();
        }

        public static bool operator >=(IssuePriority field, string value)
        {
            throw new NotImplementedException();
        }
    }
}
