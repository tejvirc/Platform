namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Interface to for entities that have name and Id.
    /// </summary>
    public interface INamedId
    {
        /// <summary>
        ///     Id of the entity.
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     Name of the entity.
        /// </summary>
        string Name { get; }
    }
}