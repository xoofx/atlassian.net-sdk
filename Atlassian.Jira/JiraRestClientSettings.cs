using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    public class JiraRestClientSettings
    {
        public bool EnableTrace { get; set; }
        public IDictionary<string, ICustomFieldValueSerializer> CustomFieldSerializers { get; set; }

        public JiraRestClientSettings()
        {
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
        }

        private static string GetBuiltInType(string name)
        {
            return String.Format("com.atlassian.jira.plugin.system.customfieldtypes:{0}", name);
        }
    }
}
