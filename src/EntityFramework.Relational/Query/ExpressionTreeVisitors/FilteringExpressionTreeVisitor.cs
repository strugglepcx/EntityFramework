// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class FilteringExpressionTreeVisitor : ThrowingExpressionTreeVisitor
    {
        private readonly RelationalQueryModelVisitor _queryModelVisitor;

        private bool _requiresClientEval;

        public FilteringExpressionTreeVisitor([NotNull] RelationalQueryModelVisitor queryModelVisitor)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
        }

        public virtual bool RequiresClientEval => _requiresClientEval;

        protected override Expression VisitBinaryExpression([NotNull] BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                {
                    var structuralComparisonExpression
                        = UnfoldStructuralComparison(
                            binaryExpression.NodeType,
                            ProcessComparisonExpression(binaryExpression));

                    return structuralComparisonExpression;
                }
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                {
                    return ProcessComparisonExpression(binaryExpression);
                }

                case ExpressionType.AndAlso:
                {
                    var left = VisitExpression(binaryExpression.Left);
                    var right = VisitExpression(binaryExpression.Right);

                    return left != null
                           && right != null
                        ? Expression.AndAlso(left, right)
                        : (left ?? right);
                }

                case ExpressionType.OrElse:
                {
                    var left = VisitExpression(binaryExpression.Left);
                    var right = VisitExpression(binaryExpression.Right);

                    return left != null
                           && right != null
                        ? Expression.OrElse(left, right)
                        : null;
                }
            }

            _requiresClientEval = true;

            return null;
        }

        private static Expression UnfoldStructuralComparison(ExpressionType expressionType, Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;
            var leftConstantExpression = binaryExpression?.Left as ConstantExpression;
            var leftExpressions = leftConstantExpression?.Value as Expression[];

            if (leftExpressions != null)
            {
                var rightConstantExpression = binaryExpression.Right as ConstantExpression;

                var rightExpressions = rightConstantExpression?.Value as Expression[];

                if (rightExpressions != null
                    && leftExpressions.Length == rightExpressions.Length)
                {
                    return leftExpressions
                        .Zip(rightExpressions, (l, r) =>
                            Expression.MakeBinary(expressionType, l, r))
                        .Aggregate((e1, e2) =>
                            expressionType == ExpressionType.Equal
                                ? Expression.AndAlso(e1, e2)
                                : Expression.OrElse(e1, e2));
                }
            }

            return expression;
        }

        private Expression ProcessComparisonExpression(BinaryExpression binaryExpression)
        {
            var leftExpression = VisitExpression(binaryExpression.Left);
            var rightExpression = VisitExpression(binaryExpression.Right);

            if (leftExpression == null
                || rightExpression == null)
            {
                return null;
            }

            var nullExpression
                = TransformNullComparison(leftExpression, rightExpression, binaryExpression.NodeType);

            return nullExpression
                   ?? Expression.MakeBinary(binaryExpression.NodeType, leftExpression, rightExpression);
        }

        private static Expression TransformNullComparison(
            Expression left, Expression right, ExpressionType expressionType)
        {
            if (expressionType == ExpressionType.Equal
                || expressionType == ExpressionType.NotEqual)
            {
                var constantExpression
                    = right as ConstantExpression
                      ?? left as ConstantExpression;

                if (constantExpression != null
                    && constantExpression.Value == null)
                {
                    var columnExpression
                        = left as ColumnExpression
                          ?? right as ColumnExpression;

                    if (columnExpression != null)
                    {
                        return expressionType == ExpressionType.Equal
                            ? (Expression)new IsNullExpression(columnExpression)
                            : new IsNotNullExpression(columnExpression);
                    }
                }
            }

            return null;
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var operand = VisitExpression(methodCallExpression.Object);

            if (operand != null)
            {
                var arguments
                    = methodCallExpression.Arguments
                        .Select(VisitExpression)
                        .Where(e => e != null)
                        .ToArray();

                if (arguments.Length == methodCallExpression.Arguments.Count)
                {
                    var boundExpression
                        = Expression.Call(
                            operand,
                            methodCallExpression.Method,
                            arguments);

                    var translatedMethodExpression
                        = _queryModelVisitor.QueryCompilationContext.MethodCallTranslator
                            .Translate(boundExpression);

                    if (translatedMethodExpression != null)
                    {
                        return translatedMethodExpression;
                    }
                }
            }
            else
            {
                var columnExpression
                    = _queryModelVisitor
                        .BindMethodCallExpression(
                            methodCallExpression,
                            (property, querySource, selectExpression)
                                => new ColumnExpression(
                                    _queryModelVisitor.QueryCompilationContext
                                        .GetColumnName(property),
                                    property,
                                    selectExpression.FindTableForQuerySource(querySource)));

                if (columnExpression != null)
                {
                    return columnExpression;
                }
            }

            _requiresClientEval = true;

            return null;
        }

        protected override Expression VisitMemberExpression([NotNull] MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            var columnExpression
                = _queryModelVisitor
                    .BindMemberExpression(
                        memberExpression,
                        (property, querySource, selectExpression)
                            => new ColumnExpression(
                                _queryModelVisitor.QueryCompilationContext
                                    .GetColumnName(property),
                                property,
                                selectExpression.FindTableForQuerySource(querySource)));

            if (columnExpression != null)
            {
                return columnExpression;
            }

            _requiresClientEval = true;

            return null;
        }

        protected override Expression VisitUnaryExpression([NotNull] UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                var operand = VisitExpression(expression.Operand);

                if (operand != null)
                {
                    var inExpression = operand as InExpression;
                    if (inExpression != null)
                    {
                        return new NotInExpression(
                            inExpression.Column, 
                            inExpression.Values, 
                            inExpression.ParameterArgument);
                    }

                    return Expression.Not(operand);
                }
            }
            else if (expression.NodeType == ExpressionType.Convert)
            {
                var operand = VisitExpression(expression.Operand);

                if (operand != null)
                {
                    return Expression.Convert(operand, expression.Type);
                }
            }

            _requiresClientEval = true;

            return null;
        }

        protected override Expression VisitNewExpression([NotNull] NewExpression newExpression)
        {
            Check.NotNull(newExpression, nameof(newExpression));

            if (newExpression.Members != null
                && newExpression.Arguments.Any()
                && newExpression.Arguments.Count == newExpression.Members.Count)
            {
                var memberBindings
                    = newExpression.Arguments
                        .Select(VisitExpression)
                        .Where(e => e != null)
                        .ToArray();

                if (memberBindings.Length == newExpression.Arguments.Count)
                {
                    return Expression.Constant(memberBindings);
                }
            }

            _requiresClientEval = true;

            return null;
        }

        private static readonly Type[] _supportedConstantTypes =
            {
                typeof(bool),
                typeof(byte),
                typeof(byte[]),
                typeof(char),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(double),
                typeof(float),
                typeof(Guid),
                typeof(int),
                typeof(long),
                typeof(sbyte),
                typeof(short),
                typeof(string),
                typeof(uint),
                typeof(ulong),
                typeof(ushort)
            };

        protected override Expression VisitConstantExpression([NotNull] ConstantExpression constantExpression)
        {
            Check.NotNull(constantExpression, nameof(constantExpression));

            if (constantExpression.Value == null)
            {
                return constantExpression;
            }

            var underlyingType = constantExpression.Type.UnwrapNullableType().UnwrapEnumType();

            if (_supportedConstantTypes.Contains(underlyingType))
            {
                return constantExpression;
            }

            _requiresClientEval = true;

            return null;
        }

        protected override Expression VisitParameterExpression([NotNull] ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            var underlyingType = parameterExpression.Type.UnwrapNullableType().UnwrapEnumType();

            if (_supportedConstantTypes.Contains(underlyingType))
            {
                return parameterExpression;
            }

            _requiresClientEval = true;

            return null;
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.QueryModel.IsIdentityQuery()
                && expression.QueryModel.ResultOperators.Count == 1)
            {
                var contains = expression.QueryModel.ResultOperators.Single() as ContainsResultOperator;
                if (contains != null)
                {
                    var parameter = expression.QueryModel.MainFromClause.FromExpression as ParameterExpression;
                    var memberItem = contains.Item as MemberExpression;
                    if (parameter != null && memberItem != null)
                    {
                        var columnExpression = (ColumnExpression)VisitMemberExpression(memberItem);
                        _requiresClientEval = false;

                        return new InExpression(columnExpression, parameter);
                    }
                }
            }

            _requiresClientEval = true;

            return null;
        }

        protected override TResult VisitUnhandledItem<TItem, TResult>(
            TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
        {
            _requiresClientEval = true;

            return default(TResult);
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            return null; // never called
        }
    }
}
