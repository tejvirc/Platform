namespace Aristocrat.Monaco.Common.Storage
{
    /// <summary>
    ///     Generic implementation of repository for direct usage of generic interfaces of Repository.
    /// </summary>
    /// <typeparam name="T">Type of entity.</typeparam>
    /// <seealso cref="Aristocrat.Monaco.Common.Storage.BaseRepository{T}" />
    /// <seealso cref="Aristocrat.Monaco.Common.Storage.IRepository{T}" />
    public class Repository<T> : BaseRepository<T>
        where T : BaseEntity
    {
    }
}