using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Remote
{
    public class SingleObjectCustomFieldValueSerializer : ICustomFieldValueSerializer
    {
        private readonly string _propertyName;

        public SingleObjectCustomFieldValueSerializer(string propertyName)
        {
            this._propertyName = propertyName;
        }

        public string[] FromJson(JToken json)
        {
            return new string[1] { json[this._propertyName].ToString() };
        }

        public JToken ToJson(string[] values)
        {
            return new JObject(new JProperty(this._propertyName, values[0]));
        }
    }

    public class MultiObjectCustomFieldValueSerializer : ICustomFieldValueSerializer
    {
        private readonly string _propertyName;

        public MultiObjectCustomFieldValueSerializer(string propertyName)
        {
            this._propertyName = propertyName;
        }

        public string[] FromJson(JToken json)
        {
            return ((JArray)json).Select(j => j[_propertyName].ToString()).ToArray();
        }

        public JToken ToJson(string[] values)
        {
            return JArray.FromObject(values.Select(v => new JObject(new JProperty(_propertyName, v))).ToArray());
        }
    }

    public class FloatCustomFieldValueSerializer : ICustomFieldValueSerializer
    {
        public string[] FromJson(JToken json)
        {
            return new string[1] { json.ToString() };
        }

        public JToken ToJson(string[] values)
        {
            return float.Parse(values[0]);
        }
    }

    public class MultiStringCustomFieldValueSerializer : ICustomFieldValueSerializer
    {
        public string[] FromJson(JToken json)
        {
            return JsonConvert.DeserializeObject<string[]>(json.ToString());
        }

        public JToken ToJson(string[] values)
        {
            return JArray.FromObject(values);
        }
    }
}
