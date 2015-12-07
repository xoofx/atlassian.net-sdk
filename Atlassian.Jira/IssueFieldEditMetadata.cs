using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// This class is used as output of 
    /// http://example.com:8080/jira/rest/api/2/issue/{issueIdOrKey}/editmeta [GET] 
    ///  </summary>
    /// Maybe could be also used for elements in http://example.com:8080/jira/rest/api/2/issue/createmeta [GET]
    /// however I am not sure if it's really the same object. In those there is an extra parameter "hasDefaultValue": true/false
    public class IssueFieldEditMetadata
    {

        /// <summary>
        /// Is the field required 
        /// </summary>
        [JsonProperty("required")]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Schema of this field.
        /// </summary>
        [JsonProperty("schema")]
        public IssueFieldEditMetadataSchema Schema { get; set; }

        /// <summary>
        /// Name of this field.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty("autoCompleteUrl")]
        public string AutoCompleteUrl { get; set; }

        /// <summary>
        /// Operations that can be done on this resource
        /// </summary>
        [JsonProperty("operations")]
        public IList<IssueFieldEditMetadataOperation> Operations { get; set; }

        /// <summary>
        /// List of available allowed values that could be setted up. All objects in this array are of the same type. 
        /// However there is multiple possible types it could be. 
        /// You should decide what the type it is and convert to custom implemented type by yourself.
        /// </summary>
        [JsonProperty("allowedValues")] 
        public JArray AllowedValues { get; set; }

        /// <summary>
        /// List of field's available allowed values as object of class T which is ought to be implemented by user of this method. 
        /// Conversion from serialized JObject to custom class T takes here place.
        /// </summary>
        public IEnumerable<T> AllowedValuesAs<T>()
        {
            return AllowedValues.Values<JObject>().Select(x => x.ToObject<T>());
        }

    }
}
