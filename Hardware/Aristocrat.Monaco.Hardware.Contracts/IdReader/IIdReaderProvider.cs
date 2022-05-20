namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using SharedDevice;

    /// <summary>
    ///     Provides a mechanism to retrieve and interact with the available ID readers.
    /// </summary>
    public interface IIdReaderProvider : IDeviceProvider<IIdReader>
    {
        /// <summary>Sets identifier validation.</summary>
        /// <param name="idReaderId">Identifier for the ID reader.</param>
        /// <param name="identity">The identity.</param>
        void SetIdValidation(int idReaderId, Identity identity);

        /// <summary>Sets validation as complete</summary>
        /// <param name="idReaderId">Identifier for the ID reader.</param>
        void SetValidationComplete(int idReaderId);

        /// <summary>Sets validation as failed.</summary>
        /// <param name="idReaderId">Identifier for the ID reader.</param>
        void SetValidationFailed(int idReaderId);
    }
}
