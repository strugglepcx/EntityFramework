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

        protected const string customQueryableSimple = "SELECT * FROM Customers";
        [Fact]
        public virtual void Custom_queryable_simple()
        {
            AssertCustomQuery<Customer>(
                cs => cs.Query(customQueryableSimple),
                cs => cs,
                entryCount: 91);
        }

        protected const string customQueryableFilterContactNamePattern = "z";
        protected const string customQueryableFilter = "SELECT * FROM Customers WHERE Customers.ContactName LIKE '%"
            + customQueryableFilterContactNamePattern + "%'";
        [Fact]
        public virtual void Custom_queryable_filter()
        {
            AssertCustomQuery<Customer>(
                cs => cs.Query(customQueryableFilter),
                cs => cs.Where(c => c.ContactName.Contains(customQueryableFilterContactNamePattern)),
                entryCount: 14);
        }

        protected const string customQueryableCachedByQueryFirstCity = "London";
        protected const string customQueryableCachedByQueryFirst = "SELECT * FROM Customers WHERE Customers.City = '"
            + customQueryableCachedByQueryFirstCity + "'";
        protected const string customQueryableCachedByQuerySecondCity = "Seattle";
        protected const string customQueryableCachedByQuerySecond = "SELECT * FROM Customers WHERE Customers.City = '"
            + customQueryableCachedByQuerySecondCity + "'";
        [Fact]
        public virtual void Custom_queryable_cached_by_query()
        {
            AssertCustomQuery<Customer>(
                cs => cs.Query(customQueryableCachedByQueryFirst),
                cs => cs.Where(c => c.City == customQueryableCachedByQueryFirstCity),
                entryCount: 6);

            AssertCustomQuery<Customer>(
                cs => cs.Query(customQueryableCachedByQuerySecond),
                cs => cs.Where(c => c.City == customQueryableCachedByQuerySecondCity),
                entryCount: 1);
        }

        protected const string customQueryableWhereSimpleClosureViaQueryCacheContactNamePattern = "o";
        protected const string customQueryableWhereSimpleClosureViaQueryCache = "SELECT * FROM Customers WHERE Customers.ContactName LIKE '%"
            + customQueryableWhereSimpleClosureViaQueryCacheContactNamePattern + "%'";
        [Fact]
        public virtual void Custom_queryable_where_simple_closure_via_query_cache()
        {
            var title = "Sales Associate";

            AssertCustomQuery<Customer>(
                cs => cs.Query(customQueryableWhereSimpleClosureViaQueryCache).Where(c => c.ContactTitle == title),
                cs => cs.Where(c => c.ContactName.Contains(customQueryableWhereSimpleClosureViaQueryCacheContactNamePattern)).Where(c => c.ContactTitle == title),
                entryCount: 4);

            title = "Sales Manager";

            AssertCustomQuery<Customer>(
                cs => cs.Query(customQueryableWhereSimpleClosureViaQueryCache).Where(c => c.ContactTitle == title),
                cs => cs.Where(c => c.ContactName.Contains(customQueryableWhereSimpleClosureViaQueryCacheContactNamePattern)).Where(c => c.ContactTitle == title),
                entryCount: 7);
        }

        protected void AssertCustomQuery<TItem>(
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
