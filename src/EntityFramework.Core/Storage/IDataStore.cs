// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public interface IDataStore
    {
        ILogger Logger { get; }
        IModel Model { get; }
        EntityKeyFactorySource EntityKeyFactorySource { get; }
        EntityMaterializerSource EntityMaterializerSource { get; }

        int SaveChanges([NotNull] IReadOnlyList<InternalEntityEntry> entries);

        Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<InternalEntityEntry> entries, 
            CancellationToken cancellationToken = default(CancellationToken));

        Func<QueryContext, IEnumerable<TResult>> CompileQuery<TResult>([NotNull] QueryModel queryModel);
        Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>([NotNull] QueryModel queryModel);
    }
}
