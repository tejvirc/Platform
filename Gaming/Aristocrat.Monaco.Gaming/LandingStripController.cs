namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Kernel;
    using log4net;
    using DisabledEvent = Hardware.Contracts.NoteAcceptor.DisabledEvent;
    using EnabledEvent = Hardware.Contracts.NoteAcceptor.EnabledEvent;

    public class LandingStripController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static IEdgeLightingController _edgeLightingController;

        private readonly IBank _bank;
        private readonly INoteAcceptor _noteAcceptor;
        private readonly IPrinter _printer;
        private readonly ISystemDisableManager _systemDisableManager;

        private readonly Dictionary<StripIDs, LandingStrip> _landingStrips = new Dictionary<StripIDs, LandingStrip>();

        public LandingStripController(IEventBus eventBus, IEdgeLightingController edgeLightingController, IBank bank, ISystemDisableManager disable)
        {
            if (eventBus == null || bank == null || edgeLightingController == null || disable == null)
            {
                throw new ArgumentNullException();
            }

            _bank = bank;
            _edgeLightingController = edgeLightingController;
            _systemDisableManager = disable;

            var serviceManager = ServiceManager.GetInstance();
            _printer = serviceManager.TryGetService<IPrinter>();
            _noteAcceptor = serviceManager.TryGetService<INoteAcceptor>();

            if (_noteAcceptor != null)
            {
                _landingStrips.Add(StripIDs.LandingStripRight, new LandingStrip((int)StripIDs.LandingStripRight));
                UpdateBnaLandingStrip();

                eventBus.Subscribe<BankBalanceChangedEvent>(this, e => UpdateBnaLandingStrip());
                eventBus.Subscribe<DisabledEvent>(this, e => UpdateBnaLandingStrip());
                eventBus.Subscribe<EnabledEvent>(this, e => UpdateBnaLandingStrip());
            }

            if (_printer != null)
            {
                _landingStrips.Add(StripIDs.LandingStripLeft, new LandingStrip((int)StripIDs.LandingStripLeft));
                UpdatePrinterLandingStrip();

                eventBus.Subscribe<HardwareWarningEvent>(this, e => UpdatePrinterLandingStrip());
                eventBus.Subscribe<HardwareWarningClearEvent>(this, e => UpdatePrinterLandingStrip());
                eventBus.Subscribe<MissedStartupEvent>(this, MissedStartupEventHandler);
            }

            // Update both landing strips any time any disable event occurs
            eventBus.Subscribe<SystemDisabledEvent>(this, e => UpdateBothLandingStrips());
            eventBus.Subscribe<SystemDisableAddedEvent>(this, e => UpdateBothLandingStrips());
            eventBus.Subscribe<SystemDisableUpdatedEvent>(this, e => UpdateBothLandingStrips());
            eventBus.Subscribe<SystemDisableRemovedEvent>(this, e => UpdateBothLandingStrips());
            eventBus.Subscribe<SystemEnabledEvent>(this, e => UpdateBothLandingStrips());
        }

        private void MissedStartupEventHandler(MissedStartupEvent evt)
        {
            if (evt.MissedEvent is HardwareWarningEvent || evt.MissedEvent is HardwareWarningClearEvent)
            {
                UpdatePrinterLandingStrip();
            }
        }

        private void UpdateBothLandingStrips()
        {
            UpdateBnaLandingStrip();
            UpdatePrinterLandingStrip();
        }

        private void UpdateBnaLandingStrip()
        {
            if (!_landingStrips.ContainsKey(StripIDs.LandingStripRight))
            {
                return;
            }

            if (!_noteAcceptor.Enabled || _bank.QueryBalance() > 0 || _systemDisableManager.IsDisabled)
            {
                _landingStrips[StripIDs.LandingStripRight].Idle();
            }
            else
            {
                _landingStrips[StripIDs.LandingStripRight].Active();
            }
        }

        private void UpdatePrinterLandingStrip()
        {
            if (!_landingStrips.ContainsKey(StripIDs.LandingStripLeft))
            {
                return;
            }

            if (!_printer.Warnings.HasFlag(PrinterWarningTypes.PaperInChute) || _systemDisableManager.IsDisabled)
            {
                _landingStrips[StripIDs.LandingStripLeft].Idle();
            }
            else
            {
                _landingStrips[StripIDs.LandingStripLeft].Active();
            }
        }

        private class LandingStrip : IDisposable
        {
            private const int Delay = 300;
            private readonly Stopwatch _stopwatch = new Stopwatch();

            private readonly int _stripId;
            private readonly PatternParameters _idleLandingStrip;
            private readonly PatternParameters _activeLandingStrip;

            private readonly List<(int startLed, int endLed)> _landingStripLeds = new List<(int, int)>
            {
                (1, 5), (6, 9), (10, 12), (13, 13)
            };

            private int _startLedIndex;
            private Color[] _landingStripColors;
            private IEdgeLightToken _landingStripToken;
            private bool _disposed;

            public LandingStrip(int stripId)
            {
                _stripId = stripId;

                _idleLandingStrip = new SolidColorPatternParameters
                {
                    Color = Color.Blue, Strips = new List<int> { stripId }, Priority = StripPriority.LobbyView
                };

                _activeLandingStrip = new IndividualLedPatternParameters
                {
                    Strips = new List<int> { stripId },
                    Priority = StripPriority.LobbyView,
                    StripUpdateFunction = StripUpdateFunction
                };

                _landingStripToken = _edgeLightingController.AddEdgeLightRenderer(_idleLandingStrip);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void Idle()
            {
                if (!_stopwatch.IsRunning)
                {
                    // Already Idle
                    return;
                }

                _edgeLightingController.RemoveEdgeLightRenderer(_landingStripToken);
                _stopwatch.Stop();
                _landingStripToken = _edgeLightingController.AddEdgeLightRenderer(_idleLandingStrip);
                Logger.Debug($"Strip ID {_stripId} IDLE");
            }

            public void Active()
            {
                if (_stopwatch.IsRunning)
                {
                    // Already Active
                    return;
                }

                _edgeLightingController.RemoveEdgeLightRenderer(_landingStripToken);
                _landingStripToken = _edgeLightingController.AddEdgeLightRenderer(_activeLandingStrip);
                _stopwatch.Start();
                Logger.Debug($"Strip ID {_stripId} ACTIVE");
            }

            private Color[] StripUpdateFunction(int stripId, int ledCount)
            {
                if (_stopwatch.Elapsed.TotalMilliseconds < Delay)
                {
                    return _landingStripColors;
                }

                _landingStripColors = Enumerable.Range(1, ledCount).Select(
                    x => x >= _landingStripLeds[_startLedIndex].startLed &&
                         x <= _landingStripLeds[_startLedIndex].endLed
                        ? Color.Blue
                        : Color.Black).ToArray();
                _startLedIndex = (_startLedIndex + 1) % _landingStripLeds.Count;
                _stopwatch.Restart();

                return _landingStripColors;
            }

            private void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    ClearLandingStripLighting();
                    _stopwatch.Stop();
                }

                _disposed = true;
            }

            private void ClearLandingStripLighting()
            {
                if (_landingStripToken != null)
                {
                    _edgeLightingController.RemoveEdgeLightRenderer(_landingStripToken);
                    _landingStripToken = null;
                }
            }
        }
    }
}