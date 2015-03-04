﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.Data.Entity.Relational.Query
{
    public interface IRelationalCustomQueryable : IQueryable
    {
        string Query { get; }
    }
}
