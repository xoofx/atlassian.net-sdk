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
        public static IEnumerable<Parameter> GetQueryParametersFromPath(string query)
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
    }
}
