using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Linq
{
    /// <summary>
    /// Attribute that can be applied to properties that map to different JQL field names
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class JqlFieldNameAttribute: Attribute
    {
        public string Name { get; set; }

        public JqlFieldNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
