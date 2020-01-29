using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace LinqExpander
{
	internal class ExpandableQuery<T> : IQueryable<T>, IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
		private readonly ExtendableQueryProvider _provider;
        private readonly Expression _expression;

		public ExpandableQuery(ExtendableQueryProvider provider, Expression expression)
		{
			_provider = provider;
			_expression = expression;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _provider.ExecuteQuery<T>(_expression).GetEnumerator();
		}

        IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return _provider.ExecuteQuery<T>(_expression).ToAsyncEnumerable().GetAsyncEnumerator();
        }

        public Type ElementType => typeof(T);

        public Expression Expression => _expression;

        public IQueryProvider Provider => _provider;
    }

}
