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
        /// <summary>
        /// Adds a Where clause (predicate) based on the projected type
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IListRequestBuilder<TEntity, TProjectedEntity, TContext> Where(Expression<Func<TProjectedEntity, bool>> predicate);
        /// <summary>
        /// Adds a Where clause (predicate) based on the entity type
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IListRequestBuilder<TEntity, TProjectedEntity, TContext> WhereEntity(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Adds a conditional order based on the ListRequest, applied on the projection fields
        /// </summary>
        /// <param name="listRequestOrderField"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        IListRequestBuilder<TEntity, TProjectedEntity, TContext> ConditionalOrder(string listRequestOrderField, Expression<Func<TProjectedEntity, dynamic>> keySelector);
        /// <summary>
        /// Adds a conditional order based on the ListRequest, applied on the entity fields
        /// </summary>
        /// <param name="listRequestOrderField"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        IListRequestBuilder<TEntity, TProjectedEntity, TContext> ConditionalOrderEntity(string listRequestOrderField, Expression<Func<TEntity, dynamic>> keySelector);
        /// <summary>
        /// Adds ordering applicable to the projection fields
        /// </summary>
        /// <param name="keySelector"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        IListRequestBuilder<TEntity, TProjectedEntity, TContext> OrderBy(Expression<Func<TProjectedEntity, dynamic>> keySelector, bool descending = false);
        /// <summary>
        /// Adds ordering applicable to the entity fields
        /// </summary>
        /// <param name="keySelector"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        IListRequestBuilder<TEntity, TProjectedEntity, TContext> OrderByEntity(Expression<Func<TEntity, dynamic>> keySelector, bool descending = false);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<TProjectedEntity>> ExecuteAsync();
        Task<TProjectedEntity> FirstOrDefaultAsync(Expression<Func<TProjectedEntity, bool>> predicate = null);

        Task<int> CountAsync();
    }
}
