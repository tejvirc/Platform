namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Contracts;
    using Contracts.AlteredMediaLogger;
    using Kernel;
    using Models;
    using OperatorMenu;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///     ViewModel for Altered Media Log
    /// </summary>
    [CLSCompliant(false)]
    public class AlteredMediaLogViewModel : OperatorMenuPageViewModelBase
    {
        private readonly ITime _timeService;
        private ObservableCollection<AlteredMediaLogInfo> _alteredMedialLogs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AlteredMediaLogViewModel" /> class.
        /// </summary>
        public AlteredMediaLogViewModel()
        {
            _alteredMedialLogs = new ObservableCollection<AlteredMediaLogInfo>();
            _timeService = ServiceManager.GetInstance().GetService<ITime>();
        }

        /// <summary>
        ///     Gets the data to show in the data grid.
        /// </summary>
        public ObservableCollection<AlteredMediaLogInfo> AlteredMediaLogData
        {
            get => _alteredMedialLogs;
            set
            {
                _alteredMedialLogs = value;
                OnPropertyChanged(nameof(AlteredMediaLogData));
            }
        }

        protected override void OnLoaded()
        {
            LoadAlteredMediaLogData();
        }

        private void LoadAlteredMediaLogData()
        {
            AlteredMediaLogData.Clear();

            var alterMedia = ServiceManager.GetInstance().GetService<IAlteredMediaLogger>();
            var alteredMediaHistory = alterMedia.Logs.OrderByDescending(l => l.TimeStamp).ToList();

            foreach (var history in alteredMediaHistory)
            {
                AlteredMediaLogData.Add(
                    new AlteredMediaLogInfo
                    {
                        TimeStamp = _timeService.GetLocationTime(history.TimeStamp),
                        MediaType = history.MediaType,
                        ReasonForChange = history.ReasonForChange,
                        Authentication = history.Authentication
                    });
            }
        }
    }
}