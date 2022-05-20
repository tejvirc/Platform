namespace Aristocrat.Monaco.Application.UI.MeterPage
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///    
    /// </summary>
    [CLSCompliant(false)]
    public class ProgressiveDisplayMeter : DisplayMeter
    {
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveDisplayMeter" /> class.
        /// </summary>
        public ProgressiveDisplayMeter(string meterName, bool showLifetime, ObservableCollection<DisplayMeter> collection)
            : base(meterName, null, showLifetime)
        {
            Details = collection;
        }

        /// <summary>
        ///     Details for corresponding Levels
        /// </summary>
        public ObservableCollection<DisplayMeter> Details { get; }

        public DisplayMeter this[string name]
        {
            get { return Details.First(x => x.Name == name); }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var meter in Details)
                {
                    meter.Dispose();
                }

                Details.Clear();
            }

            base.Dispose(disposing);
            _disposed = true;
        }
    }
}