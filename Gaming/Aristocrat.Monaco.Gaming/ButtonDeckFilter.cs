namespace Aristocrat.Monaco.Gaming
{
    using Contracts;
    using Hardware.Contracts.Button;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     Implements high-level platform filtering of button deck key presses.
    /// </summary>
    public class ButtonDeckFilter : IButtonDeckFilter
    {
        private readonly IButtonService _buttonService;
        private readonly Collection<int> _cashOutOnlyButtons;
        private readonly Collection<int> _lockupButtons;

        private ButtonDeckFilterMode _filter = ButtonDeckFilterMode.Normal;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ButtonDeckFilter" /> class.
        /// </summary>
        /// <param name="buttonService">The button service</param>
        public ButtonDeckFilter(IButtonService buttonService)
        {
            _buttonService = buttonService ?? throw new ArgumentNullException(nameof(buttonService));

            _lockupButtons = new Collection<int>();
            _cashOutOnlyButtons = new Collection<int>();
            var buttonBase = (int)ButtonLogicalId.ButtonBase;
            for (var i = buttonBase + 1; i <= buttonBase + 23; ++i)
            {
                _lockupButtons.Add(i);

                // Cashout active in this state.
                if ((ButtonLogicalId)i != ButtonLogicalId.Collect)
                {
                    _cashOutOnlyButtons.Add(i);
                }
            }
        }

        /// <inheritdoc />
        public ButtonDeckFilterMode FilterMode
        {
            get => _filter;

            set
            {
                if (_filter != value)
                {
                    _filter = value;
                    OnFilterChanged();
                }
            }
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IButtonDeckFilter) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private void OnFilterChanged()
        {
            switch (_filter)
            {
                case ButtonDeckFilterMode.Normal:
                    _buttonService.Enable(_lockupButtons);
                    break;
                case ButtonDeckFilterMode.Lockup:
                    _buttonService.Disable(_lockupButtons);
                    break;
                case ButtonDeckFilterMode.CashoutOnly:
                    _buttonService.Disable(_cashOutOnlyButtons);
                    break;
            }
        }
    }
}
