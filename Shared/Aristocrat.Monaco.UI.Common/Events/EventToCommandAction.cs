namespace Aristocrat.Monaco.UI.Common.Events
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    /// <summary>
    /// This custom implementation of the <see cref="InvokeCommandAction"/> automatically executes the command with the
    /// event args as the parameter rather than requiring to set the CommandParameter manually
    /// </summary>
    public sealed class EventToCommandAction : TriggerAction<DependencyObject>
    {
        /// <summary>
        /// Command dependency property for binding
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(EventToCommandAction), null);

        /// <summary>
        /// Command to route event args to
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Invoke command
        /// </summary>
        /// <param name="parameter"></param>
        protected override void Invoke(object parameter)
        {
            if (AssociatedObject != null)
            {
                var command = Command;
                if (command != null)
                {
                    if (command.CanExecute(parameter))
                    {
                        command.Execute(parameter);
                    }
                }
            }
        }
    }
}
