namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>
    ///     An interface that handles validating a currency received by a note acceptor.
    /// </summary>
    public interface ICurrencyValidator: IService
    {
        /// <summary>
        ///     Gets a value indicating whether a validating host is online or not
        /// </summary>
        bool HostOnline { get; }

        /// <summary>
        ///     Method to validate a note request
        /// </summary>
        /// <param name="note">The note value to validate.</param>
        /// <returns>Task CurrencyInExceptionCode result</returns>
        Task<CurrencyInExceptionCode> ValidateNote(int note);

        /// <summary>
        ///     Method to stacked note request
        /// </summary>
        /// <param name="note">The note value stacked.</param>
        /// <returns>Task true if stacked.</returns>
        Task<bool> StackedNote(int note);
    }
}