using Aristocrat.Monaco.Accounting.Contracts.Handpay;

namespace Aristocrat.Monaco.Accounting.Contracts
{
    /// <summary>
    ///     An Interface that provide access to voucher validators of the protocol
    ///     that handles the validation.
    /// </summary>
    public interface IValidationProvider
    {
        /// <summary>
        ///     The getter that gives the Voucher validator of the protocol that handles the validation
        /// </summary>
        /// <param name="waitForService">Specifies whether the caller needs to wait for the service to come up</param>
        IVoucherValidator GetVoucherValidator(bool waitForService = false);

        /// <summary>
        ///     The getter that gives the Currency validator of the protocol that handles the validation
        /// </summary>
        /// <param name="waitForService">Specifies whether the caller needs to wait for the service to come up</param>
        ICurrencyValidator GetCurrencyValidator(bool waitForService = false);

        /// <summary>
        ///     The getter that gives the Handpay validator of the protocol that handles the validation
        /// </summary>
        /// <param name="waitForService">Specifies whether the caller needs to wait for the service to come up</param>
        IHandpayValidator GetHandPayValidator(bool waitForService = false);

        /// <summary>
        ///     A function that can be used by various protocols to register their voucher validator.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to register its voucher validator</param>
        /// <param name="handler">IVoucherValidator validator implementation</param>
        bool Register(string protocolName, IVoucherValidator handler);

        /// <summary>
        ///     A function that can be used by various protocols to register their currency validator.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to register its voucher validator</param>
        /// <param name="handler">ICurrencyValidator validator implementation</param>
        bool Register(string protocolName, ICurrencyValidator handler);

        /// <summary>
        ///     A function that can be used by various protocols to register their handpay validator.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to register its voucher validator</param>
        /// <param name="handler">IHandpayValidator validator implementation</param>
        bool Register(string protocolName, IHandpayValidator handler);

        /// <summary>
        ///     A function that can be used by various protocols to unregister their voucher validator.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to unregister its voucher validator</param>
        /// <param name="handler">IVoucherValidator validator implementation</param>
        bool UnRegister(string protocolName, IVoucherValidator handler);

        /// <summary>
        ///     A function that can be used by various protocols to unregister their currency validator.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to unregister its voucher validator</param>
        /// <param name="handler">ICurrencyValidator validator implementation</param>
        bool UnRegister(string protocolName, ICurrencyValidator handler);

        /// <summary>
        ///     A function that can be used by various protocols to unregister their handpay validator.
        /// </summary>
        /// <param name="protocolName">name of the protocol that is trying to unregister its voucher validator</param>
        /// <param name="handler">IHandpayValidator validator implementation</param>
        bool UnRegister(string protocolName, IHandpayValidator handler);
    }
}