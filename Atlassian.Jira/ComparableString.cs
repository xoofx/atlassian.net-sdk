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
        private const string JIRA_DATE_FORMAT_STRING = "yyyy/MM/dd";

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

        public static bool operator ==(ComparableString field, DateTime value)
        {
            if ((object)field == null)
            {
                return value == null;
            }
            else
            {
                return field.Value == value.ToString(JIRA_DATE_FORMAT_STRING);
            }
        }

        public static bool operator !=(ComparableString field, DateTime value)
        {
            if ((object)field == null)
            {
                return value != null;
            }
            else
            {
                return field.Value != value.ToString(JIRA_DATE_FORMAT_STRING);
            }
        }

        public static bool operator >(ComparableString field, DateTime value)
        {
            return field.Value.CompareTo(value.ToString(JIRA_DATE_FORMAT_STRING)) > 0;
        }

        public static bool operator <(ComparableString field, DateTime value)
        {
            return field.Value.CompareTo(value.ToString(JIRA_DATE_FORMAT_STRING)) < 0;
        }

        public static bool operator <=(ComparableString field, DateTime value)
        {
            return field.Value.CompareTo(value.ToString(JIRA_DATE_FORMAT_STRING)) <= 0;
        }

        public static bool operator >=(ComparableString field, DateTime value)
        {
            return field.Value.CompareTo(value.ToString(JIRA_DATE_FORMAT_STRING)) >= 0;
        }

        public override string ToString()
        {
            return this.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is ComparableString)
            {
                return this.Value.Equals(((ComparableString)obj).Value);
            }
            else if (obj is string)
            {
                return this.Value.Equals((string)obj);
            }

            return base.Equals(obj);
        }
    }
}
