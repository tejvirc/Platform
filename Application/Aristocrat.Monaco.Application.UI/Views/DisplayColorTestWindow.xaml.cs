namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.Windows;
    using Cabinet.Contracts;

    /// <summary>
    /// </summary>
    [CLSCompliant(false)]
    public partial class DisplayColorTestWindow
    {
        private WindowToScreenMapper _mapper;
        private DisplayRole _displayRole;

        public DisplayColorTestWindow()
        {
            InitializeComponent();
        }

        public bool DisplayControls
        {
            set => ControlsPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public DisplayRole DisplayRole
        {
            get => _displayRole;
            set
            {
                _displayRole = value;
                _mapper = new WindowToScreenMapper(value);
                Topmost = _mapper.IsFullscreen;
                _mapper.MapWindow(this);
            }
        }

        private void OnExitButtonPressed(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}