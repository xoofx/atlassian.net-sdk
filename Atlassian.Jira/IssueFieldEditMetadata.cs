using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// An issue field edit metadata as defined in JIRA.
    /// </summary>
    public class IssueFieldEditMetadata
    {
        /// <summary>
        /// Creates a new instance of IssueFieldEditMetadata based on a remote Entity
        /// </summary>
        /// <param name="remoteEntity">The remote field entity</param>
        public IssueFieldEditMetadata(RemoteIssueFieldMetadata remoteEntity)
        {
            IsRequired = remoteEntity.Required;
            Schema = remoteEntity.Schema == null ? null : new IssueFieldEditMetadataSchema(remoteEntity.Schema);
            Name = remoteEntity.name;
            AutoCompleteUrl = remoteEntity.AutoCompleteUrl;
            AllowedValues = remoteEntity.AllowedValues;
            HasDefaultValue = remoteEntity.HasDefaultValue;
            Operations = remoteEntity.Operations;
        }

        /// <summary>
        /// Whether this is a custom field.
        /// </summary>
        public bool IsCustom
        {
            get
            {
                return Schema.Custom != null;
            }
        }

        /// <summary>
        /// Whether the field is required.
        /// </summary>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Schema of this field.
        /// </summary>
        public IssueFieldEditMetadataSchema Schema { get; private set; }

        /// <summary>
        /// Name of this field.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The url to use in autocompletion.
        /// </summary>
        public string AutoCompleteUrl { get; private set; }

        /// <summary>
        /// Operations that can be done on this field.
        /// </summary>
        public IList<IssueFieldEditMetadataOperation> Operations { get; private set; }

        /// <summary>
        /// List of available allowed values that can be set. All objects in this array are of the same type.
        /// However there is multiple possible types it could be.
        /// You should decide what the type it is and convert to custom implemented type by yourself.
        /// </summary>
        public JArray AllowedValues { get; private set; }

        /// <summary>
        /// Whether the field has a default value.
        /// </summary>
        public bool HasDefaultValue { get; set; }

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
