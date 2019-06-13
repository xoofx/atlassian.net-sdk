using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Remote
{
    internal class JiraHttpBasicAuthenticator : IAuthenticator
    {
        private readonly string _authHeader;

        public JiraHttpBasicAuthenticator(string username, string password)
        {
            _authHeader = GetAuthorizationHeader(username, password);
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (!request.Parameters.Any(p => "Authorization".Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
            {
                request.AddParameter("Authorization", _authHeader, ParameterType.HttpHeader);
            }
        }

        public static string GetAuthorizationHeader(string username, string password)
        {
            string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"));
            return $"Basic {encoded}";
        }
    }
}
