namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    ///     The note acceptor provider
    /// </summary>
    public interface ISasNoteAcceptorProvider
    {
        /// <summary>
        ///     enables bill acceptor (long poll 0x06)
        /// </summary>
        Task EnableBillAcceptor();

        /// <summary>
        ///     disables bill acceptor. (long poll 0x07)
        /// </summary>
        Task DisableBillAcceptor();

        /// <summary>
        ///     Configures the different bill denominations as enabled or disabled. (long poll 0x08)
        /// </summary>
        /// <param name="denominations">The denominations to enable. All others should be disabled.</param>
        /// <returns>True if the denomination configuration is successful.</returns>
        bool ConfigureBillDenominations(IEnumerable<ulong> denominations);

        /// <summary>
        ///     Gets or sets whether or not the bill acceptor stops accepting bills
        ///     after accepting one bill (long poll 0x08)
        /// </summary>
        bool BillDisableAfterAccept { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the note acceptor hardware diagnostic test is active.
        /// </summary>
        bool DiagnosticTestActive { get; set; }
    }
}