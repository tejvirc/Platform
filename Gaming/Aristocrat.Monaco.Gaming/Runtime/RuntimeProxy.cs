namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Client;
    using Contracts.Central;

    public class RuntimeProxy : IRuntime
    {
        private readonly IClientEndpointProvider<IRuntime> _serviceProvider;

        public RuntimeProxy(IClientEndpointProvider<IRuntime> serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public bool Connected => _serviceProvider.Client?.Connected ?? false;

        public bool GetFlag(RuntimeCondition flag)
        {
            return _serviceProvider.Client?.GetFlag(flag) ?? false;
        }

        public void UpdateFlag(RuntimeCondition flag, bool state)
        {
            _serviceProvider.Client?.UpdateFlag(flag, state);
        }

        public RuntimeState GetState()
        {
            return _serviceProvider.Client?.GetState() ?? RuntimeState.Error;
        }

        public void UpdateState(RuntimeState state)
        {
            _serviceProvider.Client?.UpdateState(state);
        }

        public void InvokeButton(uint id, int state)
        {
            _serviceProvider.Client?.InvokeButton(id, state);
        }

        public void UpdateBalance(long credits)
        {
            _serviceProvider.Client?.UpdateBalance(credits);
        }

        public void JackpotNotification()
        {
            _serviceProvider.Client?.JackpotNotification();
        }

        public void JackpotWinNotification(string poolName, IDictionary<int, long> winLevels)
        {
            _serviceProvider.Client?.JackpotWinNotification(poolName, winLevels);
        }

        public void BeginGameRoundResponse(BeginGameRoundResult result, IEnumerable<Outcome> outcomes, CancellationTokenSource cancellationTokenSource = null)
        {
            _serviceProvider.Client?.BeginGameRoundResponse(result, outcomes, cancellationTokenSource);
        }

        public void UpdateVolume(float level)
        {
            _serviceProvider.Client?.UpdateVolume(level);
        }

        public void UpdateButtonState(uint buttonId, ButtonMask mask, ButtonState state)
        {
            _serviceProvider.Client?.UpdateButtonState(buttonId, mask, state);
        }

        public void UpdateLocalTimeTranslationBias(TimeSpan duration)
        {
            _serviceProvider.Client?.UpdateLocalTimeTranslationBias(duration);
        }

        public void UpdateParameters(IDictionary<string, string> parameters, ConfigurationTarget target)
        {
            _serviceProvider.Client?.UpdateParameters(parameters, target);
        }

        public void UpdatePlatformMessages(IEnumerable<string> messages)
        {
            _serviceProvider.Client?.UpdatePlatformMessages(messages);
        }

        public void UpdateTimeRemaining(string message)
        {
            _serviceProvider.Client?.UpdateTimeRemaining(message);
        }

        public void Shutdown()
        {
            _serviceProvider.Client?.Shutdown();
        }
    }
}