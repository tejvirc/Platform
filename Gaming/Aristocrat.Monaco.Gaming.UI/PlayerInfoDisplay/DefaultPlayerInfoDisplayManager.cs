namespace Aristocrat.Monaco.Gaming.UI.PlayerInfoDisplay
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Contracts;
    using Contracts.Events;
    using Contracts.PlayerInfoDisplay;
    using Kernel;
    using log4net;
    using MVVM;
    using Runtime;
    using Runtime.Client;

    /// <inheritdoc cref="IPlayerInfoDisplayManager" />
    public sealed class DefaultPlayerInfoDisplayManager : IPlayerInfoDisplayManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IRuntime _runtime;
        private readonly IGameResourcesModelProvider _gameResourcesModelProvider;
        private readonly object _sync = new object();

        private readonly ConcurrentDictionary<PageType, IPlayerInfoDisplayViewModel> _pages = new ConcurrentDictionary<PageType, IPlayerInfoDisplayViewModel>();

        private bool _isActive;
        private bool _disposed;

        public DefaultPlayerInfoDisplayManager(
            IEventBus eventBus
            , IRuntime runtime
            , IGameResourcesModelProvider gameResourcesModelProvider
        )
        {
            Logger.Debug("Create DefaultPlayerInfoDisplayManager");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _gameResourcesModelProvider = gameResourcesModelProvider ?? throw new ArgumentNullException(nameof(gameResourcesModelProvider));

            _isActive = false;

            SubscribeToEvents();
        }

        /// <inheritdoc />
        public void AddPages(IList<IPlayerInfoDisplayViewModel> pages)
        {
            if (pages == null || !pages.Any())
            {
                return;
            }
            Logger.Debug("Adding pages");
            RemovePages();
            foreach (var playerInfoDisplayViewModel in pages)
            {
                if (_pages.TryAdd(playerInfoDisplayViewModel.PageType, playerInfoDisplayViewModel))
                {
                    Logger.Debug($"Page {playerInfoDisplayViewModel.PageType} added");
                }
                else
                {
                    Logger.Error($"Cannot add page {playerInfoDisplayViewModel.PageType}");
                }
            }
        }

        private void RemovePages()
        {
            Logger.Debug("Removing pages");
            foreach (var playerInfoDisplayViewModel in _pages.ToArray())
            {
                HidePage(playerInfoDisplayViewModel.Value);
                _pages.TryRemove(playerInfoDisplayViewModel.Key, out _);
            }
        }

        private void HidePages()
        {
            foreach (var playerInfoDisplayViewModel in _pages)
            {
                HidePage(playerInfoDisplayViewModel.Value);
            }
        }

        private void ShowPage(IPlayerInfoDisplayViewModel page)
        {
            Logger.Debug($"PID screen {page.PageType} is showing");
            page.ButtonClicked += PlayerInfoDisplayViewModelOnButtonClicked;
            MvvmHelper.ExecuteOnUI(page.Show);
        }

        private void HidePage(IPlayerInfoDisplayViewModel page)
        {
            Logger.Debug($"PID screen {page.PageType} is hiding");
            page.ButtonClicked -= PlayerInfoDisplayViewModelOnButtonClicked;
            MvvmHelper.ExecuteOnUI(page.Hide);
        }

        private void PlayerInfoDisplayViewModelOnButtonClicked(object sender, CommandArgs e)
        {
            Logger.Debug($"PID screen button {e.CommandType} pressed");
            switch (e.CommandType)
            {
                case CommandType.Exit:
                    OnExit();
                    break;

                default:
                    Logger.Warn($"Unknown command {e.CommandType} requested");
                    return;
            }
        }

        /// <inheritdoc />
        public bool IsActive()
        {
            return _isActive;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            Logger.Debug("Disposing of Player Info Display");
            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                RemovePages();
            }

            _disposed = true;
        }

        private void OnEnter()
        {
            Logger.Debug("Entering PID");
            var firstPage = GetPage(PageType.Menu);
            if (firstPage == null)
            {
                Logger.Error("No initial screen for Player Information Display");
                OnExit();
                return;
            }
            ShowPage(firstPage);
            InPlayerInfoDisplay();
            Active();
            _eventBus.Publish(new PlayerInfoDisplayEnteredEvent());
        }

        private IPlayerInfoDisplayViewModel GetPage(PageType pageType)
        {
            return !_pages.ContainsKey(pageType) ? null : _pages[pageType];
        }

        private void Active()
        {
            lock (_sync)
            {
                _isActive = true;
            }
            Logger.Debug("PID is active");
        }

        private void OnExit()
        {
            lock (_sync)
            {
                if (!_isActive)
                {
                    return;
                }
            }
            Logger.Debug("Exiting PID");
            ExitPlayerInfoDisplay();
            HidePages();
            Inactive();
            _eventBus.Publish(new PlayerInfoDisplayExitedEvent());
        }

        private void Inactive()
        {
            lock (_sync)
            {
                _isActive = false;
            }
            Logger.Debug("PID is inactive");
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<PlayerInfoButtonPressedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PlayerInfoDisplayExitRequestEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameSelectedEvent>(this, HandleEvent);

            // cash out
            _eventBus.Subscribe<HandpayStartedEvent>(this, _ => OnExit());
            _eventBus.Subscribe<VoucherOutStartedEvent>(this, _ => OnExit());

            //cash in
            _eventBus.Subscribe<CurrencyInStartedEvent>(this, _ => OnExit());
            _eventBus.Subscribe<VoucherRedemptionRequestedEvent>(this, _ => OnExit());
        }

        private void HandleEvent(GameSelectedEvent @event)
        {
            var model = _gameResourcesModelProvider.Find(@event.GameId);
            MvvmHelper.ExecuteOnUI(() =>
                {
                    foreach (var p in _pages.Values)
                    {
                        p.SetupResources(model);
                    }
                }
            );
        }


        private void HandleEvent(PlayerInfoButtonPressedEvent @event)
        {
            OnEnter();
        }

        private void HandleEvent(PlayerInfoDisplayExitRequestEvent @event)
        {
            OnExit();
        }

        private void InPlayerInfoDisplay()
        {
            Logger.Debug("Notify runtime PID Main Screen is shown");
            lock (_sync)
            {
                if (_runtime.Connected)
                {
                    _runtime.UpdateFlag(RuntimeCondition.InPlayerInfoDisplayMenu, true);
                }
            }
        }

        private void ExitPlayerInfoDisplay()
        {
            Logger.Debug("Notify runtime PID screens exited");
            lock (_sync)
            {
                if (_runtime.Connected)
                {
                    _runtime.UpdateFlag(RuntimeCondition.InPlayerInfoDisplayMenu, false);
                }
            }
        }
    }
}