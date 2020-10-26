using Newtonsoft.Json;

namespace Atlassian.Jira
{
    /// <summary>
    /// Urls for the different renditions of an avatar.
    /// </summary>
    public class AvatarUrls
    {
        [JsonProperty("16x16")]
        public string XSmall { get; set; }
        [JsonProperty("24x24")]
        public string Small { get; set; }
        [JsonProperty("32x32")]
        public string Medium { get; set; }
        [JsonProperty("48x48")]
        public string Large { get; set; }
    }
}
