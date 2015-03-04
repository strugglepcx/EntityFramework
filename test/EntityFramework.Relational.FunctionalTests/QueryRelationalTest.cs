// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Xunit;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public class QueryRelationalTest<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        public QueryRelationalTest(TFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public virtual void From_sql_queryable_simple()
        {
            AssertRelationalQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers"),
                cs => cs,
                entryCount: 91);
        }

        [Fact]
        public virtual void From_sql_queryable_filter()
        {
            AssertRelationalQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'"),
                cs => cs.Where(c => c.ContactName.Contains("z")),
                entryCount: 14);
        }

        [Fact]
        public virtual void From_sql_queryable_cached_by_query()
        {
            AssertRelationalQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.City = 'London'"),
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);

            AssertRelationalQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.City = 'Seattle'"),
                cs => cs.Where(c => c.City == "Seattle"),
                entryCount: 1);
        }

        [Fact]
        public virtual void From_sql_queryable_where_simple_closure_via_query_cache()
        {
            var title = "Sales Associate";

            AssertRelationalQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%o%'").Where(c => c.ContactTitle == title),
                cs => cs.Where(c => c.ContactName.Contains("o")).Where(c => c.ContactTitle == title),
                entryCount: 4);

            title = "Sales Manager";

            AssertRelationalQuery<Customer>(
                cs => cs.FromSql("SELECT * FROM Customers WHERE Customers.ContactName LIKE '%o%'").Where(c => c.ContactTitle == title),
                cs => cs.Where(c => c.ContactName.Contains("o")).Where(c => c.ContactTitle == title),
                entryCount: 7);
        }

        protected void AssertRelationalQuery<TItem>(
            Func<RelationalDbSet<TItem>, IQueryable<object>> relationalQuery,
            Func<IQueryable<TItem>, IQueryable<object>> l2oQuery,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                AssertResults(
                    l2oQuery(NorthwindData.Set<TItem>()).ToArray(),
                    relationalQuery(context.Set<TItem>().AsRelational()).ToArray(),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }
    }
}
