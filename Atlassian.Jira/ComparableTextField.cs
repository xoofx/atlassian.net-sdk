using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    public class ComparableTextField
    {
        public int? Id { get; set; }
        public string Value { get; set; }

        public ComparableTextField(): this(String.Empty)
        {
        }


        public ComparableTextField(string value)
        {
            this.Value = value;
        }

        public static bool operator ==(ComparableTextField field, string value)
        {
            return field.Value == value;
        }

        public static bool operator !=(ComparableTextField field, string value)
        {
            return field.Value != value;
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

        public static bool operator ==(ComparableTextField field, int id)
        {
            return field.Id == id;
        }

        public static bool operator !=(ComparableTextField field, int id)
        {
            return field.Id != id;
        }

        public static bool operator >(ComparableTextField field, int id)
        {
            return field.Id > id;
        }

        public static bool operator <(ComparableTextField field, int id)
        {
            return field.Id < id;
        }

        public static bool operator <=(ComparableTextField field, int id)
        {
            return field.Id <= id;
        }

        public static bool operator >=(ComparableTextField field, int id)
        {
            return field.Id >= id;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}
