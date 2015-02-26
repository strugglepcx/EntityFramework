// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalDbSetExtensions
    {
        public static RelationalDbSet<TEntity> AsRelational<TEntity>([NotNull]this DbSet<TEntity> dbSet) where TEntity :class
        {
            Check.NotNull(dbSet, nameof(dbSet));

            return new RelationalDbSet<TEntity>(dbSet);
        }
    }
}
