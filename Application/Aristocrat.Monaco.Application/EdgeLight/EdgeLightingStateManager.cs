namespace Aristocrat.Monaco.Application.EdgeLight
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.EdgeLight;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using log4net;

    /// <summary>
    ///     The Class that maintains the different edge light states/conditions. (they are not states but rather conditions, as there
    ///     is a list of states inside EdgeLightingControllerService.cs that can be active at the same time but the highest priority one
    ///     will be chosen to be displayed)
    /// </summary>
    public class EdgeLightingStateManager : IEdgeLightingStateManager, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaxChannelBrightness = 100;

        private static readonly IReadOnlyList<int> NonMainCabinetStripIds = new List<int>
        {
            (int)StripIDs.BarkeeperStrip1Led,
            (int)StripIDs.BarkeeperStrip4Led,
            (int)StripIDs.LandingStripLeft,
            (int)StripIDs.LandingStripRight,
            (int)StripIDs.StepperReel1,
            (int)StripIDs.StepperReel2,
            (int)StripIDs.StepperReel3,
            (int)StripIDs.StepperReel4,
            (int)StripIDs.StepperReel5
        };

        private static readonly IReadOnlyList<int> NonOperatorMenuControlledStripIds = new List<int>
        {
            (int)StripIDs.BarkeeperStrip1Led,
            (int)StripIDs.BarkeeperStrip4Led,
            (int)StripIDs.StepperReel1,
            (int)StripIDs.StepperReel2,
            (int)StripIDs.StepperReel3,
            (int)StripIDs.StepperReel4,
            (int)StripIDs.StepperReel5
        };

        private readonly IEdgeLightingController _edgeLightingController;
        private readonly object _lock = new object();

        private readonly Dictionary<IEdgeLightToken, Action<IEdgeLightingController>> _rendererCleanupActions =
            new Dictionary<IEdgeLightToken, Action<IEdgeLightingController>>();

        private readonly Dictionary<EdgeLightState, Func<IEdgeLightingController, IEdgeLightToken>> _stateHandlers;

        public EdgeLightingStateManager()
            : this(
                ServiceManager.GetInstance().GetService<IEdgeLightingController>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public EdgeLightingStateManager(
            IEdgeLightingController edgeLightingController,
            IPropertiesManager propertiesManager)
        {
            _edgeLightingController =
                edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));
            if (propertiesManager == null)
            {
                throw new ArgumentNullException(nameof(propertiesManager));
            }

            _stateHandlers = new Dictionary<EdgeLightState, Func<IEdgeLightingController, IEdgeLightToken>>
            {
                {
                    EdgeLightState.Cashout, edgeLightController => edgeLightController.AddEdgeLightRenderer(
                        new RainbowPatternParameters
                        {
                            Priority = StripPriority.CashOut,
                            Strips = new List<int>
                            {
                                (int)StripIDs.MainCabinetLeft,
                                (int)StripIDs.MainCabinetRight,
                                (int)StripIDs.MainCabinetBottom,
                                (int)StripIDs.MainCabinetTop
                            }
                        })
                },
                {
                    EdgeLightState.DoorOpen, edgeLightController =>
                    {
                        var rendererId = edgeLightController.AddEdgeLightRenderer(
                            new SolidColorPatternParameters { Priority = StripPriority.DoorOpen, Color = Color.White });
                        edgeLightController.SetBrightnessForPriority(MaxChannelBrightness, StripPriority.DoorOpen);
                        _rendererCleanupActions.Add(
                            rendererId,
                            x => { x.ClearBrightnessForPriority(StripPriority.DoorOpen); });
                        return rendererId;
                    }
                },
                {
                    EdgeLightState.Lobby, edgeLightController =>
                    {
                        Color lobbyColor = Color.FromName(propertiesManager.GetValue(ApplicationConstants.EdgeLightingLobbyModeColorOverrideSelectionKey, "Blue"));
                        var rendererId = edgeLightController.AddEdgeLightRenderer(
                            new SolidColorPatternParameters
                            {
                                Priority = StripPriority.LobbyView,
                                Color = lobbyColor,
                                Strips = edgeLightController.StripIds.Except(NonMainCabinetStripIds).ToList()
                            });
                         return rendererId;
                    }
                },
                {
                    EdgeLightState.OperatorMode, edgeLightController =>
                    {
                        var rendererId = edgeLightController.AddEdgeLightRenderer(
                            new SolidColorPatternParameters
                            {
                                Priority = StripPriority.AuditMenu, Color = Color.FromArgb(255, 0, 0, 204),
                                Strips = edgeLightController.StripIds.Except(NonOperatorMenuControlledStripIds).ToList()
                            });
                        edgeLightController.SetBrightnessForPriority(MaxChannelBrightness,
                            StripPriority.AuditMenu);
                        _rendererCleanupActions.Add(
                            rendererId,
                            x => { x.ClearBrightnessForPriority(StripPriority.AuditMenu); });
                        return rendererId;
                    }
                },
                {
                    EdgeLightState.TowerLightMode, edgeLightController =>
                    {
                        if (!propertiesManager.GetValue(ApplicationConstants.EdgeLightAsTowerLightEnabled, false))
                        {
                            return null;
                        }

                        var rendererId = edgeLightController.AddEdgeLightRenderer(
                            new BlinkPatternParameters
                            {
                                Priority = StripPriority.BarTopTowerLight, OnColor = Color.Red, OffColor = Color.Black
                            });
                        edgeLightController.SetBrightnessForPriority(
                            EdgeLightingBrightnessLimits.MaximumBrightness,
                            StripPriority.BarTopTowerLight);
                        _rendererCleanupActions.Add(
                            rendererId,
                            x => { x.ClearBrightnessForPriority(StripPriority.BarTopTowerLight); });
                        return rendererId;
                    }
                },
                {
                    EdgeLightState.DefaultMode, edgeLightController =>
                    {
                        var defaultColor = Color.FromName(propertiesManager.GetValue(ApplicationConstants.EdgeLightingDefaultStateColorOverrideSelectionKey, "Blue") ?? "Blue");
                        var rendererId = edgeLightController.AddEdgeLightRenderer(
                            new SolidColorPatternParameters
                            {
                                Priority = StripPriority.LowPriority,
                                Color = defaultColor,
                                Strips = edgeLightController.StripIds.Except(NonMainCabinetStripIds).ToList()
                            });
                        _rendererCleanupActions.Add(
                            rendererId,
                            x => { x.ClearBrightnessForPriority(StripPriority.LowPriority); });
                        return rendererId;
                    }
                },
                {
                    EdgeLightState.AttractMode, edgeLightController =>
                    {
                        var attractColor = Color.FromName(propertiesManager.GetValue(ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey, "Transparent"));
                        var rendererId = edgeLightController.AddEdgeLightRenderer(
                            new SolidColorPatternParameters
                            {
                                Priority = StripPriority.LobbyView,
                                Color = attractColor,
                                Strips = edgeLightController.StripIds.Except(NonMainCabinetStripIds).ToList()
                            });
                        _rendererCleanupActions.Add(
                            rendererId,
                            x => { x.ClearBrightnessForPriority(StripPriority.LobbyView); });
                        return rendererId;
                    }
                }
            };
            //Adds a default state upon construction as to always display a certain Edgelight in-case nothing else is driving the EdgeLights.
            SetState(EdgeLightState.DefaultMode);
        }

        /// <inheritdoc />
        public IEdgeLightToken SetState(EdgeLightState state)
        {
            lock (_lock)
            {
                Logger.Debug($"SetState to {state}");
                return !_stateHandlers.TryGetValue(state, out var func)
                    ? null
                    : func.Invoke(_edgeLightingController);
            }
        }

        /// <inheritdoc />
        public void ClearState(IEdgeLightToken stateToken)
        {
            lock (_lock)
            {
                _edgeLightingController.RemoveEdgeLightRenderer(stateToken);
                if (stateToken != null && _rendererCleanupActions.TryGetValue(stateToken, out var cleanupAction))
                {
                    cleanupAction?.Invoke(_edgeLightingController);
                }
            }
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IEdgeLightingStateManager) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}