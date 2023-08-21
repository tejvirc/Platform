namespace Aristocrat.Monaco.Protocol.Common.Storage.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Builds a query expression to be execute against a repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public sealed class Query<TEntity> : IQuery<TEntity>
        where TEntity : BaseEntity
    {
        private readonly Expression<Func<TEntity, bool>> _expression;

        private readonly List<Expression<Func<TEntity, object>>> _includes;

        private readonly Protocol.Common.Storage.Repositories.Repository<TEntity> _repository;

        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _orderBy;

        /// <summary>
        ///     Initializes an instance of the <see cref="Query{TEntity}"/> class.
        /// </summary>
        /// <param name="repository"></param>
        public Query(Protocol.Common.Storage.Repositories.Repository<TEntity> repository)
        {
            _repository = repository;
            _includes = new List<Expression<Func<TEntity, object>>>();
        }

        /// <summary>
        ///     Initializes an instance of the <see cref="Query{TEntity}"/> class.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="queryObject"></param>
        public Query(Protocol.Common.Storage.Repositories.Repository<TEntity> repository, IQueryObject<TEntity> queryObject) : this(repository) { _expression = queryObject.Query(); }

        /// <summary>
        ///     Initializes an instance of the <see cref="Query{TEntity}"/> class.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="expression"></param>
        public Query(Protocol.Common.Storage.Repositories.Repository<TEntity> repository, Expression<Func<TEntity, bool>> expression) : this(repository) { _expression = expression; }

        /// <summary>
        ///     Initializes an instance of the <see cref="Query{TEntity}"/> class.
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public IQuery<TEntity> OrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            _orderBy = orderBy;
            return this;
        }

        /// <summary>
        ///     Adds an expression filter to the query.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQuery<TEntity> Include(Expression<Func<TEntity, object>> expression)
        {
            _includes.Add(expression);
            return this;
        }

        /// <summary>
        ///     Projects a set of elements of a sequence matching the filter criteria.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> SelectPage(int page, int pageSize, out int totalCount)
        {
            totalCount = _repository.Select(_expression).Count();

            return _repository.Select(_expression, _orderBy, _includes, page, pageSize);
        }

        /// <summary>
        ///     Projects each element of a sequence matching the filter criteria.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TEntity> Select() => _repository.Select(_expression, _orderBy, _includes);

        /// <summary>
        ///     Projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <returns></returns>
        public IEnumerable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector) { return _repository.Select(_expression, _orderBy, _includes).Select(selector); }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IQueryable<TEntity> SqlQuery(string query, params object[] parameters) => _repository.SelectQuery(query, parameters).AsQueryable();
    }
}
