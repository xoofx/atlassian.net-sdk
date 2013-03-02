using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Force a DateTime field to use a string provided as the JQL query value.
    /// </summary>
    public class LiteralDateTime
    {
        private readonly string _dateTimeString;

        public LiteralDateTime(string dateTimeString)
        {
            _dateTimeString = dateTimeString;
        }

        public override string ToString()
        {
            return _dateTimeString;
        }

        public static bool operator ==(DateTime dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator !=(DateTime dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator >(DateTime dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator <(DateTime dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator >=(DateTime dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator <=(DateTime dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator ==(DateTime? dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator !=(DateTime? dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator >(DateTime? dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator <(DateTime? dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator >=(DateTime? dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }

        public static bool operator <=(DateTime? dateTime, LiteralDateTime literalDateTime)
        {
            return false;
        }
    }
}
