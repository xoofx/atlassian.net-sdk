using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Atlassian.Jira.Remote
{
    internal class IssueFieldService : IIssueFieldService
    {
        private readonly Jira _jira;

        public IssueFieldService(Jira jira)
        {
            _jira = jira;
        }

        public async Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CancellationToken token = default(CancellationToken))
        {
            var cache = _jira.Cache;

            if (!cache.CustomFields.Any())
            {
                var remoteFields = await _jira.RestClient.ExecuteRequestAsync<RemoteField[]>(Method.GET, "rest/api/2/field", null, token).ConfigureAwait(false);
                var results = remoteFields.Where(f => f.IsCustomField).Select(f => new CustomField(f));
                cache.CustomFields.TryAdd(results);
            }

            return cache.CustomFields.Values;
        }

        private static IEnumerable<CustomField> GetCustomFieldsFromIssueType(JToken issueType, JsonSerializerSettings serializerSettings)
        {
            return ((JObject)issueType["fields"]).Properties()
                .Where(f => f.Name.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase))
                .Select(f => JsonConvert.DeserializeObject<RemoteField>(f.Value.ToString(), serializerSettings))
                .Select(remoteField => new CustomField(remoteField));
        }

        public async Task<IEnumerable<CustomField>> GetCustomFieldsForProjectAsync(string projectKey, CancellationToken token = default(CancellationToken))
        {
            var cache = _jira.Cache;

            if (!cache.ProjectCustomFields.TryGetValue(projectKey, out JiraEntityDictionary<CustomField> fields))
            {
                var resource = $"rest/api/2/issue/createmeta?projectKeys={projectKey}&expand=projects.issuetypes.fields";
                var jObject = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resource, null, token).ConfigureAwait(false);
                var jProject = jObject["projects"].FirstOrDefault();

                if (jProject == null)
                {
                    throw new InvalidOperationException($"Project with key '{projectKey}' was not found on the Jira server.");
                }

                var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
                var customFields = jProject["issuetypes"].SelectMany(issueType => GetCustomFieldsFromIssueType(issueType, serializerSettings));
                var distinctFields = customFields.GroupBy(c => c.Id).Select(g => g.First());

                cache.ProjectCustomFields.TryAdd(projectKey, new JiraEntityDictionary<CustomField>(distinctFields));
            }

            return cache.ProjectCustomFields[projectKey].Values;
        }
    }
}
