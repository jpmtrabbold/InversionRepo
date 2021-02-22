using InversionRepo.Extensions;
using InversionRepo.Interfaces;

using LinqKit;
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
        internal TContext Context { get; set; }

        private List<Expression<Func<TProjectedEntity, bool>>> Predicates { get; set; } = new List<Expression<Func<TProjectedEntity, bool>>>();
        private List<Expression<Func<TEntity, bool>>> EntityPredicates { get; set; } = new List<Expression<Func<TEntity, bool>>>();

        private IQueryable<TProjectedEntity> Query { get; set; }
        private List<(bool descending, Expression<Func<TProjectedEntity, dynamic>> expression)> OrderExpressions { get; set; } = new List<(bool descending, Expression<Func<TProjectedEntity, dynamic>> expression)>();
        private List<(bool descending, Expression<Func<TEntity, dynamic>> expression)> EntityOrderExpressions { get; set; } = new List<(bool descending, Expression<Func<TEntity, dynamic>> expression)>();

        Dictionary<string, Expression<Func<TProjectedEntity, dynamic>>> ConditionalOrderExpressions { get; set; } = new Dictionary<string, Expression<Func<TProjectedEntity, dynamic>>>();
        Dictionary<string, Expression<Func<TEntity, dynamic>>> EntityConditionalOrderExpressions { get; set; } = new Dictionary<string, Expression<Func<TEntity, dynamic>>>();


        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> Where(Expression<Func<TProjectedEntity, bool>> predicate)
        {
            Predicates.Add(predicate);
            ResetQuery();

            return this;
        }
        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> WhereEntity(Expression<Func<TEntity, bool>> predicate)
        {
            EntityPredicates.Add(predicate);
            ResetQuery();

            return this;
        }

        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> ConditionalOrder(string listRequestOrderField, Expression<Func<TProjectedEntity, dynamic>> keySelector)
        {
            if (ConditionalOrderExpressions.ContainsKey(listRequestOrderField))
                throw new Exception($"Key {listRequestOrderField} was already add through ListRequestBuilder.ConditionalOrder.");

            ConditionalOrderExpressions.Add(listRequestOrderField, keySelector);
            ResetQuery();

            return this;
        }
        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> ConditionalOrderEntity(string listRequestOrderField, Expression<Func<TEntity, dynamic>> keySelector)
        {
            if (EntityConditionalOrderExpressions.ContainsKey(listRequestOrderField))
                throw new Exception($"Key {listRequestOrderField} was already add through ListRequestBuilder.ConditionalOrderEntity.");

            EntityConditionalOrderExpressions.Add(listRequestOrderField, keySelector);

            ResetQuery();

            return this;
        }

        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> OrderBy(Expression<Func<TProjectedEntity, dynamic>> keySelector, bool descending = false)
        {
            OrderExpressions.Add((descending, keySelector));
            ResetQuery();

            return this;
        }
        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> OrderByEntity(Expression<Func<TEntity, dynamic>> keySelector, bool descending = false)
        {
            EntityOrderExpressions.Add((descending, keySelector));
            ResetQuery();

            return this;
        }

        private void ResetQuery()
        {
            if (Query != null)
                Query = null;
        }
        IQueryable<TProjectedEntity> CreateQuery()
        {
            if (Query == null)
            {
                if (!string.IsNullOrWhiteSpace(ListRequest?.SortField) &&
                    !ConditionalOrderExpressions.ContainsKey(ListRequest?.SortField) &&
                    !EntityConditionalOrderExpressions.ContainsKey(ListRequest?.SortField))

                    throw new Exception($"The list request sent an unexpected SortField: {ListRequest?.SortField}");

                IQueryable<TEntity> preQuery;
                if (!EntityPredicates.Any() && !EntityOrderExpressions.Any() && !EntityConditionalOrderExpressions.Any())
                    preQuery = Context.Set<TEntity>().AsQueryable();
                else
                    preQuery = Context.Set<TEntity>().Include(Context.GetIncludePaths(typeof(TEntity)));
                preQuery = ApplyEntityConstraints(preQuery);

                Query = preQuery.AsNoTracking().AsExpandable().Select(Projection);

                Query = ApplyProjectionConstraints(Query);
            }
            return Query;
        }

        private IQueryable<TProjectedEntity> ApplyProjectionConstraints(IQueryable<TProjectedEntity> projectionQuery)
        {
            foreach (var predicate in Predicates)
                projectionQuery = projectionQuery.Where(predicate);

            foreach (var orderExpression in OrderExpressions)
                projectionQuery = (orderExpression.descending
                    ? projectionQuery.OrderByDescending(orderExpression.expression)
                    : projectionQuery.OrderBy(orderExpression.expression));

            if (!string.IsNullOrWhiteSpace(ListRequest?.SortField) && ConditionalOrderExpressions.ContainsKey(ListRequest?.SortField))
            {
                if (ListRequest.SortOrderAscending)
                    projectionQuery = projectionQuery.OrderBy(ConditionalOrderExpressions[ListRequest.SortField]);
                else
                    projectionQuery = projectionQuery.OrderByDescending(ConditionalOrderExpressions[ListRequest.SortField]);
            }
            return projectionQuery;
        }

        private IQueryable<TEntity> ApplyEntityConstraints(IQueryable<TEntity> entityQuery)
        {
            foreach (var predicate in EntityPredicates)
                entityQuery = entityQuery.Where(predicate);

            foreach (var orderExpression in EntityOrderExpressions)
                entityQuery = (orderExpression.descending
                    ? entityQuery.OrderByDescending(orderExpression.expression)
                    : entityQuery.OrderBy(orderExpression.expression));

            if (!string.IsNullOrWhiteSpace(ListRequest?.SortField) && EntityConditionalOrderExpressions.ContainsKey(ListRequest?.SortField))
            {
                if (ListRequest.SortOrderAscending)
                    entityQuery = entityQuery.OrderBy(EntityConditionalOrderExpressions[ListRequest.SortField]);
                else
                    entityQuery = entityQuery.OrderByDescending(EntityConditionalOrderExpressions[ListRequest.SortField]);
            }

            return entityQuery;
        }

        private IQueryable<TProjectedEntity> BuildPaginatedQuery()
        {
            var query = CreateQuery();

            if (ListRequest != null)
                query = query.Paginate(ListRequest);

            return query;
        }

        public async Task<List<TProjectedEntity>> ExecuteAsync()
        {
            var query = BuildPaginatedQuery();

            return await query.ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            var query = CreateQuery();

            return await query.CountAsync();
        }

        public async Task<TProjectedEntity> FirstOrDefaultAsync(Expression<Func<TProjectedEntity, bool>> predicate = null)
        {
            var query = BuildPaginatedQuery();

            if (predicate == null)
                return await query.FirstOrDefaultAsync();
            else
                return await query.FirstOrDefaultAsync(predicate);
        }
    }
}
