// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalDbSet<TEntity> where TEntity : class
    {
        private readonly LazyRef<EntityQueryProvider> _queryProvider;

        public RelationalDbSet([NotNull]DbSet<TEntity> dbSet)
        {
            Check.NotNull(dbSet, nameof(dbSet));

            _queryProvider
                = new LazyRef<EntityQueryProvider>(
                    () => ((IAccessor<IServiceProvider>)dbSet).Service.GetRequiredService<EntityQueryProvider>());
        }

        public virtual IQueryable<TEntity> FromSql([NotNull]string query)
        {
            var queryable = new EntityQueryable<TEntity>(_queryProvider.Value);
            queryable.AddAnnotation("sql", query);

            return _queryProvider.Value.CreateQuery<TEntity>(Expression.Constant(queryable));
        }
    }
}
