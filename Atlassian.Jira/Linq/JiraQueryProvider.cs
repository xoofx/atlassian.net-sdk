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
            bool isEnumerable = (typeof(T).Name == "IEnumerable`1");

            return (T)this.Execute(expression, isEnumerable);
        }

        public object Execute(Expression expression)
        {
            return Execute(expression, true);
        }

        private object Execute(Expression expression, bool isEnumerable)
        {
            var jql = _translator.Translate(expression);

            IQueryable<Issue> issues = _jiraServer.GetIssuesFromJql(jql).AsQueryable();

            var treeCopier = new ExpressionTreeModifier(issues);
            Expression newExpressionTree = treeCopier.Visit(expression);

            if (isEnumerable)
            {
                return issues;
            }
            else
            {
                return issues.Provider.Execute(newExpressionTree);
            }
        }
    }
}
