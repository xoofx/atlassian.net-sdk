using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Type that represents a JIRA text field that can be compared with less than and greater than operators.
    /// </summary>
    public class ComparableTextField
    {
        public string Value { get; set; }

        public ComparableTextField()
        {
        }

        public ComparableTextField(string value)
        {
            this.Value = value;
        }

        public static implicit operator ComparableTextField(string value)
        {
            if (value != null)
            {
                return new ComparableTextField(value);
            }
            else
            {
                return null;
            }
        }

        public static bool operator ==(ComparableTextField field, string value)
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

        public static bool operator !=(ComparableTextField field, string value)
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

        public static bool operator >(ComparableTextField field, string value)
        {
            return false;
        }

        public static bool operator <(ComparableTextField field, string value)
        {
            return false;
        }

        public static bool operator <=(ComparableTextField field, string value)
        {
            return false;
        }

        public static bool operator >=(ComparableTextField field, string value)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}
