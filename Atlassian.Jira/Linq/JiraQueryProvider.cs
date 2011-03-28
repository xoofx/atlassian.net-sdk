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
        private readonly Jira _jiraServer;

        public JiraQueryProvider(IJqlExpressionTranslator translator, Jira jiraInstance)
        {
            _translator = translator;
            _jiraServer = jiraInstance;
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

            return _jiraServer.GetIssuesFromJql(jql);
        }
    }
}
