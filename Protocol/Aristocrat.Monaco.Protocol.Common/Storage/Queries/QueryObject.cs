namespace Aristocrat.Monaco.Protocol.Common.Storage.Queries
{
    using System;
    using System.Linq.Expressions;
    using LinqKit;
    using Monaco.Common.Storage;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class QueryObject<TEntity> : IQueryObject<TEntity>
        where TEntity : BaseEntity
    {
        private Expression<Func<TEntity, bool>> _query;

        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        public virtual Expression<Func<TEntity, bool>> Query()
        {
            return _query;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> And(Expression<Func<TEntity, bool>> query)
        {
            return _query = _query == null ? query : _query.And(query.Expand());
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> Or(Expression<Func<TEntity, bool>> query)
        {
            return _query = _query == null ? query : _query.Or(query.Expand());
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="queryObject"></param>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> And(IQueryObject<TEntity> queryObject)
        {
            return And(queryObject.Query());
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="queryObject"></param>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> Or(IQueryObject<TEntity> queryObject)
        {
            return Or(queryObject.Query());
        }
    }
}
