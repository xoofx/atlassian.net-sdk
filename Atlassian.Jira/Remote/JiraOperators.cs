using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Container for the supported JIRA operator strings.
    /// </summary>
    public class JiraOperators
    {
        public const string EQUALS = "=";
        public const string NOTEQUALS = "!=";

        public const string CONTAINS = "~";
        public const string NOTCONTAINS = "!~";

        public const string IS = "is";
        public const string ISNOT = "is not";

        public const string GREATERTHAN = ">";
        public const string LESSTHAN = "<";

        public const string GREATERTHANOREQUALS = ">=";
        public const string LESSTHANOREQUALS = "<=";

        public const string OR = "or";
        public const string AND = "and";
        

    }
}
