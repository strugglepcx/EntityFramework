// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.

namespace Microsoft.Data.Entity.Relational
{
    public static class RelationalEntityServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddRelational([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            ((IAccessor<IServiceCollection>)builder).Service.TryAdd(new ServiceCollection()
                .AddSingleton<RelationalObjectArrayValueReaderFactory>()
                .AddSingleton<RelationalTypedValueReaderFactory>()
                .AddSingleton<ParameterNameGeneratorFactory>()
                .AddSingleton<ModificationCommandComparer>()
                .AddSingleton<MigrationIdGenerator>()
                .AddScoped<RelationalCustomQueryProvider>()
                .AddScoped<Migrator>()
                .AddScoped<MigrationAssembly>()
                .AddScoped(RelationalDataStoreServiceFactories.ModelDifferFactory)
                .AddScoped(RelationalDataStoreServiceFactories.HistoryRepositoryFactory)
                .AddScoped(RelationalDataStoreServiceFactories.MigrationSqlGeneratorFactory)
                .AddScoped(RelationalDataStoreServiceFactories.RelationalConnectionFactory));

            return builder;
        }
    }
}
