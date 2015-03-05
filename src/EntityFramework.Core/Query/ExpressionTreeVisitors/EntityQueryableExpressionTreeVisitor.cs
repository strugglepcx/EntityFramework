// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public abstract class EntityQueryableExpressionTreeVisitor : DefaultQueryExpressionTreeVisitor
    {
        protected EntityQueryableExpressionTreeVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
            : base(Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)))
        {
        }

        protected override Expression VisitConstantExpression(ConstantExpression constantExpression)
        {
            var entityQueryable = constantExpression.Value as IEntityQueryable;

            if (entityQueryable != null)
            {
                return VisitEntityQueryable(entityQueryable.ElementType);
            }

            return constantExpression;
        }

        protected abstract Expression VisitEntityQueryable([NotNull] Type elementType);
    }
}
