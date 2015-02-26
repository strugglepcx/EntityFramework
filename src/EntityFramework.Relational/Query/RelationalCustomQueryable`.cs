// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalCustomQueryable<TEntity> : QueryableBase<TEntity>, IRelationalCustomQueryable
    {
        public virtual string Query { get; private set; }

        public RelationalCustomQueryable([NotNull] IQueryProvider provider, [NotNull] string query)
        : base(Check.NotNull(provider, nameof(provider)))
        {
            Check.NotNull(query, nameof(query));
            Query = query;
        }

        public RelationalCustomQueryable([NotNull] IQueryProvider provider, [NotNull] Expression expression)
        : base(
            Check.NotNull(provider, nameof(provider)),
            Check.NotNull(expression, nameof(expression)))
        {
        }

        public override string ToString()
        {
            return Query;
        }
    }
}
