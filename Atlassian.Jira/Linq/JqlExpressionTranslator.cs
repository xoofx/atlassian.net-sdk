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
        private StringBuilder _jql;

        public string Jql 
        { 
            get
            {
                return _jql.ToString();
            } 
        }

        public string Translate(Expression expression)
        {
            _jql = new StringBuilder();
            this.Visit(expression);
            return _jql.ToString();
        }

        private Tuple<PropertyInfo, object> DecomposeConstantOperatorExpression(BinaryExpression expression)
        {
            
            var memberExpression = expression.Left as MemberExpression;
            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    var constant = expression.Right as ConstantExpression;
                    if (constant != null)
                    {
                        return Tuple.Create<PropertyInfo, object>(propertyInfo, constant.Value);
                    }
                }
            }

            throw new NotSupportedException(String.Format(
                   "Operator '{0}' can only be applied on fields with constant values.",
                   expression.NodeType));
        }

        private void ProcessNumbericOperatorExpression(BinaryExpression expression, string operatorString)
        {
            var tuple = DecomposeConstantOperatorExpression(expression);

            // field
            _jql.Append(tuple.Item1.Name);

            // operator
            _jql.Append(String.Format(" {0} ", operatorString));

            // value
            ProcessConstant(tuple.Item2);
        }

        private void ProcessEqualityOperator(BinaryExpression expression, bool equal)
        {
            var tuple = DecomposeConstantOperatorExpression(expression);

            // field
            _jql.Append(tuple.Item1.Name);

            // special cases for empty/null string
            if (tuple.Item2 == null || tuple.Item2.Equals(""))
            {
                _jql.Append(" ");
                _jql.Append(equal? Operators.IS : Operators.ISNOT);
                _jql.Append(" ");
                _jql.Append(tuple.Item2 == null? "null" : "empty");
                return;
            }

            // operator
            var operatorString = String.Empty;
            if(tuple.Item1.GetCustomAttributes(typeof(ContainsEqualityAttribute), true).Count() > 0)
            {
                operatorString = equal? Operators.CONTAINS: Operators.NOTCONTAINS;
            }
            else
            {
                operatorString = equal? Operators.EQUALS: Operators.NOTEQUALS;
            }
            _jql.Append(String.Format(" {0} ", operatorString));

            // value
            ProcessConstant(tuple.Item2);
        }

        private void ProcessConstant(object value)
        {
            var valueType = value.GetType();
            if (valueType == typeof(String)
                || valueType == typeof(ComparableTextField))
            {
                _jql.Append(String.Format("\"{0}\"", value));
            }
            else
            {
                _jql.Append(value);
            }
        }

        private void ProcessUnionOperator(BinaryExpression expression, string operatorString)
        {
            _jql.Append("(");
            Visit(expression.Left);
            _jql.Append(operatorString);
            Visit(expression.Right);
            _jql.Append(")");
        }


        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.GreaterThan:
                    ProcessNumbericOperatorExpression(node, Operators.GREATERTHAN);
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    ProcessNumbericOperatorExpression(node, Operators.GREATERTHANOREQUALS);
                    break;

                case ExpressionType.LessThan:
                    ProcessNumbericOperatorExpression(node, Operators.LESSTHAN);
                    break;

                case ExpressionType.LessThanOrEqual:
                    ProcessNumbericOperatorExpression(node, Operators.LESSTHANOREQUALS);
                    break;

                case ExpressionType.Equal:
                    ProcessEqualityOperator(node, true);
                    break;

                case ExpressionType.NotEqual:
                    ProcessEqualityOperator(node, false);
                    break;

                case ExpressionType.AndAlso:
                    ProcessUnionOperator(node, " and ");
                    break;

                case ExpressionType.OrElse:
                    ProcessUnionOperator(node, " or ");
                    break;

                default:
                    throw new NotSupportedException(String.Format("Expression type '{0}' is not supported.", node.NodeType));

            }

            return node;
        }

    }
}
