using InversionRepo.Extensions;
using InversionRepo.Interfaces;
using LinqExpander;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace InversionRepo
{
    public class ListRequestBuilder<TEntity, TProjectedEntity, TContext> : IListRequestBuilder<TEntity, TProjectedEntity, TContext>
        where TEntity : class, IEntity
        where TContext : DbContext
    {
        internal IListRequest ListRequest { get; set; }
        internal Expression<Func<TEntity, TProjectedEntity>> Projection { get; set; }
        List<Expression<Func<TEntity, bool>>> Predicates { get; set; } = new List<Expression<Func<TEntity, bool>>>();
        internal TContext Context { get; set; }
        IQueryable<TEntity> Query { get; set; }

        List<(bool ascending, Expression<Func<TEntity, dynamic>> expression)> orderExpressions { get; set; } = new List<(bool ascending, Expression<Func<TEntity, dynamic>> expression)>();

        Dictionary<string, Expression<Func<TEntity, dynamic>>> conditionalOrderExpressions { get; set; } = new Dictionary<string, Expression<Func<TEntity, dynamic>>>();
        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> Where(Expression<Func<TEntity, bool>> predicate)
        {
            Predicates.Add(predicate);
            return this;
        }

        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> ConditionalOrder(string listRequestOrderField, Expression<Func<TEntity, dynamic>> keySelector)
        {
            if (conditionalOrderExpressions.ContainsKey(listRequestOrderField))
                throw new Exception($"Key {listRequestOrderField} was already add through ListRequestBuilder.ConditionalOrder.");

            conditionalOrderExpressions.Add(listRequestOrderField, keySelector);

            return this;
        }

        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> OrderBy(Expression<Func<TEntity, dynamic>> keySelector)
        {
            orderExpressions.Add((true, keySelector));

            return this;
        }

        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> OrderByDescending(Expression<Func<TEntity, dynamic>> keySelector)
        {
            orderExpressions.Add((false, keySelector));

            return this;
        }
        IQueryable<TEntity> CreateQuery()
        {
            if (Query == null)
            {
                Query = Context.Set<TEntity>()
                    .Include(Context.GetIncludePaths(typeof(TEntity)));

                foreach (var predicate in Predicates)
                {
                    Query = Query.Where(predicate);
                }
            }
            return Query;
        }

        public IQueryable<TEntity> BuildQuery()
        {
            var query = CreateQuery();

            foreach (var orderExpression in orderExpressions)
                query = (orderExpression.ascending ? query.OrderBy(orderExpression.expression) : query.OrderByDescending(orderExpression.expression));

            if (ListRequest != null)
            {
                if (!string.IsNullOrWhiteSpace(ListRequest.SortField))
                {
                    if (!conditionalOrderExpressions.ContainsKey(ListRequest.SortField))
                        throw new Exception($"The list request sent an unexpected SortField:{ListRequest.SortField}");

                    if (ListRequest.SortOrderAscending)
                        query = query.OrderBy(conditionalOrderExpressions[ListRequest.SortField]);
                    else
                        query = query.OrderByDescending(conditionalOrderExpressions[ListRequest.SortField]);
                }

                query = query.Paginate(ListRequest);
            }

            return query;
        }

        public async Task<List<TProjectedEntity>> ExecuteAsync()
        {
            var query = BuildQuery();

            return await query.AsNoTracking().AsExpandable().Select(Projection).ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            var query = CreateQuery();

            return await query.AsNoTracking().AsExpandable().CountAsync();
        }

        public async Task<TProjectedEntity> FirstOrDefaultAsync(Expression<Func<TProjectedEntity, bool>> predicate = null)
        {
            var query = BuildQuery();

            if (predicate == null)
                return await query.AsNoTracking().AsExpandable().Select(Projection).FirstOrDefaultAsync();
            else
                return await query.AsNoTracking().AsExpandable().Select(Projection).FirstOrDefaultAsync(predicate);
        }
    }
}
