// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalDbSet<TEntity> where TEntity : class
    {
        private readonly IServiceProvider _serviceProvider;

        public RelationalDbSet([NotNull]DbSet<TEntity> dbSet)
        {
            Check.NotNull(dbSet, nameof(dbSet));

            _serviceProvider = ((IAccessor<IServiceProvider>)dbSet).Service;
        }

        public virtual IQueryable<TEntity> Query([NotNull]string query)
        {
            return new RelationalCustomQueryable<TEntity>(
                _serviceProvider.GetRequiredService<RelationalCustomQueryProvider>(),
                query);
        }
    }
}
