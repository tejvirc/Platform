namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;

    /// <summary>
    ///     Provides a mechanism to interact with a currency in configuration validator
    /// </summary>
    public interface ICurrencyInValidator : IDisposable
    {
        /// <summary>
        ///     Determines if the configuration is valid based on jurisdictional configuration
        /// </summary>
        /// <returns>true, if valid</returns>
        /// <param name="credit">Optional credit amount to be used in the validation</param>
        /// <returns>true, if currency can be added based on the current state and configuration</returns>
        bool IsValid(long credit = 0L);
    }
}