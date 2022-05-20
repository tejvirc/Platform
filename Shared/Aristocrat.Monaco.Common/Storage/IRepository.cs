namespace Aristocrat.Monaco.Common.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Base interface for repository.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T>
        where T : BaseEntity
    {
        /// <summary>
        ///     Adds new entity to database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="entity">Entity instance.</param>
        /// <returns>the entity</returns>
        T Add(DbContext context, T entity);

        /// <summary>
        ///     Updates entity in database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="entity">Entity instance.</param>
        void Update(DbContext context, T entity);

        /// <summary>
        ///     Deletes specified entity from database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="entity">Entity instance.</param>
        void Delete(DbContext context, T entity);

        /// <summary>
        ///     Deletes all entities from database.
        /// </summary>
        /// <param name="context">The database context.</param>
        void DeleteAll(DbContext context);

        /// <summary>
        ///     Deletes all entities from database that specifies criteria in predicate.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="predicate">Delete only the qualified entities.</param>
        void DeleteAll(DbContext context, Expression<Func<T, bool>> predicate);

        /// <summary>
        ///     Removes the given collection of entities from the context.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="entities">The collection of entities to delete.</param>
        void DeleteAll(DbContext context, IEnumerable<T> entities);

        /// <summary>
        ///     Gets all entities of repository from database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>Returns all entities of repository from database.</returns>
        IQueryable<T> GetAll(DbContext context);

        /// <summary>
        ///     Gets entity by its unique identifier.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="id">Entity unique identifier.</param>
        /// <returns>Returns entity instance or null.</returns>
        T Get(DbContext context, long id);

        /// <summary>
        ///     Gets entity by using specified search predicate function.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="predicate">Search predicate function.</param>
        /// <returns>Returns all entities that match search predicate or empty collection.</returns>
        IQueryable<T> Get(DbContext context, Expression<Func<T, bool>> predicate);

        /// <summary>
        ///     Gets single entity
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>Returns single entity of repository from database.</returns>
        T GetSingle(DbContext context);

        /// <summary>
        ///     Gets the specified last sequence.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="lastSequence">The last sequence.</param>
        /// <param name="totalEntries">The total entries.</param>
        /// <returns>A queryable collection</returns>
        IQueryable<T> Get(DbContext context, int lastSequence, int totalEntries);

        /// <summary>
        ///     Gets the maximum last sequence.
        /// </summary>
        /// <typeparam name="TSequence">The type that is an <see cref="ILogSequence" /></typeparam>
        /// <param name="context">The database context.</param>
        /// <returns>the last sequence</returns>
        long GetMaxLastSequence<TSequence>(DbContext context)
            where TSequence : T, ILogSequence;


        /// <summary>
        ///     Gets the maximum last sequence.
        /// </summary>
        /// <typeparam name="TSequence">The type that is an <see cref="ILogSequence" /></typeparam>
        /// <param name="context">The database context.</param>
        /// <param name="predicate">Search predicate function.</param>
        /// <returns>the last sequence</returns>
        long GetMaxLastSequence<TSequence>(DbContext context, Expression<Func<T, bool>> predicate)
            where TSequence : T, ILogSequence;

        /// <summary>
        ///     Gets the count of entities.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>the count</returns>
        int Count(DbContext context);

        /// <summary>
        ///     Gets the count of entities.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="predicate">Search predicate function.</param>
        /// <returns>the count</returns>
        int Count(DbContext context, Expression<Func<T, bool>> predicate);

        /// <summary>
        ///     Gets the specified last sequence.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="lastSequence">The last sequence.</param>
        /// <param name="totalEntries">The total entries.</param>
        /// <returns>a collection</returns>
        IQueryable<T> GetRange(DbContext context, long lastSequence, long totalEntries);
    }
}