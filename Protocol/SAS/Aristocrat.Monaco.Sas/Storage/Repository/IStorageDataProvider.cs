namespace Aristocrat.Monaco.Sas.Storage.Repository
{
    using System;
    using System.Threading.Tasks;
    using Common.Storage;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     The storage data provider
    /// </summary>
    /// <typeparam name="TEntity">The persistence entity this provider is for</typeparam>
    public interface IStorageDataProvider<TEntity> where TEntity : BaseEntity, ICloneable, new()
    {
        /// <summary>
        ///     Gets the data for this storage item
        /// </summary>
        /// <returns>The entity from storage</returns>
        TEntity GetData();

        /// <summary>
        ///     Saves the data into persistence
        /// </summary>
        /// <param name="entity">The entity to save</param>
        /// <returns>The saving task</returns>
        Task Save(TEntity entity);

        /// <summary>
        ///     Saves the entity into persistence
        /// </summary>
        /// <param name="entity">The entity to save</param>
        /// <param name="work">The unit of work to use</param>
        void Save(TEntity entity, IUnitOfWork work);
    }
}