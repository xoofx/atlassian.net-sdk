using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Settings to configure the JIRA REST client.
    /// </summary>
    public class JiraRestClientSettings
    {
        /// <summary>
        /// Whether to trace each request.
        /// </summary>
        public bool EnableRequestTrace { get; set; }

        /// <summary>
        /// Dictionary of serializers for custom fields.
        /// </summary>
        public IDictionary<string, ICustomFieldValueSerializer> CustomFieldSerializers { get; set; }

        /// <summary>
        /// Cache to store frequently accessed server items.
        /// </summary>
        public JiraCache Cache { get; set; }

        /// <summary>
        /// Create a new instance of the settings.
        /// </summary>
        public JiraRestClientSettings()
        {
            this.Cache = new JiraCache();

            this.CustomFieldSerializers = new Dictionary<string, ICustomFieldValueSerializer>();
            this.CustomFieldSerializers.Add(GetBuiltInType("labels"), new MultiStringCustomFieldValueSerializer());
            this.CustomFieldSerializers.Add(GetBuiltInType("float"), new FloatCustomFieldValueSerializer());

            this.CustomFieldSerializers.Add(GetBuiltInType("userpicker"), new SingleObjectCustomFieldValueSerializer("name"));
            this.CustomFieldSerializers.Add(GetBuiltInType("grouppicker"), new SingleObjectCustomFieldValueSerializer("name"));
            this.CustomFieldSerializers.Add(GetBuiltInType("project"), new SingleObjectCustomFieldValueSerializer("key"));
            this.CustomFieldSerializers.Add(GetBuiltInType("radiobuttons"), new SingleObjectCustomFieldValueSerializer("value"));
            this.CustomFieldSerializers.Add(GetBuiltInType("select"), new SingleObjectCustomFieldValueSerializer("value"));
            this.CustomFieldSerializers.Add(GetBuiltInType("version"), new SingleObjectCustomFieldValueSerializer("name"));

            this.CustomFieldSerializers.Add(GetBuiltInType("multigrouppicker"), new MultiObjectCustomFieldValueSerializer("name"));
            this.CustomFieldSerializers.Add(GetBuiltInType("multiuserpicker"), new MultiObjectCustomFieldValueSerializer("name"));
            this.CustomFieldSerializers.Add(GetBuiltInType("multiselect"), new MultiObjectCustomFieldValueSerializer("value"));
            this.CustomFieldSerializers.Add(GetBuiltInType("multiversion"), new MultiObjectCustomFieldValueSerializer("name"));
            this.CustomFieldSerializers.Add(GetBuiltInType("multicheckboxes"), new MultiObjectCustomFieldValueSerializer("value"));

            this.CustomFieldSerializers.Add(GetBuiltInType("cascadingselect"), new CascadingSelectCustomFieldValueSerializer());
        }

        private static string GetBuiltInType(string name)
        {
            return String.Format("com.atlassian.jira.plugin.system.customfieldtypes:{0}", name);
        }
    }
}
