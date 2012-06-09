using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Force a CustomField comparison to use the exact match JQL operator.
    /// </summary>
    public class LiteralMatch
    {
        private readonly string _value;

        public LiteralMatch(string value)
        {
            this._value = value;
        }

        public override string ToString()
        {
            return _value;
        }

        public static bool operator ==(ComparableString comparable, LiteralMatch literal)
        {
            if ((object)comparable == null)
            {
                return literal == null;
            }
            else
            {
                return comparable.Value == literal._value;
            }
        }

        public static bool operator !=(ComparableString comparable, LiteralMatch literal)
        {
            if ((object)comparable == null)
            {
                return literal != null;
            }
            else
            {
                return comparable.Value != literal._value;
            }
        }
    }
}
