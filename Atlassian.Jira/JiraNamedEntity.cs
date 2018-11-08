using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a named entity within JIRA.
    /// </summary>
    public class JiraNamedEntity : IJiraEntity
    {
        /// <summary>
        /// Creates an instance of a JiraNamedEntity base on a remote entity.
        /// </summary>
        public JiraNamedEntity(AbstractNamedRemoteEntity remoteEntity)
            : this(remoteEntity.id, remoteEntity.name)
        {
        }

        /// <summary>
        /// Creates an instance of a JiraNamedEntity.
        /// </summary>
        /// <param name="id">Identifier of the entity.</param>
        /// <param name="name">Name of the entity.</param>
        public JiraNamedEntity(string id, string name = null)
        {
            if (String.IsNullOrEmpty(id) && String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException($"Named entity should have and id or a name. Id: '{id}'. Name: '{name}'.");
            }

            Id = id;
            Name = name;
        }

        /// <summary>
        /// Id of the entity.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Name of the entity.
        /// </summary>
        public string Name { get; private set; }

        protected virtual Task<IEnumerable<JiraNamedEntity>> GetEntitiesAsync(Jira jira, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Name))
            {
                return Name;
            }
            else
            {
                return Id;
            }
        }

        internal async Task<JiraNamedEntity> LoadIdAndNameAsync(Jira jira, CancellationToken token)
        {
            if (String.IsNullOrEmpty(Id) || String.IsNullOrEmpty(Name))
            {
                var entities = await this.GetEntitiesAsync(jira, token).ConfigureAwait(false);
                var entity = entities.FirstOrDefault(e =>
                    (!String.IsNullOrEmpty(Name) && String.Equals(e.Name, this.Name, StringComparison.OrdinalIgnoreCase)) ||
                    (!String.IsNullOrEmpty(Id) && String.Equals(e.Id, this.Id, StringComparison.OrdinalIgnoreCase)));

                if (entity == null)
                {
                    throw new InvalidOperationException(String.Format("Entity with id '{0}' and name '{1}' was not found for type '{2}'. Available: [{3}]",
                        this.Id,
                        this.Name,
                        this.GetType(),
                        String.Join(",", entities.Select(s => s.Id + ":" + s.Name).ToArray())));
                }

                Id = entity.Id;
                Name = entity.Name;
            }

            return this;
        }
    }
}
