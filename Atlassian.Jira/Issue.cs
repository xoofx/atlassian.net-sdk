using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    public class Issue
    {
        [ContainsEquality]
        public string Summary { get; set; }

        [ContainsEquality]
        public string Description { get; set; }

        [ContainsEquality]
        public string Environment { get; set; }

        public string Assignee { get; set; }
        public ComparableTextField Key { get; set; }
        public ComparableTextField Priority { get; set; }
        public string Project { get; set; }
        public string Reporter { get; set; }
        public ComparableTextField Resolution { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }

        public long Votes { get; set; }

        /*
         * Not supported yet
         */
        public DateTime CreatedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime UpdatedDate { get; set; }

    }
}
