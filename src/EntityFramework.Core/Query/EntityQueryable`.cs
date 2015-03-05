// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryable<TResult>
        : QueryableBase<TResult>, IAsyncEnumerable<TResult>, IEntityQueryable
    {
        private readonly LazyRef<Annotatable> _annotatable
            = new LazyRef<Annotatable>(
                () => new Annotatable());

        public EntityQueryable([NotNull] EntityQueryProvider provider)
            : base(Check.NotNull(provider, nameof(provider)))
        {
        }

        public EntityQueryable([NotNull] EntityQueryProvider provider, [NotNull] Expression expression)
            : base(
                Check.NotNull(provider, nameof(provider)),
                Check.NotNull(expression, nameof(expression)))
        {
        }

        IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator()
        {
            return ((IAsyncQueryProvider)Provider).ExecuteAsync<TResult>(Expression).GetEnumerator();
        }

        public virtual Annotation AddAnnotation([NotNull] string annotationName, [NotNull] string value)
        {
            Check.NotNull(annotationName, nameof(annotationName));
            Check.NotNull(value, nameof(value));

            return _annotatable.Value.AddAnnotation(annotationName, value);
        }

        public virtual string this[[NotNull]string annotationName]
        {
            get
            {
                Check.NotNull(annotationName, annotationName);
                return _annotatable.Value[annotationName];
            }
        }

        public virtual IEnumerable<IAnnotation> Annotations
        {
            get
            {
                return _annotatable.Value.Annotations;
            }
        }

        public virtual Annotation GetAnnotation([NotNull]string annotationName)
        {
            Check.NotNull(annotationName, nameof(annotationName));

            return _annotatable.Value.GetAnnotation(annotationName);
        }

        public override string ToString()
        {
            return base.ToString() + string.Join(", ", _annotatable.Value.Annotations.Select(annotation => annotation.Value));
        }

        private class Annotatable : MetadataBase
        {
        }
    }
}
