namespace Aristocrat.Monaco.Protocol.Common.Storage.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using LinqKit;
    using Monaco.Common.Storage;
    using Queries;

    /// <summary>
    ///     Manages operations on the <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
        /// </summary>
        /// <param name="context"></param>
        public Repository(DbContext context)
        {
            Context = context;
            Entities = context.Set<TEntity>();
        }

        /// <inheritdoc />
        public DbContext Context { get; }

        /// <inheritdoc />
        public DbSet<TEntity> Entities { get; }

        /// <inheritdoc />
        public IQuery<TEntity> Query() => new Query<TEntity>(this);

        /// <inheritdoc />
        public virtual IQuery<TEntity> Query(IQueryObject<TEntity> queryObject) => new Query<TEntity>(this, queryObject);

        /// <inheritdoc />
        public virtual IQuery<TEntity> Query(Expression<Func<TEntity, bool>> query) => new Query<TEntity>(this, query);

        /// <inheritdoc />
        public IQueryable<TEntity> Queryable() => Entities;

        /// <inheritdoc />
        public virtual TEntity Find(params object[] keyValues)
        {
            return Entities.Find(keyValues);
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters)
        {
            return Entities.SqlQuery(query, parameters).AsQueryable();
        }

        /// <inheritdoc />
        public virtual void Add(TEntity entity)
        {
            Entities.Add(entity);
            Context.SaveChanges();
        }

        /// <inheritdoc />
        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            Entities.AddRange(entities);
            Context.SaveChanges();
        }

        /// <inheritdoc />
        public virtual void Update(TEntity entity)
        {
            Entities.Attach(entity);
            var entry = Context.Entry(entity);
            entry.State = EntityState.Modified;
            Context.SaveChanges();
        }

        /// <inheritdoc />
        public void AddOrUpdate(TEntity entity)
        {
            if (EqualityComparer<long>.Default.Equals(entity.Id, default(long)))
                Add(entity);
            else
                Update(entity);
        }

        /// <inheritdoc />
        public virtual void Delete(TEntity entity)
        {
            Entities.Remove(entity);
            Context.SaveChanges();
        }

        public void ClearTable()
        {
            Context.Database.ExecuteSqlCommand($"Delete from [{typeof(TEntity).Name}]");
            Context.SaveChanges();
        }

        /// <inheritdoc />
        public void Delete(params object[] keyValues)
        {
            var entity = Entities.Find(keyValues);
            Delete(entity);
        }

        /// <inheritdoc />
        public virtual void Delete(long id)
        {
            var entity = Entities.Find(id);
            Delete(entity);
        }

        internal IQueryable<TEntity> Select(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<TEntity> query = Entities;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (filter != null)
            {
                query = query.AsExpandable().Where(filter);
            }

            if (page != null && pageSize != null)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return query;
        }
    }
}
