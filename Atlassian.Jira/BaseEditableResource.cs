using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atlassian.Jira
{
    public class BaseEditableResource
    {
        protected Jira Jira { get; set; }

        protected BaseEditableResource()
        {
        }


        protected BaseEditableResource(Jira jira)
        {
            Jira = jira;    
        }

        /// <summary>
        /// Syntactic shortcut
        /// </summary>
        protected JsonSerializerSettings SerializerSettings => Jira.RestClient.GetSerializerSettings();

        protected T ExecuteAndGuard<T>(Func<T> execute)
        {
            try
            {
                return execute();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        protected void ExecuteAndGuard(Action execute)
        {
            ExecuteAndGuard(() =>
            {
                execute();
                return false;
            });
        }

        protected JToken ToJToken<T>(T entity)
        {
            var settings = Jira.RestClient.GetSerializerSettings();
            var serializer = JsonSerializer.Create(settings);
            var requestBody = JToken.FromObject(entity, serializer);
            return requestBody;
        }

        /// <summary>
        /// Ensures parameter arguments are escaped
        /// </summary>
        protected string BuildResourceUri(string pathFormat, params object[] queryArguments)
        {
            var safeQueryArguments = queryArguments.Select(arg => (object) Uri.EscapeDataString(arg.ToString())).ToArray();
            var output = string.Format(pathFormat, safeQueryArguments);
            return output;
        }
    }
}
