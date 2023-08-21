namespace Aristocrat.Monaco.Protocol.Common.Storage.Queries
{
    using System;
    using System.Linq.Expressions;
    using Monaco.Common.Storage;

    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IQueryObject<TEntity>
        where TEntity : BaseEntity
    {
        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        Expression<Func<TEntity, bool>> Query();

        /// <summary>
        ///     
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Expression<Func<TEntity, bool>> And(Expression<Func<TEntity, bool>> query);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Expression<Func<TEntity, bool>> Or(Expression<Func<TEntity, bool>> query);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="queryObject"></param>
        /// <returns></returns>
        Expression<Func<TEntity, bool>> And(IQueryObject<TEntity> queryObject);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="queryObject"></param>
        /// <returns></returns>
        Expression<Func<TEntity, bool>> Or(IQueryObject<TEntity> queryObject);
    }
}
