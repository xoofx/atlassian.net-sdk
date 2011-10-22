using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Type that wraps a string and exposes opertor overloads for
    /// easier LINQ queries
    /// </summary>
    /// <remarks>
    /// Allows comparisons in the form of issue.Key > "TST-1"
    /// </remarks>
    public class ComparableString
    {
        public string Value { get; set; }

        public ComparableString(string value)
        {
            this.Value = value;
        }

        public static implicit operator ComparableString(string value)
        {
            if (value != null)
            {
                return new ComparableString(value);
            }
            else
            {
                return null;
            }
        }

        public static bool operator ==(ComparableString field, string value)
        {
            if ((object) field == null)
            {
                return value == null;
            }
            else
            {
                return field.Value == value;
            }
        }

        public static bool operator !=(ComparableString field, string value)
        {
            if ((object) field == null)
            {
                return value != null;
            }
            else
            {
                return field.Value != value;
            }
        }

        public static bool operator >(ComparableString field, string value)
        {
            return field.Value.CompareTo(value) > 0;
        }

        public static bool operator <(ComparableString field, string value)
        {
            return field.Value.CompareTo(value) < 0;
        }

        public static bool operator <=(ComparableString field, string value)
        {
            return field.Value.CompareTo(value) <= 0;
        }

        public static bool operator >=(ComparableString field, string value)
        {
            return field.Value.CompareTo(value) >= 0;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}
