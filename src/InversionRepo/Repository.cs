using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using InversionRepo.Interfaces;
using InversionRepo.Extensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using LinqKit;

namespace InversionRepo
{
    public class Repository<TContext> : IRepository<TContext> where TContext : DbContext
    {
        public TContext Context { get => _context; }

        private readonly TContext _context;

        public Repository(TContext context)
        {
            _context = context;
        }

        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class, IEntity
        {
            return _context.Entry(entity);
        }

        public IListRequestBuilder<TEntity, TProjectedEntity, TContext> ProjectedListBuilder<TEntity, TProjectedEntity>(Expression<Func<TEntity, TProjectedEntity>> projection, IListRequest listRequest = null)
            where TEntity : class, IEntity
        {
            var requestBuilder = new ListRequestBuilder<TEntity, TProjectedEntity, TContext>();
            requestBuilder.ListRequest = listRequest;
            requestBuilder.Projection = projection;
            requestBuilder.Context = _context;
            return requestBuilder;

        }

        public async Task<List<TProjectedEntity>> ProjectedList<TEntity, TProjectedEntity>(Expression<Func<TEntity, TProjectedEntity>> projection, Expression<Func<TProjectedEntity, bool>> predicate = null)
            where TEntity : class, IEntity
        {
            var query = Context.Set<TEntity>()
                .AsNoTracking()
                .AsExpandable()
                .Select(projection);

            if (predicate != null)
                query = query.Where(predicate);

            return await query.ToListAsync();
        }

        public async Task<TProjectedEntity> ProjectedGetById<TEntity, TProjectedEntity>(int? id, Expression<Func<TEntity, TProjectedEntity>> projection)
            where TEntity : class, IEntity
        {
            if (!id.HasValue || id == 0)
                return default;

            return await Context.Set<TEntity>()
                .Where(e => e.Id == id.Value)
                .AsNoTracking()
                .AsExpandable()
                .Select(projection)
                .SingleAsync();
        }

        public DbSet<T> Set<T>() where T : class, IEntity
        {
            return Context.Set<T>();
        }

        public async Task<T> GetById<T>(int? id) where T : class, IEntity
        {
            if (!id.HasValue || id == 0)
                return null;

            return await Context.Set<T>().SingleAsync(d => d.Id == id);
        }

        public IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate = null) where T : class, IEntity
        {
            return Context.Set<T>().Where(predicate);
        }

        public async Task<T> SaveEntity<T>(T entity) where T : class, IEntity
        {
            if (entity.Id == 0)
                _context.Add(entity);

            await _context.SaveChangesAsync();
            return entity;
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Remove(entity);
        }

        public async Task<int> Count<TEntity>()
            where TEntity : class, IEntity
        {
            return await Context.Set<TEntity>().CountAsync();
        }

        public bool IsEntityDeleted<TEntity>(TEntity entity) where TEntity : class
        {
            var entry = Context.Entry(entity);
            return entry.State == EntityState.Deleted;
        }

        public void LoadCollection<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression)
            where TEntity : class
            where TProperty : class
        {
            Context.Entry(entity).Collection(propertyExpression).Load();
        }
        public void LoadReference<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : class
            where TProperty : class
        {
            Context.Entry(entity).Reference(propertyExpression).Load();
        }

        public void ResetChanges(List<object> exceptionEntities = default)
        {
            foreach (var entry in Context.ChangeTracker.Entries())
            {
                if (!exceptionEntities.Any(e => e == entry.Entity))
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            entry.State = EntityState.Unchanged;
                            break;
                        case EntityState.Added:
                            entry.State = EntityState.Detached;
                            break;
                        case EntityState.Deleted:
                            entry.Reload();
                            break;
                        default: break;
                    }
                }
            }
        }
    }
}
