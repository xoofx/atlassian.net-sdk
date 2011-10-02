using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Atlassian.Jira
{
    public class CustomFieldCollection : ReadOnlyCollection<CustomField>
    {
        private readonly Jira _jira;

        internal CustomFieldCollection(Jira jira, IList<CustomField> list)
            : base(list)
        {
            _jira = jira;
        }

        public void Add(string fieldName, string[] fieldValues)
        {
            var fieldId = _jira.GetCustomFields().First(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)).Id;
            this.Items.Add(new CustomField(fieldId, fieldName) { Values = fieldValues });
        }

        public CustomField this[string fieldName]
        {
            get
            {
                return this.Items.First(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
