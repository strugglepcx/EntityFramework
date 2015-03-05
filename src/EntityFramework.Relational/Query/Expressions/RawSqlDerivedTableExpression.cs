﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class RawSqlDerivedTableExpression : TableExpressionBase
    {
        public RawSqlDerivedTableExpression(
            [NotNull] string rawSql,
            [NotNull] string alias,
            [NotNull] IQuerySource querySource)
            : base(
                  Check.NotNull(querySource, nameof(querySource)),
                  Check.NotEmpty(alias, nameof(alias)))
        {
            Check.NotEmpty(rawSql, nameof(rawSql));

            RawSql = rawSql;
        }

        public virtual string RawSql { get; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitRawSqlDerivedTableExpression(this)
                : base.Accept(visitor);
        }

        public override string ToString()
        {
            return RawSql + " " + Alias;
        }
    }
}
