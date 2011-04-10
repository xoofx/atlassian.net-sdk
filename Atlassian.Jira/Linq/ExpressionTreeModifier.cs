using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Atlassian.Jira.Linq
{
    public class ExpressionTreeModifier: ExpressionVisitor
    {
        private readonly IQueryable<Issue> _queryableIssues;

        public ExpressionTreeModifier(IQueryable<Issue> queryableIssues)
        {
            _queryableIssues = queryableIssues;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type == typeof(JiraQueryable<Issue>))
            {
                return Expression.Constant(_queryableIssues);
            }
            else
            {
                return node;
            }
        }

    }
}
