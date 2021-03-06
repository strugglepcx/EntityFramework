// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDataStoreSource : DataStoreSource<ISqlServerDataStoreServices, SqlServerOptionsExtension>
    {
        public SqlServerDataStoreSource([NotNull] DbContextServices services, [NotNull] IDbContextOptions options)
            : base(services, options)
        {
        }

        public override string Name => typeof(SqlServerDataStore).Name;

        public override void AutoConfigure() => ContextOptions.UseSqlServer();
    }
}
