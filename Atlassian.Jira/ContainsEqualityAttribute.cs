using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Attribute that can be applied to properties to use a "Contains" rather than "Equals"
    /// when performing equality comparisons.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class ContainsEqualityAttribute: System.Attribute
    {
    }
}
