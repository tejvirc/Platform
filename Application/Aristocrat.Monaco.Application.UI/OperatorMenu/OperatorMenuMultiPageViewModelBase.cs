namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Contracts;
    using Contracts.Localization;
    using Contracts.MeterPage;
    using Contracts.OperatorMenu;
    using Events;
    using Kernel;
    using MVVM;

    [CLSCompliant(false)]
    public abstract class OperatorMenuMultiPageViewModelBase : OperatorMenuPageViewModelBase
    {
        private readonly IOperatorMenuPageLoader _mainPage;

        protected readonly string MenuExtensionPath;
        private bool _buttonsEnabled = true;
        private IOperatorMenuPageLoader _selectedPage;
        private string _warningMessageText;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="mainPage">Page loader for the main page for this view model</param>
        /// <param name="extensionPath">The path of the extension point to load the sub-pages</param>;
        protected OperatorMenuMultiPageViewModelBase(IOperatorMenuPageLoader mainPage, string extensionPath)
        {
            _mainPage = mainPage;
            DisplayPageTitle = mainPage?.PageName ?? string.Empty;
            MenuExtensionPath = extensionPath;
            Pages = new ObservableCollection<IOperatorMenuPageLoader>();
            LoadPages();
        }

        public override bool CanCalibrateTouchScreens => SelectedPage?.ViewModel?.CanCalibrateTouchScreens ?? false;

        /// <summary>
        ///     Show or hide tab selection buttons.True if multiple tabs are present in the page.
        /// </summary>
        public bool ShowSelectionButtons => (Pages?.Count(o => o.IsVisible) ?? 0) > 1;

        public string DisplayPageTitle { get; set; }

        /// <summary>
        ///     This is the minimum width of the tab control buttons
        /// </summary>
        public virtual int MinButtonWidth => 200;

        public ObservableCollection<IOperatorMenuPageLoader> Pages { get; }

        protected override bool IsContainerPage => true;

        protected bool ParentIsMultiPage { get; set; }

        public IOperatorMenuPageLoader SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (_selectedPage == value || value == null)
                {
                    if (_selectedPage != null)
                    {
                        // reset the page title if the selected item is being re-selected
                        SetPageTitle();
                    }

                    return;
                }

                if (_selectedPage != null)
                {
                    _selectedPage.OnEnabledChanged -= SelectedPageEnableChanged;
                }

                var oldVm = _selectedPage?.ViewModel;
                if (oldVm != null)
                {
                    oldVm.PropertyChanged -= OnSelectedPagePropertyChanged;
                }

                _selectedPage = value;

                var vm = _selectedPage?.ViewModel;
                if (vm != null)
                {
                    vm.PropertyChanged += OnSelectedPagePropertyChanged;
                    // VLT-12225
                    // Subscribe to an event when a specific meter page is loaded (switching tabs) so we can
                    // synchronize the ShowLifetime and Master/period button status
                    if (vm is MetersPageViewModelBase)
                    {
                        EventBus.Subscribe<MeterPageLoadedEvent>(this, _ => PageLoaded());
                    }
                }

                if (_selectedPage != null)
                {
                    _selectedPage.OnEnabledChanged += SelectedPageEnableChanged;
                }

                RaisePropertyChanged(nameof(SelectedPage));
                RaisePropertyChanged(nameof(DataEmpty));
                RaisePropertyChanged(nameof(IsLoadingData));
                RaisePropertyChanged(nameof(MainPrintButtonEnabled));
                RaisePropertyChanged(nameof(CanCalibrateTouchScreens));

                if (_selectedPage == null)
                {
                    return;
                }

                SetPageTitle();

                SelectedPageChanged();
            }
        }

        protected virtual void PageLoaded()
        {
            // do not put logic here - if you need to process/set a prop in another object not in this inheritance chain
            // after a page is loaded override in the page
            // VLT-12225 handles keeping the Master/Period button status in sync with each specific meter page ShowLifetime state
        }

        private void OnSelectedPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataEmpty) || e.PropertyName == nameof(IsLoadingData) || e.PropertyName == nameof(MainPrintButtonEnabled))
            {
                RaisePropertyChanged(e.PropertyName);
            }
        }

        public bool ButtonsEnabled
        {
            get => _buttonsEnabled;
            set
            {
                if (_buttonsEnabled == value)
                {
                    return;
                }
                _buttonsEnabled = value;
                RaisePropertyChanged(nameof(ButtonsEnabled));
            }
        }

        public override bool DataEmpty => SelectedPage?.ViewModel?.DataEmpty ?? base.DataEmpty;

        public override bool IsLoadingData => SelectedPage?.ViewModel?.IsLoadingData ?? base.IsLoadingData;

        public override bool MainPrintButtonEnabled => SelectedPage?.ViewModel?.MainPrintButtonEnabled ?? base.MainPrintButtonEnabled;

        public override bool PopupOpen
        {
            get => SelectedPage?.ViewModel?.PopupOpen ?? false;
            set
            {
                if (SelectedPage?.ViewModel != null)
                {
                    SelectedPage.ViewModel.PopupOpen = value;
                }
            }
        }

        public string WarningMessageText
        {
            get => _warningMessageText;
            set => SetProperty(ref _warningMessageText, value, nameof(WarningMessageText));
        }

        protected override void OnLoaded()
        {
            SetButtonsEnabled(true);
            if (Pages.Any())
            {
                SelectedPage = Pages.First();
            }

            EventBus.Subscribe<OperatorMenuPrintJobStartedEvent>(this, _ => SetButtonsEnabled(false));
            EventBus.Subscribe<OperatorMenuPrintJobCompletedEvent>(this, _ => SetButtonsEnabled(true));
            EventBus.Subscribe<EnableOperatorMenuEvent>(this, evt => SetButtonsEnabled(evt.Enable));
            EventBus.Subscribe<OperatorMenuWarningMessageEvent>(this, OnUpdateWarningMessage);
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, OnOperatorCultureChanged);
        }

        protected void OnOperatorCultureChanged(OperatorCultureChangedEvent @event)
        {
            SetPageTitle();
        }

        protected override void OnUnloaded()
        {
            EventBus.UnsubscribeAll(this);
        }

        protected virtual void SelectedPageChanged()
        {
        }

        private void OnUpdateWarningMessage(OperatorMenuWarningMessageEvent e)
        {
            MvvmHelper.ExecuteOnUI(() => WarningMessageText = e.Message);
        }

        /// <summary>Sets the state of the buttons as enabled or disabled.</summary>
        /// <param name="enabled">Indicates whether or not other buttons are enabled.</param>
        public void SetButtonsEnabled(bool enabled)
        {
            MvvmHelper.ExecuteOnUI(() => ButtonsEnabled = enabled);
        }

        private void LoadPages()
        {
            Logger.Info($"Loading {GetType()} pages...");

            if (string.IsNullOrEmpty(MenuExtensionPath))
            {
                return;
            }

            var nodes = MonoAddinsHelper.GetSelectedNodes<OperatorMenuTypeExtensionNode>(MenuExtensionPath);

            foreach (var node in nodes.OrderBy(n => n.Order))
            {
                var page = (IOperatorMenuPageLoader)node.CreateInstance();
                page.IsSubPage = true;

                page.Initialize();

                if (!page.IsVisible)
                {
                    Logger.DebugFormat("Skipping page button: {0}", page.PageName);
                    continue;
                }
                if (page.IsMultiPage)
                {
                    ((OperatorMenuMultiPageViewModelBase)page.ViewModel).ParentIsMultiPage = true;
                }
                Logger.Debug($"Loaded {GetType()} {page.PageName} page {node.Id} {node.Type} {node.Order}");
                Pages.Add(page);
            }

            Logger.Info($"Loading {GetType()} pages...completed!");
        }

        protected override void DisposeInternal()
        {
            foreach (var page in Pages)
            {
                page.Dispose();
            }

            Pages.Clear();

            base.DisposeInternal();
        }

        protected virtual void SetPageTitle()
        {
            if (!ParentIsMultiPage)
            {
                DisplayPageTitle = _mainPage.PageName;

                if (!string.IsNullOrEmpty(DisplayPageTitle))
                {
                    var title = !string.IsNullOrEmpty(SelectedPage.PageName)
                        ? $"{DisplayPageTitle} - {SelectedPage.PageName}"
                        : DisplayPageTitle;
                    EventBus.Publish(new PageTitleEvent(title));
                }
            }
        }

        private void SelectedPageEnableChanged(object sender, EventArgs e)
        {
            if (!SelectedPage.IsEnabled)
            {
                var newPage = Pages.FirstOrDefault(o => o.IsEnabled);
                if (newPage != null)
                {
                    SelectedPage = newPage;
                }
            }
        }
    }
}
