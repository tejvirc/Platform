namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
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

            ApplyCommand = new ActionCommand<object>(OnApply, _ => !string.IsNullOrWhiteSpace(IdleText) && _isDirty);

        }

        /// <summary>
        ///     Gets or sets action command that applies the idle text.
        /// </summary>
        public ActionCommand<object> ApplyCommand { get; set; }

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
                RaisePropertyChanged(nameof(IdleText));
                ApplyCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnApply(object parameter)
        {
            PropertiesManager.SetProperty(GamingConstants.IdleText, IdleText);

            _isDirty = false;
            ApplyCommand.RaiseCanExecuteChanged();
        }

    }
}
