// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata.ModelConventions
{
    public class SqlServerKeyConvention : IKeyConvention
    {
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            var key = keyBuilder.Metadata;

            if (key.IsPrimaryKey() && key.Properties.Count == 1 && key.Properties.First().PropertyType.IsInteger())
            {
                var identityProperty = key.Properties.First();
                var entityBuilder = keyBuilder.ModelBuilder.Entity(identityProperty.EntityType.Name, ConfigurationSource.Convention);
                ConfigureDefaultValueGenerationOnProperty(entityBuilder.Property(identityProperty.PropertyType, identityProperty.Name, ConfigurationSource.Convention));
            }
            return keyBuilder;
        }

        protected virtual void ConfigureDefaultValueGenerationOnProperty([NotNull] InternalPropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Annotation(
                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration,
                SqlServerValueGenerationStrategy.Default.ToString(),
                ConfigurationSource.Convention);
        }
    }
}
