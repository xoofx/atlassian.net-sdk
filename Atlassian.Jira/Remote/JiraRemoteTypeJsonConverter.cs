using Newtonsoft.Json;
using System;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// JsonConverter that uses an alternative sub-type (with overriden JsonProperties) to serialize a base type.
    /// This converter can be used as a generic 'transformer' of JSON used by Jira into the types used by the library.
    /// </summary>
    /// <typeparam name="T">The base type that needs to be serialized.</typeparam>
    /// <typeparam name="R">A sub-type that is used to serialize properties into/from the base type.</typeparam>
    public class JiraRemoteTypeJsonConverter<T, R> : JsonConverter
        where T : new()
        where R : T, new()
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var customObj = serializer.Deserialize<R>(reader);
            var remoteObj = new T();

            Copy(customObj, remoteObj);

            return remoteObj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var customObj = new R();

            Copy(value, customObj);

            serializer.Serialize(writer, customObj);
        }

        private void Copy(object source, object target)
        {

            var propInfo = source.GetType().GetProperties();
            foreach (var item in propInfo)
            {
                target.GetType().GetProperty(item.Name).SetValue(target, item.GetValue(source, null), null);
            }
        }
    }
}
