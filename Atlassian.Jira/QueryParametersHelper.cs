using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace Atlassian.Jira
{
    public class QueryParametersHelper
    {
        /// <summary>
        /// Gets the parameters from a full query string.
        /// </summary>
        /// <param name="query">The url query.</param>
        /// <returns>List of all parameters within the query.</returns>
        [Obsolete("GetQueryParametersFromPath() use obsolete RestSharp.Parameter. Use GetParametersFromPath() instead.")]
        public static IEnumerable<RestSharp.Parameter> GetQueryParametersFromPath(string query)
        {
            return GetParametersFromPath(query).Select(x => new RestSharp.Parameter(x.Name, x.Value, x.Type, x.Encode));
        }

        /// <summary>
        /// Gets the parameters from a full query string.
        /// </summary>
        /// <param name="query">The url query.</param>
        /// <returns>List of all parameters within the query.</returns>
        public static IEnumerable<Parameter> GetParametersFromPath(string query)
        {
            var parameters = query.TrimStart('?')
                .Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    var p = s.Split(new[] { '=' }, 2);
                    return new Parameter(name: p[0], value: p.Length > 1 ? p[1] : "", type: ParameterType.QueryString);
                });

            return parameters;
        }

        public class Parameter
        {
            public Parameter(string name, object value, ParameterType type, bool encode = true)
            {
                Name = name;
                Value = value;
                Type = type;
                Encode = encode;
            }

            public string Name { get; }

            public object Value { get; }

            public ParameterType Type { get; }

            public bool Encode { get; }
        }
    }
}
