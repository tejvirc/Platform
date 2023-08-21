namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    ///     Provides a mechanism to get a unique transaction identifier
    /// </summary>
    public interface IIdProvider
    {
        /// <summary>
        ///     Gets the transaction identifier for the EGM.
        /// </summary>
        /// <returns>The transaction id</returns>
        long CurrentTransactionId { get; }

        /// <summary>
        ///     Gets a unique transaction identifier for the EGM.
        /// </summary>
        /// <returns>The transaction id</returns>
        long GetNextTransactionId();

        /// <summary>
        ///     Gets the current unique log sequence for the specified type
        /// </summary>
        /// <typeparam name="T">The log type</typeparam>
        /// <returns>The log sequence number</returns>
        long GetCurrentLogSequence<T>()
            where T : class;

        /// <summary>
        ///     Gets the current unique log sequence for the specified type
        /// </summary>
        /// <param name="type">The log type</param>
        /// <returns>The log sequence number</returns>
        long GetCurrentLogSequence(Type type);

        /// <summary>
        ///     Gets the next unique log sequence for the specified type
        /// </summary>
        /// <typeparam name="T">The log type</typeparam>
        /// <returns>The log sequence number</returns>
        long GetNextLogSequence<T>()
            where T : class;

        /// <summary>
        ///     Gets the next unique log sequence for the specified type
        /// </summary>
        /// <typeparam name="T">The log type</typeparam>
        /// <param name="maxValue">Max value.</param>
        /// <returns>The log sequence number</returns>
        long GetNextLogSequence<T>(long maxValue)
            where T : class;
        
        /// <summary>
        ///     Gets the next unique log sequence for the specified type
        /// </summary>
        /// <param name="type">The log type</param>
        /// <returns>The log sequence number</returns>
        long GetNextLogSequence(Type type);

        /// <summary>
        ///     Gets the next unique device id for the specified type
        /// </summary>
        /// <typeparam name="T">The device type</typeparam>
        /// <returns>The device identifier</returns>
        int GetNextDeviceId<T>()
            where T : class;

        /// <summary>
        ///     Gets the next device id for the specified type
        /// </summary>
        /// <param name="type">The device type</param>
        /// <returns>The device identifier</returns>
        int GetNextDeviceId(Type type);

        /// <summary>
        ///     Gets the current unique device identifier for the specified type
        /// </summary>
        /// <typeparam name="T">The device type</typeparam>
        /// <returns>The device identifier</returns>
        int GetCurrentDeviceId<T>()
            where T : class;

        /// <summary>
        ///     Gets the current unique device identifier for the specified type
        /// </summary>
        /// <param name="type">The device type</param>
        /// <returns>The device identifier</returns>
        int GetCurrentDeviceId(Type type);
    }
}