using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Atlassian.Jira.Remote;
using System.Globalization;

namespace Atlassian.Jira
{
    /// <summary>
    /// Collection of custom fields
    /// </summary>
    public class CustomFieldCollection : ReadOnlyCollection<CustomField>, IRemoteIssueFieldProvider
    {
        private readonly Issue _issue;

        internal CustomFieldCollection(Issue issue)
            : this(issue, new List<CustomField>())   
        {
        }

        internal CustomFieldCollection(Issue issue, IList<CustomField> list)
            : base(list)
        {
            _issue = issue;
        }

        /// <summary>
        /// Add a custom field by name
        /// </summary>
        /// <param name="fieldName">The name of the custom field as defined in JIRA</param>
        /// <param name="fieldValues">The values of the field</param>
        public void Add(string fieldName, string[] fieldValues)
        {
            var fieldId = GetIdForFieldName(fieldName);
            this.Items.Add(new CustomField(fieldId, fieldName, _issue) { Values = fieldValues });
        }

        /// <summary>
        /// Add a custom field by name and action
        /// </summary>
        /// <param name="fieldName">The name of the custom field as defined in JIRA</param>
        /// <param name="actionId">The id of the JIRA action associated with this custom field.</param>
        /// <param name="fieldValues">The values of the field</param>
        public void Add(string fieldName, string actionId, string[] fieldValues)
        {
            var fieldId = GetIdForFieldName(fieldName, actionId);
            this.Items.Add(new CustomField(fieldId, fieldName, _issue) { Values = fieldValues });
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
                _issue.Jira.GetFieldsForEdit(_issue.Key.Value, _issue.Project).FirstOrDefault(
                    f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

            if (customField == null)
            {
                throw new InvalidOperationException(String.Format("Could not find custom field with name '{0}' on the JIRA server. " 
                    + "Make sure this field is available when editing this issue. For more information see JRA-6857", fieldName));
            }

            return customField.Id;
        }

        private string GetIdForFieldName(string fieldName, string actionId)
        {
            var customField =
                _issue.Jira.GetFieldsForAction(_issue.Key.Value, _issue.Project, actionId).FirstOrDefault(
                    f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

            if (customField == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                    CultureInfo.InvariantCulture,
                    "Could not find custom field with name '{0}' and action with id '{1}' on the JIRA server. ", 
                    fieldName,
                    actionId));
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
