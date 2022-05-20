namespace Aristocrat.Sas.Client
{
    using System;

    public class NewSasPollDataEventArgs : EventArgs
    {
        public NewSasPollDataEventArgs(SasPollData data)
        {
            SasPollData = data;
        }

        public SasPollData SasPollData { get; }
    }
}