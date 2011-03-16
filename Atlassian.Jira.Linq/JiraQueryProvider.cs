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

        public JiraQueryProvider(JqlExpressionTranslator translator)
        {
            _translator = translator;
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
            _translator.ProcessExpression(expression);
            return new List<Issue>();
        }
    }
}
