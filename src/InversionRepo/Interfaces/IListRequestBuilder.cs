using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace InversionRepo.Interfaces
{
    public interface IListRequestBuilder<TEntity, TProjectedEntity, TContext>
        where TEntity : class, IEntity
        where TContext : DbContext
    {
        IListRequestBuilder<TEntity, TProjectedEntity, TContext> Where(Expression<Func<TEntity, bool>> predicate);

        IListRequestBuilder<TEntity, TProjectedEntity, TContext> ConditionalOrder(string listRequestOrderField, Expression<Func<TEntity, dynamic>> keySelector);
        IListRequestBuilder<TEntity, TProjectedEntity, TContext> OrderBy(Expression<Func<TEntity, dynamic>> keySelector);
        IListRequestBuilder<TEntity, TProjectedEntity, TContext> OrderByDescending(Expression<Func<TEntity, dynamic>> keySelector);
        Task<List<TProjectedEntity>> ExecuteAsync();
        Task<TProjectedEntity> FirstOrDefaultAsync(Expression<Func<TProjectedEntity, bool>> predicate = null);

        Task<int> CountAsync();
    }
}
