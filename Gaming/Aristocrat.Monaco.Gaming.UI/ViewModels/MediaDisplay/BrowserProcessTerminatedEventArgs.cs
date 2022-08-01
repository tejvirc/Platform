namespace Aristocrat.Monaco.Gaming.UI.ViewModels.MediaDisplay
{
    using System;

    public class BrowserProcessTerminatedEventArgs : EventArgs
    {
        public BrowserProcessTerminatedEventArgs(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}