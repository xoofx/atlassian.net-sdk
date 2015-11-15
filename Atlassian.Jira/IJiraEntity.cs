using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Representes an Jira entity with a unique identifier.
    /// </summary>
    public interface IJiraEntity
    {
        /// <summary>
        /// Unique identifier for this entity.
        /// </summary>
        string Id { get; }
    }
}
