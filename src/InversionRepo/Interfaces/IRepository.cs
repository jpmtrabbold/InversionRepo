using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace InversionRepo.Interfaces
{
    public interface IRepository<TContext> where TContext : DbContext
    {
        TContext Context { get; }
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class, IEntity;

        IListRequestBuilder<TEntity, TProjectedEntity, TContext> ProjectedListBuilder<TEntity, TProjectedEntity>(Expression<Func<TEntity, TProjectedEntity>> projection, IListRequest listRequest = null)
            where TEntity : class, IEntity;

        Task<List<TProjectedEntity>> ProjectedList<TEntity, TProjectedEntity>(Expression<Func<TEntity, TProjectedEntity>> projection, Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class, IEntity;

        Task<TProjectedEntity> ProjectedGetById<TEntity, TProjectedEntity>(int? id, Expression<Func<TEntity, TProjectedEntity>> projection)
            where TEntity : class, IEntity;

        DbSet<T> Set<T>() where T : class, IEntity;

        Task<T> GetById<T>(int? id) where T : class, IEntity;
        IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate = null) where T : class, IEntity;
        Task SaveEntity<T>(T entity) where T : class, IEntity;
        void Remove<TEntity>(TEntity entity) where TEntity : class;
        Task<int> Count<TEntity>()
           where TEntity : class, IEntity;

        bool IsEntityDeleted<TEntity>(TEntity entity) where TEntity : class;

        void LoadCollection<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression)
           where TEntity : class
           where TProperty : class;

        void LoadReference<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : class
            where TProperty : class;
    }
}
