using Newtonsoft.Json;

namespace Atlassian.Jira
{
    /// <summary>
    /// Time tracking information for an issue.
    /// </summary>
    public class IssueTimeTrackingData
    {
        /// <summary>
        /// Creates a new instance of the IssueTimeTrackingData class.
        /// </summary>
        public IssueTimeTrackingData(string originalEstimate, string remainingEstimate = null)
        {
            this.OriginalEstimate = originalEstimate;
            this.RemainingEstimate = remainingEstimate;
        }

        [JsonProperty("originalEstimate")]
        public string OriginalEstimate { get; private set; }

        [JsonProperty("originalEstimateSeconds")]
        public long? OriginalEstimateInSeconds { get; private set; }

        [JsonProperty("remainingEstimate")]
        public string RemainingEstimate { get; private set; }

        [JsonProperty("remainingEstimateSeconds")]
        public long? RemainingEstimateInSeconds { get; private set; }

        [JsonProperty("timeSpent")]
        public string TimeSpent { get; private set; }

        [JsonProperty("timeSpentSeconds")]
        public long? TimeSpentInSeconds { get; private set; }
    }
}
