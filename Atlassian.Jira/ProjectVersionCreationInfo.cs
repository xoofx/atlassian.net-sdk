using Newtonsoft.Json;
using System;

namespace Atlassian.Jira
{
    /// <summary>
    /// Class that encapsulates the necessary information to create a new project version.
    /// </summary>
    public class ProjectVersionCreationInfo
    {
        /// <summary>
        /// Creates a new instance of ProjectVersionCreationInfo.
        /// </summary>
        /// <param name="name">The name of the project version.</param>
        public ProjectVersionCreationInfo(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Name of the project version.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of the project version.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Whether this version is archived.
        /// </summary>
        [JsonProperty("archived")]
        public bool IsArchived { get; set; }

        /// <summary>
        /// Whether this version has been released.
        /// </summary>
        [JsonProperty("released")]
        public bool IsReleased { get; set; }

        /// <summary>
        /// The release date, null if the version has not been released yet.
        /// </summary>
        [JsonProperty("releaseDate")]
        public DateTime? ReleaseDate { get; set; }
    }
}
