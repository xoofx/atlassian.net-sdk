using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// The worklog time remaining strategy 
    /// </summary>
    public enum WorklogStrategy
    {
        AutoAdjustRemainingEstimate,
        RetainRemainingEstimate,
        NewRemainingEstimate
    }
}
