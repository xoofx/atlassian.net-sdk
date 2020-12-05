using System;
using System.Collections.Generic;
using System.Net;
using Atlassian.Jira.Remote;
using Newtonsoft.Json;

namespace Atlassian.Jira
{
    /// <summary>
    /// Settings to configure the JIRA REST client.
    /// </summary>
    public class JiraRestClientSettings
    {
        private bool _userPrivacyEnabled;

        private static IEnumerable<JsonConverter> _defaultJsonConverters = new List<JsonConverter>()
        {
            new JiraUserJsonConverter()
        };

        private static IEnumerable<JsonConverter> _gdprJsonConverters = new List<JsonConverter>()
        {
            new JiraUserJsonConverter() { UserPrivacyEnabled = true },
        };

        /// <summary>
        /// Whether to trace each request.
        /// </summary>
        public bool EnableRequestTrace { get; set; }

        /// <summary>
        /// Dictionary of serializers for custom fields.
        /// </summary>
        public IDictionary<string, ICustomFieldValueSerializer> CustomFieldSerializers { get; set; } = new Dictionary<string, ICustomFieldValueSerializer>();

        /// <summary>
        /// Cache to store frequently accessed server items.
        /// </summary>
        public JiraCache Cache { get; set; } = new JiraCache();

        /// <summary>
        /// The json global serializer settings to use.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; private set; } = new JsonSerializerSettings();

        /// <summary>
        /// Proxy to use when sending requests.
        /// </summary>
        /// <example>To enable debugging with Fiddler, set Proxy to new WebProxy("127.0.0.1", 8888)</example>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Whether to enable user privacy mode when interacting with Jira server (also known as GDPR mode).
        /// </summary>
        public bool EnableUserPrivacyMode
        {
            get
            {
                return _userPrivacyEnabled;

            }
            set
            {
                _userPrivacyEnabled = value;

                UpdateSerializers();
            }
        }

        /// <summary>
        /// Create a new instance of the settings.
        /// </summary>
        public JiraRestClientSettings()
        {

            JsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            AddCoreCustomFieldValueSerializers();
            AddDefaultJsonConverters();
        }

        private void UpdateSerializers()
        {
            AddCoreCustomFieldValueSerializers();
            RemoveKnownJsonConverters();

            if (_userPrivacyEnabled)
            {
                AddGdprCustomFieldValueSerializers();
                AddGdprJsonConverters();
            }
            else
            {
                AddDefaultJsonConverters();
            }
        }

        private void AddGdprJsonConverters()
        {
            foreach (var converter in _gdprJsonConverters)
            {
                JsonSerializerSettings.Converters.Add(converter);
            }
        }

        private void AddDefaultJsonConverters()
        {
            foreach (var converter in _defaultJsonConverters)
            {
                JsonSerializerSettings.Converters.Add(converter);
            }
        }

        private void RemoveKnownJsonConverters()
        {
            foreach (var converter in _gdprJsonConverters)
            {
                JsonSerializerSettings.Converters.Remove(converter);
            }

            foreach (var converter in _defaultJsonConverters)
            {
                JsonSerializerSettings.Converters.Remove(converter);
            }
        }

        private void AddGdprCustomFieldValueSerializers()
        {
            CustomFieldSerializers[GetBuiltInType("userpicker")] = new SingleObjectCustomFieldValueSerializer("accountId");
            CustomFieldSerializers[GetBuiltInType("multiuserpicker")] = new MultiObjectCustomFieldValueSerializer("accountId");
        }

        private void AddCoreCustomFieldValueSerializers()
        {
            CustomFieldSerializers[GetBuiltInType("labels")] = new MultiStringCustomFieldValueSerializer();
            CustomFieldSerializers[GetBuiltInType("float")] = new FloatCustomFieldValueSerializer();

            CustomFieldSerializers[GetBuiltInType("userpicker")] = new SingleObjectCustomFieldValueSerializer("name");
            CustomFieldSerializers[GetBuiltInType("grouppicker")] = new SingleObjectCustomFieldValueSerializer("name");
            CustomFieldSerializers[GetBuiltInType("project")] = new SingleObjectCustomFieldValueSerializer("key");
            CustomFieldSerializers[GetBuiltInType("radiobuttons")] = new SingleObjectCustomFieldValueSerializer("value");
            CustomFieldSerializers[GetBuiltInType("select")] = new SingleObjectCustomFieldValueSerializer("value");
            CustomFieldSerializers[GetBuiltInType("version")] = new SingleObjectCustomFieldValueSerializer("name");

            CustomFieldSerializers[GetBuiltInType("multigrouppicker")] = new MultiObjectCustomFieldValueSerializer("name");
            CustomFieldSerializers[GetBuiltInType("multiuserpicker")] = new MultiObjectCustomFieldValueSerializer("name");
            CustomFieldSerializers[GetBuiltInType("multiselect")] = new MultiObjectCustomFieldValueSerializer("value");
            CustomFieldSerializers[GetBuiltInType("multiversion")] = new MultiObjectCustomFieldValueSerializer("name");
            CustomFieldSerializers[GetBuiltInType("multicheckboxes")] = new MultiObjectCustomFieldValueSerializer("value");

            CustomFieldSerializers[GetBuiltInType("cascadingselect")] = new CascadingSelectCustomFieldValueSerializer();

            CustomFieldSerializers[GetGreenhopperType("gh-sprint")] = new GreenhopperSprintJsonCustomFieldValueSerialiser();
        }

        private static string GetBuiltInType(string name)
        {
            return String.Format("com.atlassian.jira.plugin.system.customfieldtypes:{0}", name);
        }

        private static string GetGreenhopperType(string name)
        {
            return String.Format("com.pyxis.greenhopper.jira:{0}", name);
        }
    }
}
