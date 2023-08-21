namespace Aristocrat.Monaco.Gaming.UI.ViewModels.ButtonDeck
{
    using Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.ButtonDeck;
    using Kernel;
    using Monaco.UI.Common;
    using MVVM.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Contracts.Lobby;
    using Contracts.Models;

    public class ButtonDeckSimulatorViewModel : BaseEntityViewModel
    {
        private const int BetButtonWidth = 800;
        private const int BetButtonHeight = 256;
        private const int BashButtonWidth = 240;
        private const int BashButtonHeight = 320;

        private const int BetButtonDisplayId = 0;
        private const int BashButtonDisplayId = 1;

        private readonly IButtonDeckDisplay _buttonDeckDisplay;
        private readonly IButtonDeckFilter _buttonDeckFilter;
        private readonly IEventBus _eventBus;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly Dictionary<string, ButtonLogicalId> _nameToLogicalId;

        private bool _lockupMessageShowing;
        private uint _bashButtonFrameId;
        private uint _betButtonFrameId;

        public ButtonDeckSimulatorViewModel()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            _lobbyStateManager = ServiceManager.GetInstance().GetService<IContainerService>().Container.GetInstance<ILobbyStateManager>();

            BetButtonBitmap = new WriteableBitmap(BetButtonWidth, BetButtonHeight, 96, 96, PixelFormats.Bgr565, null);
            BashButtonBitmap = new WriteableBitmap(
                BashButtonWidth,
                BashButtonHeight,
                96,
                96,
                PixelFormats.Bgr565,
                null);

            _buttonDeckDisplay = ServiceManager.GetInstance().GetService<IButtonDeckDisplay>();
            _buttonDeckFilter = ServiceManager.GetInstance().GetService<IButtonDeckFilter>();

            var renderTimer = new DispatcherTimerAdapter(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(33.0) };
            renderTimer.Tick += RenderTimer_Tick;
            renderTimer.Start();

            _nameToLogicalId = new Dictionary<string, ButtonLogicalId>
            {
                { "Spin", ButtonLogicalId.Play },
                { "CashOut", ButtonLogicalId.Collect },
                { "Line1", ButtonLogicalId.Play1 },
                { "Line2", ButtonLogicalId.Play2 },
                { "Line3", ButtonLogicalId.Play3 },
                { "Line4", ButtonLogicalId.Play4 },
                { "Line5", ButtonLogicalId.Play5 },
                { "Service", ButtonLogicalId.Service },
                { "Bet1", ButtonLogicalId.Bet1 },
                { "Bet2", ButtonLogicalId.Bet2 },
                { "Bet3", ButtonLogicalId.Bet3 },
                { "Bet4", ButtonLogicalId.Bet4 },
                { "Bet5", ButtonLogicalId.Bet5 }
            };
        }

        public WriteableBitmap BetButtonBitmap { get; }

        public WriteableBitmap BashButtonBitmap { get; }

        public bool OnMouseDown(string name)
        {
            if (_nameToLogicalId.ContainsKey(name))
            {
                var logicalId = (int)_nameToLogicalId[name];
                if (IsButtonEnabled(logicalId))
                {
                    _eventBus.Publish(new DownEvent(logicalId));
                }

                return true;
            }
            return false;
        }

        public bool OnMouseUp(string name)
        {
            if (_nameToLogicalId.ContainsKey(name))
            {
                var logicalId = (int)_nameToLogicalId[name];
                if (IsButtonEnabled(logicalId))
                {
                    _eventBus.Publish(new UpEvent(logicalId));
                }

                return true;
            }
            return false;
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            if (_betButtonFrameId != _buttonDeckDisplay.GetRenderedFrameId(BetButtonDisplayId))
            {
                var sourceRect = new Int32Rect(0, 0, BetButtonWidth, BetButtonHeight);
                BetButtonBitmap.WritePixels(
                    sourceRect,
                    _buttonDeckDisplay.GetRenderedFrame(BetButtonDisplayId),
                    BetButtonBitmap.BackBufferStride,
                    0);
                RaisePropertyChanged(nameof(BetButtonBitmap));
                _betButtonFrameId = _buttonDeckDisplay.GetRenderedFrameId(BetButtonDisplayId);
            }

            if (_bashButtonFrameId != _buttonDeckDisplay.GetRenderedFrameId(BashButtonDisplayId))
            {
                var sourceRect = new Int32Rect(0, 0, BashButtonWidth, BashButtonHeight);
                BashButtonBitmap.WritePixels(
                    sourceRect,
                    _buttonDeckDisplay.GetRenderedFrame(BashButtonDisplayId),
                    BashButtonBitmap.BackBufferStride,
                    0);
                RaisePropertyChanged(nameof(BashButtonBitmap));
                _bashButtonFrameId = _buttonDeckDisplay.GetRenderedFrameId(BashButtonDisplayId);
            }
        }

        private bool IsButtonEnabled(int logicalId)
        {
            switch (_buttonDeckFilter.FilterMode)
            {
                case ButtonDeckFilterMode.Lockup:
                    ToggleCashOutFailureState();
                    return false;
                case ButtonDeckFilterMode.CashoutOnly:
                    return (ButtonLogicalId)logicalId == ButtonLogicalId.Collect;
                default:
                    return true;
            }
        }

        private void ToggleCashOutFailureState()
        {
            // Handle the special lockup state that requires user interaction.
            if (_lobbyStateManager.ContainsAnyState(LobbyState.CashOutFailure) && _lockupMessageShowing)
            {
                // We've shown the lockup message; the user is now clearing it.
                _lockupMessageShowing = false;
                _lobbyStateManager.RemoveFlagState(LobbyState.CashOutFailure);
            }
            else
            {
                // First time showing the lockup message.
                // Continue to show the message if we're not waiting on user interaction.
                _lockupMessageShowing = true;
            }
        }
    }
}
