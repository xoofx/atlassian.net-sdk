using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class ContainsEqualityAttribute: System.Attribute
    {
    }
}
