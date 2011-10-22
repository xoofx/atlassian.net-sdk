﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Atlassian.Jira.Remote
{
    public class JqlExpressionVisitor: ExpressionVisitor, IJqlExpressionVisitor
    {
        private StringBuilder _jqlWhere;
        private StringBuilder _jqlOrderBy;
        private int? _numberOfResults;

        public string Jql 
        { 
            get
            {
                return _jqlWhere.ToString() + _jqlOrderBy.ToString();
            } 
        }

        public int? NumberOfResults
        {
            get
            {
                return _numberOfResults;
            }
        }

        public JqlData Process(Expression expression)
        {
            expression = ExpressionEvaluator.PartialEval(expression);
            _jqlWhere = new StringBuilder();
            _jqlOrderBy = new StringBuilder();

            this.Visit(expression);
            return new JqlData { Expression = Jql, NumberOfResults = _numberOfResults };
        }

        private string GetFieldName(PropertyInfo fieldInfo)
        {
            var attributes = fieldInfo.GetCustomAttributes(typeof(JqlFieldNameAttribute), true);
            if (attributes.Count() > 0)
            {
                return ((JqlFieldNameAttribute)attributes[0]).Name;
            }
            else
            {
                return fieldInfo.Name;
            }
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
            if (expression.Left is MemberExpression)
            {
                ProcessMemberEqualityOperator(expression, equal);
            }
            else if (expression.Left is MethodCallExpression)
            {
                ProcessIndexedMemberEqualityOperator(expression, equal);
            }
        }

        private void ProcessIndexedMemberEqualityOperator(BinaryExpression expression, bool equal)
        {
            var methodExpression = expression.Left as MethodCallExpression;

            // field
            _jqlWhere.Append(String.Format("\"{0}\"", ((ConstantExpression)methodExpression.Arguments[0]).Value));

            // operator
            _jqlWhere.Append(String.Format(" {0} ", equal ? JiraOperators.CONTAINS : JiraOperators.NOTCONTAINS));

            // value
            ProcessConstant(GetValueFromBinaryExpression(expression));
        }

        private void ProcessMemberEqualityOperator(BinaryExpression expression, bool equal)
        {
            var field = GetFieldFromBinaryExpression(expression);
            var value = GetValueFromBinaryExpression(expression);

            // field
            _jqlWhere.Append(GetFieldName(field));

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
            if(field.GetCustomAttributes(typeof(JqlContainsEqualityAttribute), true).Count() > 0)
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
            else if (node.Method.Name == "Take")
            {
                ProcessTake(node);
            }

            return base.VisitMethodCall(node) ;
        }

        private void ProcessTake(MethodCallExpression node)
        {
            _numberOfResults = int.Parse(((ConstantExpression)node.Arguments[1]).Value.ToString());
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