namespace Aristocrat.Monaco.Protocol.Common.Storage.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using Monaco.Common.Storage;
    using Queries;

    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity>
        where TEntity : BaseEntity
    {
        /// <summary>
        ///     
        /// </summary>
        DbContext Context { get; }

        /// <summary>
        ///     
        /// </summary>
        DbSet<TEntity> Entities { get; }

        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        IQuery<TEntity> Query();

        /// <summary>
        ///     
        /// </summary>
        /// <param name="queryObject"></param>
        /// <returns></returns>
        IQuery<TEntity> Query(IQueryObject<TEntity> queryObject);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IQuery<TEntity> Query(Expression<Func<TEntity, bool>> query);

        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> Queryable();

        /// <summary>
        ///     
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        TEntity Find(params object[] keyValues);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IQueryable<TEntity> SelectQuery(string query, params object[] parameters);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="entity"></param>
        void Add(TEntity entity);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="entities"></param>
        void AddRange(IEnumerable<TEntity> entities);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="entity"></param>
        void Update(TEntity entity);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="entity"></param>
        void AddOrUpdate(TEntity entity);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="entity"></param>
        void Delete(TEntity entity);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="keyValues"></param>
        void Delete(params object[] keyValues);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="id"></param>
        void Delete(long id);

        /// <summary>
        /// 
        /// </summary>
        void ClearTable();
    }
}
