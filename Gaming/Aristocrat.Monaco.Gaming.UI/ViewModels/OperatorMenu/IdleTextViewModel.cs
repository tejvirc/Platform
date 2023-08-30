namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;

    /// <summary>
    ///     Defines the <see cref="IdleTextViewModel" /> class
    /// </summary>
    public class IdleTextViewModel : OperatorMenuPageViewModelBase
    {
        private string _idleText;
        private bool _isDirty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdleTextViewModel" /> class.
        /// </summary>
        public IdleTextViewModel()
        {
            _idleText = (string)PropertiesManager.GetProperty(GamingConstants.IdleText, string.Empty);

            ApplyCommand = new RelayCommand<object>(OnApply, _ => !string.IsNullOrWhiteSpace(IdleText) && _isDirty);

        }

        /// <summary>
        ///     Gets or sets action command that applies the idle text.
        /// </summary>
        public RelayCommand<object> ApplyCommand { get; set; }

        /// <summary>
        ///     Gets or sets the idle mode banner text to display
        /// </summary>
        public string IdleText
        {
            get => _idleText;

            set
            {
                if (_idleText == value)
                {
                    return;
                }

                _idleText = value;
                _isDirty = true;
                OnPropertyChanged(nameof(IdleText));
                ApplyCommand.NotifyCanExecuteChanged();
            }
        }

        private void OnApply(object parameter)
        {
            PropertiesManager.SetProperty(GamingConstants.IdleText, IdleText);

            _isDirty = false;
            ApplyCommand.NotifyCanExecuteChanged();
        }

    }
}
