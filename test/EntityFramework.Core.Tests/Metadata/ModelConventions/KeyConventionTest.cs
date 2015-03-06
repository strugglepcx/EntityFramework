// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.ModelConventions
{
    public class KeyConventionTest
    {
        public class SampleEntity
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        [Fact]
        public void GenerateValueOnAdd_flag_is_set_for_key_properties()
        {
            var conventions = new ConventionSet();
            conventions.EntityTypeAddedConventions.Add(new PropertiesConvention());

            var modelBuilder = new InternalModelBuilder(new Model(), conventions);
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string>() { "Id", "Title" };
            var keyBuilder = entityBuilder.Key(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.NotNull(keyProperties[0].GenerateValueOnAdd);
            Assert.NotNull(keyProperties[1].GenerateValueOnAdd);

            Assert.True(keyProperties[0].GenerateValueOnAdd.Value);
            Assert.True(keyProperties[1].GenerateValueOnAdd.Value);
        }


        public class ReferencedEntity
        {
            public int Id { get; set; }
            public int SampleEntityId { get; set; }
        }

        [Fact]
        public void GenerateValueOnAdd_flag_is_not_set_for_foreign_key()
        {
            var conventions = new ConventionSet();
            conventions.EntityTypeAddedConventions.Add(new PropertiesConvention());
            conventions.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention());

            var modelBuilder = new InternalModelBuilder(new Model(), conventions);

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string>() { "SampleEntityId" };
            principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.Key(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Null(keyProperties[0].GenerateValueOnAdd);
        }

        [Fact]
        public void KeyConvention_does_not_override_GenerateValueOnAddFlag_when_configured_explicitly()
        {
            var conventions = new ConventionSet();
            conventions.EntityTypeAddedConventions.Add(new PropertiesConvention());

            var modelBuilder = new InternalModelBuilder(new Model(), conventions);
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string>() { "Id" };
            entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).GenerateValueOnAdd(false, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.Key(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].GenerateValueOnAdd.Value);
        }
    }
}
