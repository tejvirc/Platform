namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using OperatorMenu;
    using System;
    using System.Diagnostics;
    using System.Linq;

    [CLSCompliant(false)]
    public class DiagnosticViewMemoryViewModel : OperatorMenuSaveViewModelBase
    {
        private string _pagedMemorySize64;
        private string _peakPagedMemorySize64;
        private string _peakWorkingSet64;
        private string _privateMemorySize64;
        private string _virtualMemorySize64;
        private string _workingSet64;
        private string _pagedSystemMemorySize64;
        private string _nonPagedSystemMemorySize64;
        private string _threadCount;
        private string _handleCount;

        public DiagnosticViewMemoryViewModel()
        {
            GetStats();
        }

        public string PagedMemorySize64
        {
            get => _pagedMemorySize64;

            set
            {
                if (_pagedMemorySize64 != value)
                {
                    _pagedMemorySize64 = value;
                    RaisePropertyChanged(nameof(PagedMemorySize64));
                }
            }
        }

        public string PeakPagedMemorySize64
        {
            get => _peakPagedMemorySize64;

            set
            {
                if (_peakPagedMemorySize64 != value)
                {
                    _peakPagedMemorySize64 = value;
                    RaisePropertyChanged(nameof(PeakPagedMemorySize64));
                }
            }
        }

        public string PeakWorkingSet64
        {
            get => _peakWorkingSet64;

            set
            {
                if (_peakWorkingSet64 != value)
                {
                    _peakWorkingSet64 = value;
                    RaisePropertyChanged(nameof(PeakWorkingSet64));
                }
            }
        }

        public string PrivateMemorySize64
        {
            get => _privateMemorySize64;

            set
            {
                if (_privateMemorySize64 != value)
                {
                    _privateMemorySize64 = value;
                    RaisePropertyChanged(nameof(PrivateMemorySize64));
                }
            }
        }

        public string VirtualMemorySize64
        {
            get => _virtualMemorySize64;

            set
            {
                if (_virtualMemorySize64 != value)
                {
                    _virtualMemorySize64 = value;
                    RaisePropertyChanged(nameof(VirtualMemorySize64));
                }
            }
        }

        public string WorkingSet64
        {
            get => _workingSet64;

            set
            {
                if (_workingSet64 != value)
                {
                    _workingSet64 = value;
                    RaisePropertyChanged(nameof(WorkingSet64));
                }
            }
        }

        public string PagedSystemMemorySize64
        {
            get => _pagedSystemMemorySize64;

            set
            {
                if (_pagedSystemMemorySize64 != value)
                {
                    _pagedSystemMemorySize64 = value;
                    RaisePropertyChanged(nameof(PagedSystemMemorySize64));
                }
            }
        }

        public string NonPagedSystemMemorySize64
        {
            get => _nonPagedSystemMemorySize64;

            set
            {
                if (_nonPagedSystemMemorySize64 != value)
                {
                    _nonPagedSystemMemorySize64 = value;
                    RaisePropertyChanged(nameof(NonPagedSystemMemorySize64));
                }
            }
        }

        public string ThreadCount
        {
            get => _threadCount;

            set
            {
                if (_threadCount != value)
                {
                    _threadCount = value;
                    RaisePropertyChanged(nameof(ThreadCount));
                }
            }
        }

        public string HandleCount
        {
            get => _handleCount;

            set
            {
                if (_handleCount != value)
                {
                    _handleCount = value;
                    RaisePropertyChanged(nameof(HandleCount));
                }
            }
        }

        private void GetStats()
        {
            var monaco = Process.GetProcessesByName("Bootstrap").FirstOrDefault();
            if (monaco != null)
            {
                PagedMemorySize64 = monaco.PagedMemorySize64.ToString("###,###,###") + " KB";
                PeakPagedMemorySize64 = monaco.PeakPagedMemorySize64.ToString("###,###,###") + " KB";
                PeakWorkingSet64 = monaco.PeakWorkingSet64.ToString("###,###,###") + " KB";
                PrivateMemorySize64 = monaco.PrivateMemorySize64.ToString("###,###,###") + " KB";
                VirtualMemorySize64 = monaco.VirtualMemorySize64.ToString("###,###,###") + " KB";
                WorkingSet64 = monaco.WorkingSet64.ToString("###,###,###") + " KB";
                PagedSystemMemorySize64 = monaco.PagedSystemMemorySize64.ToString("###,###,###") + " KB";
                NonPagedSystemMemorySize64 = monaco.NonpagedSystemMemorySize64.ToString("###,###,###") + " KB";
                ThreadCount = monaco.Threads.Count.ToString("D");
                HandleCount = monaco.HandleCount.ToString("D");
            }
        }
    }
}
