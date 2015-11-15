using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Dictionary of Jira entities, used to store cached values.
    /// </summary>
    public class JiraEntityDictionary<T> : Dictionary<string, T>, IJiraEntity
        where T : IJiraEntity
    {
        private readonly string _id;

        /// <summary>
        /// Create an empty dictionary.
        /// </summary>
        public JiraEntityDictionary()
        {
        }

        /// <summary>
        /// Create a dictionary and initialize it with the given entities.
        /// </summary>
        public JiraEntityDictionary(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                this.Add(entity.Id, entity);
            }
        }

        /// <summary>
        /// Create a dictionary with an identifier and the given entities.
        /// </summary>
        public JiraEntityDictionary(string id, IEnumerable<T> entities)
            : this(entities)
        {
            this._id = id;
        }

        /// <summary>
        /// Adds an entity to the dictionary if it missing, otherwise no-op.
        /// </summary>
        public void AddIfMIssing(T entity)
        {
            this.AddIfMIssing(new T[1] { entity });
        }

        /// <summary>
        /// Adds a list of entities to the dictionary if their are missing.
        /// </summary>
        public void AddIfMIssing(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                if (!this.ContainsKey(entity.Id))
                {
                    this.Add(entity.Id, entity);
                }
            }
        }

        /// <summary>
        /// Unique identifier for this dictionary.
        /// </summary>
        public string Id
        {
            get { return this._id; }
        }
    }
}
