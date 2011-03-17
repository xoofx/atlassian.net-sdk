using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Atlassian.Jira.Linq
{
    public class JiraQueryProvider: IQueryProvider
    {
        private readonly JqlExpressionTranslator _translator;
        private readonly IJiraRemoteService _remoteService;

        public JiraQueryProvider(JqlExpressionTranslator translator, IJiraRemoteService remoteService)
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
            _translator.Visit(expression);

            return _remoteService.GetIssuesFromJql(_translator.Jql);
        }
    }
}
