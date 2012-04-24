using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Linq
{
    public class JqlData
    {
        public string Expression { get; set; }
        public int? MaxResults { get; set; }
        public bool ProcessCount { get; set; }
        public int StartAt { get; set; }
    }
}
