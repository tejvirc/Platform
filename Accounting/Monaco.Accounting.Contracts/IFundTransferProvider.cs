using System;
using Aristocrat.Monaco.Accounting.Contracts.Wat;

namespace Aristocrat.Monaco.Accounting.Contracts
{
    /// <summary>
    ///     An Interface that provide access to fund transfer providers of the protocol
    ///     that handles the validation.
    /// </summary>
    [CLSCompliant(false)]
    public interface IFundTransferProvider
    {
        /// <summary>
        ///     The method that gives the WATTransferOff provider of the protocol that handles the validation
        /// </summary>
        /// <param name="waitForService">Specifies whether the caller needs to wait for the service to come up</param>
        IWatTransferOffProvider GetWatTransferOffProvider(bool waitForService = false);

        /// <summary>
        ///     The method that gives the WATTransferOn provider of the protocol that handles the validation
        /// </summary>
        /// <param name="waitForService">Specifies whether the caller needs to wait for the service to come up</param>
        IWatTransferOnProvider GetWatTransferOnProvider(bool waitForService = false);

        /// <summary>
        ///     A function that can be used by various protocols to register their IWatTransferOffProvider implementation.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to register its voucher WatTransferOffProvider</param>
        /// <param name="handler">IWatTransferOffProvider implementation</param>
        bool Register(string protocolName, IWatTransferOffProvider handler);

        /// <summary>
        ///     A function that can be used by various protocols to register their IWatTransferOnProvider implementation.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to register its WatTransferOnProvider</param>
        /// <param name="handler">IWatTransferOnProvider implementation</param>
        bool Register(string protocolName, IWatTransferOnProvider handler);

        /// <summary>
        ///     A function that can be used by various protocols to UnRegister their IWatTransferOffProvider implementation.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to unregister its WatTransferOffProvider</param>
        /// <param name="handler">IWatTransferOffProvider implementation</param>
        bool UnRegister(string protocolName, IWatTransferOffProvider handler);

        /// <summary>
        ///     A function that can be used by various protocols to UnRegister their IWatTransferOnProvider implementation.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to unregister its WatTransferOnProvider</param>
        /// <param name="handler">IWatTransferOnProvider implementation</param>
        bool UnRegister(string protocolName, IWatTransferOnProvider handler);
    }
}