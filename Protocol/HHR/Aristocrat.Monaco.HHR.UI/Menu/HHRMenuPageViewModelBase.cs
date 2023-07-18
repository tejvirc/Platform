namespace Aristocrat.Monaco.Hhr.UI.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using log4net;

    /// <summary>
    ///     All Hhr-HHR menu page ViewModels should inherit from this base class
    /// </summary>
    public abstract class HhrMenuPageViewModelBase : BaseEntityViewModel, IHhrMenuPageViewModel
    {
        protected new readonly ILog Logger;

        protected HhrMenuPageViewModelBase()
        {
            Logger = LogManager.GetLogger(GetType());
            Commands = new List<HhrPageCommand>();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual Task Init(Command command) => Task.CompletedTask;
        
        public virtual void Reset()
        {
            Logger.Debug($"Reset for viewModel{GetType()} is executed");
            Commands.Clear();
        }

        public List<HhrPageCommand> Commands { get; set; }

        // The event
        public event EventHandler<HHRCommandEventArgs> HhrButtonClicked;

        // The event to handle placards
        public event EventHandler<PlacardEventArgs> PlacardEvent;

        protected void OnHandlePlacard(PlacardEventArgs args)
        {
            PlacardEvent?.Invoke(this, args);
        }

        protected void OnHhrButtonClicked(Command command)
        {
            // Safely raise the event for all subscribers
            HhrButtonClicked?.Invoke(this, new HHRCommandEventArgs(command));
        }

        /// <inheritdoc />
        public bool ShowFooterText { get; set; } = true;

        /// <inheritdoc />
        public TimerInfo TimerInfo { get; set; }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
