using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Collection of custom fields
    /// </summary>
    public class CustomFieldCollection : ReadOnlyCollection<CustomField>, IRemoteIssueFieldProvider
    {
        private readonly Jira _jira;
        private readonly string _projectKey;

        internal CustomFieldCollection(Jira jira, string projectKey)
            : this(jira, projectKey, new List<CustomField>())   
        {

        }

        internal CustomFieldCollection(Jira jira, string projectKey, IList<CustomField> list)
            : base(list)
        {
            _jira = jira;
            _projectKey = projectKey;
        }

        /// <summary>
        /// Add a custom field by name
        /// </summary>
        /// <param name="fieldName">The name of the custom field as defined in JIRA</param>
        /// <param name="fieldValues">The values of the field</param>
        public void Add(string fieldName, string[] fieldValues)
        {
            var fieldId = GetIdForFieldName(fieldName);
            this.Items.Add(new CustomField(fieldId, fieldName, _jira) { Values = fieldValues });
        }

        /// <summary>
        /// Gets a custom field by name
        /// </summary>
        /// <param name="fieldName">Name of the custom field as defined in JIRA</param>
        /// <returns>CustomField instance if the field has been set on the issue, null otherwise</returns>
        public CustomField this[string fieldName]
        {
            get
            {
                var fieldId = GetIdForFieldName(fieldName);
                return this.Items.FirstOrDefault(f => f.Id == fieldId);
            }
        }

        private string GetIdForFieldName(string fieldName)
        {
            // workaround for bug JRA-6857: GetCustomFields() is for admins only
            var customField =
                _jira.GetFieldsForEdit(_projectKey).First(
                    f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

            if (customField == null)
            {
                throw new InvalidOperationException("Could not find custom field with name '{0}' on the JIRA server. " + 
                    "Make sure this field is available when editing this issue. For more information see JRA-6857");
            }

            return customField.Id;
        }

        RemoteFieldValue[] IRemoteIssueFieldProvider.GetRemoteFields()
        {
            return this.Items.Select(f => new RemoteFieldValue()
            {
                id = f.Id,
                values = f.Values
            }).ToArray();
        }
    }
}
