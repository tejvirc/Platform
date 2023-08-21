namespace Aristocrat.Monaco.Protocol.Common.Storage.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Monaco.Common.Storage;

    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IQuery<TEntity>
        where TEntity : BaseEntity
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        IQuery<TEntity> OrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IQuery<TEntity> Include(Expression<Func<TEntity, object>> expression);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        IEnumerable<TEntity> SelectPage(int page, int pageSize, out int totalCount);

        /// <summary>
        ///     
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        IEnumerable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector = null);

        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> Select();

        /// <summary>
        ///     
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IQueryable<TEntity> SqlQuery(string query, params object[] parameters);
    }
}
