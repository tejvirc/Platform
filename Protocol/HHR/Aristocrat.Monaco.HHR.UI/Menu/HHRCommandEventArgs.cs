namespace Aristocrat.Monaco.Hhr.UI.Menu
{
    using System;

    public class HHRCommandEventArgs : EventArgs
    {
        public Command Command { get;}

        public HHRCommandEventArgs(Command command)
        {
            Command = command;
        }
    }
}
