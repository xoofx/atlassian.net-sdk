using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Atlassian.Jira.Linq
{
    public class JiraQueryProvider: IQueryProvider
    {
        private readonly IJqlExpressionTranslator _translator;
        private readonly IJiraRemoteService _remoteService;
        public bool Debug { get; set; }

        public JiraQueryProvider(IJqlExpressionTranslator translator, IJiraRemoteService remoteService)
        {
            _translator = translator;
            _remoteService = remoteService;
        }

        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new JiraQueryable<T>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(Expression expression)
        {
            return (T)this.Execute(expression);
        }

        public object Execute(Expression expression)
        {
            var jql = _translator.Translate(expression);

            if (this.Debug)
            {
                Console.WriteLine("JQL: " + jql);
            }

            return _remoteService.GetIssuesFromJql(jql);
        }
    }
}
