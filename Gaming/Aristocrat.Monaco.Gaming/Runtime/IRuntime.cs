namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Client;
    using Contracts.Central;

    /// <summary>
    ///     Provides a mechanism to communicate with a runtime client
    /// </summary>
    public interface IRuntime : IClientEndpoint
    {
        bool GetFlag(RuntimeCondition flag);

        void UpdateFlag(RuntimeCondition flag, bool state);

        RuntimeState GetState();

        void UpdateState(RuntimeState state);

        void InvokeButton(uint id, int state);

        void UpdateVolume(float level);

        void UpdateBalance(long credits);

        void UpdateButtonState(uint buttonId, ButtonMask mask, ButtonState state);

        void UpdateLocalTimeTranslationBias(TimeSpan duration);

        void UpdateParameters(IDictionary<string, string> parameters, ConfigurationTarget target);

        void UpdatePlatformMessages(IEnumerable<string> messages);

        void UpdateTimeRemaining(string message);

        void JackpotNotification();

        void JackpotWinNotification(string poolName, IDictionary<int, long> winLevels);

        void BeginGameRoundResponse(BeginGameRoundResult result, IEnumerable<Outcome> outcomes, CancellationTokenSource cancellationTokenSource = null);

        void SendTouch(DisplayId displayId, uint pointerId, TouchState touchState, uint pointerX, uint pointerY);

        void SendMouse(DisplayId displayId, MouseButton mouseButton, MouseState mouseState, uint mouseX, uint mouseY);

        void Shutdown();
    }
}