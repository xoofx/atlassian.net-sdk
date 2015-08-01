using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Time tracking information for an issue.
    /// </summary>
    public class IssueTimeTrackingData
    {
        [JsonProperty("originalEstimate")]
        public string OriginalEstimate { get; set; }

        [JsonProperty("originalEstimateSeconds")]
        public int OriginalEstimateInSeconds { get; set; }

        [JsonProperty("remainingEstimate")]
        public string RemainingEstimate { get; set; }

        [JsonProperty("remainingEstimateSeconds")]
        public int RemainingEstimateInSeconds { get; set; }

        [JsonProperty("timeSpent")]
        public string TimeSpent { get; set; }

        [JsonProperty("timeSpentSeconds")]
        public int TimeSpentInSeconds { get; set; }
    }
}
