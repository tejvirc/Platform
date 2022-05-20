namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Runtime.Client;

    /// <summary>
    ///     Command handler for the <see cref="ButtonStateChanged" /> command.
    /// </summary>
    public class ButtonStateChangedCommandHandler : ICommandHandler<ButtonStateChanged>
    {
        private readonly IButtonLamps _buttonLamps;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ButtonStateChangedCommandHandler" /> class.
        /// </summary>
        /// <param name="buttonLamps">An <see cref="IButtonLamps" /> instance.</param>
        public ButtonStateChangedCommandHandler(IButtonLamps buttonLamps)
        {
            _buttonLamps = buttonLamps ?? throw new ArgumentNullException(nameof(buttonLamps));
        }

        /// <inheritdoc />
        public void Handle(ButtonStateChanged command)
        {
            foreach (var buttonChanged in command.States)
            {
                var value = buttonChanged.Value;

                var lampOn = (value & ButtonState.LightOn) != 0;
                var blinkSlow = (value & ButtonState.BlinkSlow) != 0;

                var lampState = LampState.Off;

                if (blinkSlow)
                {
                    lampState = LampState.Blink;
                }
                else if (lampOn)
                {
                    lampState = LampState.On;
                }

                _buttonLamps.SetLampState((int)buttonChanged.Key, lampState);
            }
        }
    }
}