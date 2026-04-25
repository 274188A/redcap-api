using Newtonsoft.Json;

namespace Redcap.Models
{
    /// <summary>
    /// Mapping between a REDCap username and its assigned data access group.
    /// </summary>
    public class RedcapUserDagAssignment
    {
        /// <summary>
        /// Existing REDCap username.
        /// </summary>
        [JsonProperty("username")]
        public string? Username { get; set; }

        /// <summary>
        /// Unique group name for the assigned data access group.
        /// </summary>
        [JsonProperty("redcap_data_access_group")]
        public string? RedcapDataAccessGroup { get; set; }
    }
}
