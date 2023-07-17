namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using System;

    /// <summary>Definition of the CoinAcceptor interface.</summary>

    [CLSCompliant(false)]
    public interface ICoinAcceptor : IPwmDevice, IDisposable
    {
        /// <summary>Ack last read input</summary>
        bool AckRead(uint txnId);

        /// <summary>Instruct device for reject on/off here.</summary>
        bool RejectMechanishOnOff(bool OnOff);

        /// <summary>Instruct device for divertor on/off here.</summary>
        bool DivertorMechanishOnOff(bool OnOff);

        /// <summary>Instruct device for stop polling.</summary>
        bool StopPolling();

        /// <summary>Instruct device for start polling.</summary>
        bool StartPolling();
    }
}
