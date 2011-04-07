using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Atlassian.Jira.Linq
{
    public class JqlExpressionTranslator: ExpressionVisitor, IJqlExpressionTranslator
    {
        private StringBuilder _jqlWhere;
        private StringBuilder _jqlOrderBy;

        public string Jql 
        { 
            get
            {
                return _jqlWhere.ToString() + _jqlOrderBy.ToString();
            } 
        }

        public string Translate(Expression expression)
        {
            expression = ExpressionEvaluator.PartialEval(expression);
            _jqlWhere = new StringBuilder();
            _jqlOrderBy = new StringBuilder();

            this.Visit(expression);
            return Jql;
        }

        private PropertyInfo GetFieldFromBinaryExpression(BinaryExpression expression)
        {
            var memberExpression = expression.Left as MemberExpression;
            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }
            }

            throw new NotSupportedException(String.Format(
                   "Operator '{0}' can only be applied on property member fields.",
                   expression.NodeType));
        }

        private object GetValueFromBinaryExpression(BinaryExpression expression)
        {
            if (expression.Right.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression.Right).Value;
            }
            else if (expression.Right.NodeType == ExpressionType.New)
            {
                var newExpression = (NewExpression)expression.Right;
                var args = new List<object>();

                foreach (ConstantExpression e in newExpression.Arguments)
                {
                    args.Add(e.Value);
                }

                return newExpression.Constructor.Invoke(args.ToArray());
            }

            throw new NotSupportedException(String.Format(
                   "Operator '{0}' can only be used with constant values.",
                   expression.NodeType));
        }

        private void ProcessGreaterAndLessThanOperator(BinaryExpression expression, string operatorString)
        {
            var field = GetFieldFromBinaryExpression(expression);
            var value = GetValueFromBinaryExpression(expression);

            // field
            _jqlWhere.Append(field.Name);

            // operator
            _jqlWhere.Append(String.Format(" {0} ", operatorString));

            // value
            ProcessConstant(value);
        }

        private void ProcessEqualityOperator(BinaryExpression expression, bool equal)
        {
            var field = GetFieldFromBinaryExpression(expression);
            var value = GetValueFromBinaryExpression(expression);

            // field
            _jqlWhere.Append(field.Name);

            // special cases for empty/null string
            if (value == null || value.Equals(""))
            {
                _jqlWhere.Append(" ");
                _jqlWhere.Append(equal? JiraOperators.IS : JiraOperators.ISNOT);
                _jqlWhere.Append(" ");
                _jqlWhere.Append(value == null ? "null" : "empty");
                return;
            }

            // operator
            var operatorString = String.Empty;
            if(field.GetCustomAttributes(typeof(ContainsEqualityAttribute), true).Count() > 0)
            {
                operatorString = equal? JiraOperators.CONTAINS: JiraOperators.NOTCONTAINS;
            }
            else
            {
                operatorString = equal? JiraOperators.EQUALS: JiraOperators.NOTEQUALS;
            }
            _jqlWhere.Append(String.Format(" {0} ", operatorString));

            // value
            ProcessConstant(value);
        }

        private void ProcessConstant(object value)
        {
            var valueType = value.GetType();
            if (valueType == typeof(String)
                || valueType == typeof(ComparableTextField))
            {
                _jqlWhere.Append(String.Format("\"{0}\"", value));
            }
            else if (valueType == typeof(DateTime))
            {
                _jqlWhere.Append(String.Format("\"{0}\"", ((DateTime)value).ToString("yyyy/MM/dd")));

            }
            else
            {
                _jqlWhere.Append(value);
            }
        }

        private void ProcessUnionOperator(BinaryExpression expression, string operatorString)
        {
            _jqlWhere.Append("(");
            Visit(expression.Left);
            _jqlWhere.Append(" " + operatorString + " ");
            Visit(expression.Right);
            _jqlWhere.Append(")");
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "OrderBy" 
                || node.Method.Name == "OrderByDescending"
                || node.Method.Name == "ThenBy"
                || node.Method.Name == "ThenByDescending")
            {
                ProcessOrderBy(node);
            }

            return base.VisitMethodCall(node) ;
        }

        private void ProcessOrderBy(MethodCallExpression node)
        {
            var firstOrderBy = _jqlOrderBy.Length == 0;
            if (firstOrderBy)
            {
                _jqlOrderBy.Append(" order by ");
            }
           
            var member = ((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand).Body as MemberExpression;
            if (member != null)
            {
                if (firstOrderBy)
                {
                    _jqlOrderBy.Append(member.Member.Name);
                }
                else
                {
                    _jqlOrderBy.Insert(10, member.Member.Name + ", ");
                }
            }

            if (node.Method.Name == "OrderByDescending"
                || node.Method.Name == "ThenByDescending")
            {
                _jqlOrderBy.Append(" desc");
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.GreaterThan:
                    ProcessGreaterAndLessThanOperator(node, JiraOperators.GREATERTHAN);
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    ProcessGreaterAndLessThanOperator(node, JiraOperators.GREATERTHANOREQUALS);
                    break;

                case ExpressionType.LessThan:
                    ProcessGreaterAndLessThanOperator(node, JiraOperators.LESSTHAN);
                    break;

                case ExpressionType.LessThanOrEqual:
                    ProcessGreaterAndLessThanOperator(node, JiraOperators.LESSTHANOREQUALS);
                    break;

                case ExpressionType.Equal:
                    ProcessEqualityOperator(node, true);
                    break;

                case ExpressionType.NotEqual:
                    ProcessEqualityOperator(node, false);
                    break;

                case ExpressionType.AndAlso:
                    ProcessUnionOperator(node, JiraOperators.AND);
                    break;

                case ExpressionType.OrElse:
                    ProcessUnionOperator(node, JiraOperators.OR);
                    break;

                default:
                    throw new NotSupportedException(String.Format("Expression type '{0}' is not supported.", node.NodeType));

            }

            return node;
        }

    }
}
