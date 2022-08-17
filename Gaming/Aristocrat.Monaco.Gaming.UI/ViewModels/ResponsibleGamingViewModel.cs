namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using MVVM.ViewModel;
    using System.Windows.Input;
    using Models;
    using Microsoft.Toolkit.Mvvm.Input;
    //using MVVM.Command;

    /// <summary>
    ///     Helper class to handle responsible gaming parts of the lobby ViewModel.
    /// </summary>
    public class ResponsibleGamingViewModel : BaseEntityViewModel
    {
        private readonly LobbyViewModel _lobby;
        private readonly int _infoPageCount = 5;
        private int _infoPageIndex;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponsibleGamingViewModel" /> class
        /// </summary>
        /// <param name="lobby">Reference to the lobby VM.</param>
        public ResponsibleGamingViewModel(LobbyViewModel lobby)
        {
            _lobby = lobby ?? throw new ArgumentNullException(nameof(lobby));

            LeftInfoPageNavigationCommand = new RelayCommand<object>(LeftPageNavigationButtonPressed);
            RightInfoPageNavigationCommand = new RelayCommand<object>(RightPageNavigationButtonPressed);

            ConfigurePages();
        }

        /// <summary>
        ///     Gets the responsible gaming left page command
        /// </summary>
        public ICommand LeftInfoPageNavigationCommand { get; }

        /// <summary>
        ///     Gets the responsible gaming left page command
        /// </summary>
        public ICommand RightInfoPageNavigationCommand { get; }

        /// <summary>
        ///     Gets or sets the RGInfoPage index
        /// </summary>
        public int InfoPageIndex
        {
            get => _infoPageIndex;

            set
            {
                if (_infoPageIndex != value)
                {
                    _infoPageIndex = value;
                    RaisePropertyChanged(nameof(InfoPageIndex));
                    RaisePropertyChanged(nameof(IsBackButtonEnabled));
                    RaisePropertyChanged(nameof(IsBackButtonVisible));
                    RaisePropertyChanged(nameof(IsForwardButtonVisible));
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the back button is enabled.
        /// </summary>
        public bool IsBackButtonEnabled => InfoPageIndex > 0;

        /// <summary>
        ///     Gets a value indicating whether the back button is visible.
        /// </summary>
        public bool IsBackButtonVisible => Pages.Count > 1 && InfoPageIndex > 0;

        /// <summary>
        ///     Gets a value indicating whether the forward button is visible.
        /// </summary>
        public bool IsForwardButtonVisible => Pages.Count > 1 && InfoPageIndex < _infoPageCount - 1;

        public bool IsExitButtonVisible =>
            (_lobby.Config.ResponsibleGamingInfo.ExitStrategy &
             ResponsibleGamingInfoExitStrategy.PressButton) ==
            ResponsibleGamingInfoExitStrategy.PressButton;

        public bool IsPrintHelplineButtonVisible =>
            _lobby.Config.ResponsibleGamingInfo.PrintHelpline;

        /// <summary>
        ///     Gets the number of pages.
        /// </summary>
        public ObservableCollection<ResponsibleGamingInfoPage> Pages { get; } = new ObservableCollection<ResponsibleGamingInfoPage>();

        private void ConfigurePages()
        {
            var pages = _lobby.Config.ResponsibleGamingInfo.Pages;

            for (var i = 0; i < pages; i++)
            {
                Pages.Add(
                    new ResponsibleGamingInfoPage
                    {
                        Index = i,
                        BackgroundKey = $"RGInfoPage{i + 1}"
                    });
            }
        }

        private void LeftPageNavigationButtonPressed(object obj)
        {
            --InfoPageIndex;
            if (InfoPageIndex < 0)
            {
                InfoPageIndex = 0;
            }
            _lobby.OnResponsibleGamingInfoNavigationPressed();
        }

        private void RightPageNavigationButtonPressed(object obj)
        {
            ++InfoPageIndex;
            if (InfoPageIndex >= _infoPageCount)
            {
                InfoPageIndex = 0;
                _lobby.ExitResponsibleGamingInfoDialog();
            }
            else
            {
                _lobby.OnResponsibleGamingInfoNavigationPressed();
            }
        }
    }
}
