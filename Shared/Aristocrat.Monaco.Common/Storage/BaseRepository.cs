namespace Aristocrat.Monaco.Common.Storage
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Base implementation for repository.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public abstract class BaseRepository<T> : IRepository<T>
        where T : BaseEntity
    {
        /// <inheritdoc />
        public T Add(DbContext context, T entity)
        {
            var result = context.Set<T>().Add(entity);

            context.SaveChanges();

            return result as T;
        }

        /// <inheritdoc />
        public void Update(DbContext context, T entity)
        {
            context.Set<T>().Attach(entity);
            var entry = context.Entry(entity);
            entry.State = EntityState.Modified;
            context.SaveChanges();
        }

        /// <inheritdoc />
        public void Delete(DbContext context, T entity)
        {
            context.Set<T>().Remove(entity);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public void DeleteAll(DbContext context)
        {
            context.Set<T>().RemoveRange(context.Set<T>());
            context.SaveChanges();
        }

        /// <inheritdoc />
        public void DeleteAll(DbContext context, Expression<Func<T, bool>> predicate)
        {
            context.Set<T>().RemoveRange(context.Set<T>().Where(predicate));
            context.SaveChanges();
        }

        /// <inheritdoc />
        public void DeleteAll(DbContext context, IEnumerable<T> entities)
        {
            context.Set<T>().RemoveRange(entities);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public virtual IQueryable<T> GetAll(DbContext context)
        {
            return context.Set<T>().AsQueryable();
        }

        /// <inheritdoc />
        public virtual T Get(DbContext context, long id)
        {
            return context.Set<T>().First(entity => entity.Id == id);
        }

        /// <inheritdoc />
        public virtual IQueryable<T> Get(DbContext context, Expression<Func<T, bool>> predicate)
        {
            return predicate == null ? GetAll(context) : context.Set<T>().Where(predicate);
        }

        /// <inheritdoc />
        public T GetSingle(DbContext context)
        {
            return context.Set<T>().SingleOrDefault();
        }

        /// <inheritdoc />
        public virtual IQueryable<T> Get(DbContext context, int lastSequence, int totalEntries)
        {
            return context.Set<T>().OrderBy(x => x.Id).Skip(lastSequence).Take(totalEntries);
        }

        /// <inheritdoc />
        public virtual long GetMaxLastSequence<TSequence>(DbContext context)
            where TSequence : T, ILogSequence
        {
            return context.Set<TSequence>().AsQueryable().Max(x => (long?)x.Id) ?? 0;
        }

        /// <inheritdoc />
        public virtual long GetMaxLastSequence<TSequence>(DbContext context, Expression<Func<T, bool>> predicate)
            where TSequence : T, ILogSequence
        {
            return context.Set<TSequence>().Where(predicate).AsQueryable().Max(x => (long?)x.Id) ?? 0;
        }

        /// <inheritdoc />
        public virtual int Count(DbContext context)
        {
            return context.Set<T>().Count();
        }

        /// <inheritdoc />
        public int Count(DbContext context, Expression<Func<T, bool>> predicate)
        {
            return context.Set<T>().Count(predicate);
        }

        /// <inheritdoc />
        public virtual IQueryable<T> GetRange(DbContext context, long lastSequence, long totalEntries)
        {
            var sql = $"SELECT * FROM {context.GetTableName<T>()} ORDER BY [Id] ASC LIMIT @p0 OFFSET @p1";

            return context.Set<T>().FromSqlRaw(sql, totalEntries, lastSequence).AsQueryable();
        }
    }
}