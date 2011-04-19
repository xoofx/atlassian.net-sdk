using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// A JIRA issue
    /// </summary>
    public class Issue
    {
        public Issue()
        {
            this.Key = new ComparableTextField();
        }
       
        /// <summary>
        /// Brief one-line summary of the issue
        /// </summary>
        [ContainsEquality]
        public string Summary { get; set; }

        /// <summary>
        /// Detailed description of the issue
        /// </summary>
        [ContainsEquality]
        public string Description { get; set; }

        /// <summary>
        /// Hardware or software environment to which the issue relates
        /// </summary>
        [ContainsEquality]
        public string Environment { get; set; }

        /// <summary>
        /// Person to whom the issue is currently assigned
        /// </summary>
        public string Assignee { get; set; }

        /// <summary>
        /// Unique identifier for this issue
        /// </summary>
        public ComparableTextField Key { get; set; }

        /// <summary>
        /// Importance of the issue in relation to other issues
        /// </summary>
        public ComparableTextField Priority { get; set; }

        /// <summary>
        /// Parent project to which the issue belongs
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Person who entered the issue into the system
        /// </summary>
        public string Reporter { get; set; }
        
        /// <summary>
        /// Record of the issue's resolution, if the issue has been resolved or closed
        /// </summary>
        public ComparableTextField Resolution { get; set; }

        /// <summary>
        /// The stage the issue is currently at in its lifecycle.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The type of the issue
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Number of votes the issue has
        /// </summary>
        public long Votes { get; set; }

        /// <summary>
        /// Time and date on which this issue was entered into JIRA
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Date by which this issue is scheduled to be completed
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Time and date on which this issue was last edited
        /// </summary>
        public DateTime Updated { get; set; }

    }
}
