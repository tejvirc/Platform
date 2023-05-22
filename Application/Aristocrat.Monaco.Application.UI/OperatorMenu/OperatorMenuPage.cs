namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using Contracts.OperatorMenu;
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Interactivity;

    /// <summary>
    ///     Use this base UserControl class when creating an Operator Menu WPF page
    /// </summary>
    public abstract class OperatorMenuPage : UserControl, IOperatorMenuPage
    {
        private bool _disposed;

        protected OperatorMenuPage()
        {
            DataContextChanged += OnDataContextChanged;
        }

        //zhg!!! Save Properties Changes
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is OperatorMenuPageViewModelBase viewModel)
            {
                // Bind Loaded and Unloaded event commands as soon as we have a view model
                SetTrigger(nameof(viewModel.LoadedCommand), LoadedEvent.Name);
                SetTrigger(nameof(viewModel.UnloadedCommand), UnloadedEvent.Name);
            }
        }

        //zhg!!!
        private void SetTrigger(string commandName, string eventName)
        {
            var invokeCommandAction = new InvokeCommandAction { CommandParameter = this };
            var binding = new Binding { Path = new PropertyPath(commandName) };
            BindingOperations.SetBinding(invokeCommandAction, InvokeCommandAction.CommandProperty, binding);

            var eventTrigger = new System.Windows.Interactivity.EventTrigger { EventName = eventName };
            eventTrigger.Actions.Add(invokeCommandAction);

            var triggers = Interaction.GetTriggers(this);
            triggers.Add(eventTrigger);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (DataContext is IOperatorMenuPageViewModel vm)
                {
                    vm.Dispose();
                }
                DataContextChanged -= OnDataContextChanged;

                Resources.MergedDictionaries.Clear();
                Resources.Clear();
            }

            _disposed = true;
        }
    }
}
