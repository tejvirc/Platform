namespace Aristocrat.Monaco.Sas.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Protocol;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Storage.Models;

    /// <inheritdoc />
    public class ProgressiveWinDetailsProvider : IProgressiveWinDetailsProvider
    {
        private const int MaxNonProgressiveHits = 30;
        private const int MaxSentCount = 8;
        private const byte Client1 = 0;
        private const byte Client2 = 1;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IProgressiveLevelProvider _progressiveLevelProvider;
        private readonly IPersistentBlock _persistentBlock;
        private readonly IPropertiesManager _propertiesManager;
        private readonly bool _isSasProgressives;
        private readonly CommsProtocol _progressiveController;
        private readonly object _nonSasProgressiveLock = new object();
        private ProgressiveWinDetails _lastProgressiveWinDetails;
        private NonSasProgressiveWinDataControl _nonSasProgressiveWinDataControl;
        private bool _host1NonSasReporting;
        private bool _host2NonSasReporting;

        /// <summary>
        ///     Creates and instance of the ProgressiveWinDetailsProvider
        /// </summary>
        /// <param name="protocolLinkedProgressiveAdapter">An instance of <see cref="IProtocolLinkedProgressiveAdapter"/></param>
        /// <param name="persistenceProvider">An instance of <see cref="IPersistenceProvider"/></param>
        /// <param name="multiProtocolConfigurationProvider">An instance of <see cref="IMultiProtocolConfigurationProvider"/></param>
        /// <param name="progressiveLevelProvider">An instance of <see cref="IProgressiveLevelProvider"/></param>
        /// <param name="propertiesManager">An instance of <see cref="IPropertiesManager"/>.</param>
        public ProgressiveWinDetailsProvider(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPersistenceProvider persistenceProvider,
            IMultiProtocolConfigurationProvider multiProtocolConfigurationProvider,
            IProgressiveLevelProvider progressiveLevelProvider,
            IPropertiesManager propertiesManager)
        {
            if (persistenceProvider == null)
            {
                throw new ArgumentNullException(nameof(persistenceProvider));
            }
            if (multiProtocolConfigurationProvider == null)
            {
                throw new ArgumentNullException(nameof(multiProtocolConfigurationProvider));
            }
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _progressiveLevelProvider = progressiveLevelProvider ??
                                        throw new ArgumentNullException(nameof(progressiveLevelProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _progressiveController = multiProtocolConfigurationProvider.MultiProtocolConfiguration
                .FirstOrDefault(x => x.IsProgressiveHandled)?.Protocol ?? CommsProtocol.None;
            _isSasProgressives = _progressiveController == CommsProtocol.SAS;

            _persistentBlock = persistenceProvider.GetOrCreateBlock(GetType().FullName, PersistenceLevel.Transient);
            _lastProgressiveWinDetails =
                _persistentBlock.GetOrCreateValue<ProgressiveWinDetails>(nameof(_lastProgressiveWinDetails)) ??
                new ProgressiveWinDetails();

            _nonSasProgressiveWinDataControl = _persistentBlock.GetOrCreateValue<NonSasProgressiveWinDataControl>(nameof(_nonSasProgressiveWinDataControl)) ??
                                               new NonSasProgressiveWinDataControl();

            UpdateSettings();
        }

        /// <inheritdoc />
        public void SetLastProgressiveWin(IGameHistoryLog log)
        {
            if (!(log?.Jackpots.Any() ?? false))
            {
                return;
            }

            _lastProgressiveWinDetails = GetProgressiveWinDetails(log);
            using (var transaction = _persistentBlock.Transaction())
            {
                transaction.SetValue(nameof(_lastProgressiveWinDetails), _lastProgressiveWinDetails);
                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public ProgressiveWinDetails GetLastProgressiveWin()
        {
            return _lastProgressiveWinDetails;
        }

        /// <inheritdoc />
        public ProgressiveWinDetails GetProgressiveWinDetails(IGameHistoryLog log)
        {
            if (log is null)
            {
                return new ProgressiveWinDetails
                {
                    GroupId = 0,
                    LevelId = 0,
                    WinAmount = 0L
                };
            }

            var expectedLevel = log.Jackpots
               .OrderByDescending(x => x.WinAmount)
               .FirstOrDefault();

            var hitLinkedLevel = _protocolLinkedProgressiveAdapter
                .ViewProgressiveLevels(log.GameId, log.DenomId, expectedLevel?.PackName)
                .FirstOrDefault(
                    x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                         expectedLevel?.LevelId == x.LevelId && expectedLevel.WagerCredits == x.WagerCredits &&
                         (string.IsNullOrEmpty(x.BetOption) || x.BetOption == log.DenomConfiguration?.BetOption));

            var groupId = _isSasProgressives && hitLinkedLevel != null && _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                hitLinkedLevel.AssignedProgressiveId.AssignedProgressiveKey,
                out var linkedLevel)
                ? linkedLevel.ProgressiveGroupId
                : 0;

            return new ProgressiveWinDetails
            {
                GroupId = groupId,
                LevelId = expectedLevel is null ? 0 : (expectedLevel.LevelId + 1),
                WinAmount = log.Jackpots.Sum(x => x.WinAmount)
            };
        }

        /// <inheritdoc />
        public IEnumerable<NonSasProgressiveWinData> GetNonSasProgressiveWinData(byte clientNumber)
        {
            lock (_nonSasProgressiveLock)
            {
                if (clientNumber == Client1)
                {
                    var result = _nonSasProgressiveWinDataControl.Host1Wins.ToList();

                    if (result.Count == 0)
                    {
                        return result;
                    }

                    if (result.Count > MaxSentCount)
                    {
                        result = result.GetRange(0, MaxSentCount);
                    }

                    _nonSasProgressiveWinDataControl.Host1SentCount = result.Count;

                    SaveNonSasProgressiveWinDataControl();

                    return result;
                }

                if (clientNumber == Client2)
                {

                    var result = _nonSasProgressiveWinDataControl.Host2Wins.ToList();

                    if (result.Count == 0)
                    {
                        return result;
                    }

                    if (result.Count > MaxSentCount)
                    {
                        result = result.GetRange(0, MaxSentCount);
                    }

                    _nonSasProgressiveWinDataControl.Host2SentCount = result.Count;

                    SaveNonSasProgressiveWinDataControl();

                    return result;
                }

                return new List<NonSasProgressiveWinData>();
            }
        }

        /// <inheritdoc />
        public void HandleNonSasProgressiveWinDataAcknowledged(byte clientNumber)
        {
            lock (_nonSasProgressiveLock)
            {
                if (clientNumber == Client1)
                {
                    if (_nonSasProgressiveWinDataControl.Host1SentCount == 0)
                    {
                        return;
                    }

                    _nonSasProgressiveWinDataControl.Host1Wins.RemoveRange(0, _nonSasProgressiveWinDataControl.Host1SentCount);
                    _nonSasProgressiveWinDataControl.Host1SentCount = 0;

                    SaveNonSasProgressiveWinDataControl();
                }

                if (clientNumber == Client2)
                {
                    if (_nonSasProgressiveWinDataControl.Host2SentCount == 0)
                    {
                        return;
                    }

                    _nonSasProgressiveWinDataControl.Host2Wins.RemoveRange(0, _nonSasProgressiveWinDataControl.Host2SentCount);
                    _nonSasProgressiveWinDataControl.Host2SentCount = 0;

                    SaveNonSasProgressiveWinDataControl();
                }
            }
        }

        /// <inheritdoc />
        public void AddNonSasProgressiveLevelWin(IViewableProgressiveLevel level, JackpotTransaction jackpotTransaction)
        {
            var sapLevels = _progressiveLevelProvider.GetProgressiveLevels().Count(l => l.LevelType == ProgressiveLevelType.Sap);
            int levelId;
            int controllerType;
            var controllerId = 0;
            
            if (level.LevelId < sapLevels)
            {
                // SAP levels are always first thus this level matches reported level plus one for SAS
                levelId =  + 1;

                controllerType = (int)ProgressiveControllerType.SAP; // Standalone protocol (gaming machine internal)
            }
            else
            {
                // SAP levels are always first thus this all other progressives must subtract off the SAP levels first and reported level plus one for SAS
                levelId = level.LevelId - sapLevels + 1;

                switch (_progressiveController)
                {
                    case CommsProtocol.G2S:
                        controllerType = (int)ProgressiveControllerType.G2S; // G2S protocol
                        // TODO: G2S Progressives not implemented yet
                        // According to the protocol for G2S controllers the device Id should be used for the Controller Id
                        controllerId = 1;
                        break;
                    default:
                        controllerType = (int)ProgressiveControllerType.OtherLinked; // Other link protocol
                        break;
                }
            }
            var win = new NonSasProgressiveWinData(controllerType, controllerId, levelId, jackpotTransaction.WinAmount.MillicentsToCents(), level.ResetValue.MillicentsToCents(), level.Overflow.MillicentsToCents());
            lock (_nonSasProgressiveLock)
            {
                if (_host1NonSasReporting)
                {
                    _nonSasProgressiveWinDataControl.Host1Wins.Add(win);

                    if (_nonSasProgressiveWinDataControl.Host1Wins.Count > MaxNonProgressiveHits)
                    {
                        _nonSasProgressiveWinDataControl.Host1Wins.RemoveAt(0);

                        if (_nonSasProgressiveWinDataControl.Host1SentCount > 0)
                        {
                            _nonSasProgressiveWinDataControl.Host1SentCount--;
                        }
                    }
                }

                if (_host2NonSasReporting)
                {
                    _nonSasProgressiveWinDataControl.Host2Wins.Add(win);

                    if (_nonSasProgressiveWinDataControl.Host2Wins.Count > MaxNonProgressiveHits)
                    {
                        _nonSasProgressiveWinDataControl.Host2Wins.RemoveAt(0);

                        if (_nonSasProgressiveWinDataControl.Host2SentCount > 0)
                        {
                            _nonSasProgressiveWinDataControl.Host2SentCount--;
                        }
                    }
                }

                SaveNonSasProgressiveWinDataControl();
            }
        }

        /// <inheritdoc />
        public bool HasNonSasProgressiveWinData(byte clientNumber)
        {
            lock (_nonSasProgressiveLock)
            {
                if (clientNumber == Client1)
                {
                    return _nonSasProgressiveWinDataControl.Host1Wins.Count > 0;
                }

                if (clientNumber == Client2)
                {
                    return _nonSasProgressiveWinDataControl.Host2Wins.Count > 0;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public void UpdateSettings()
        {
            lock (_nonSasProgressiveLock)
            {
                var settings = _propertiesManager.GetValue(SasProperties.SasPortAssignments, new PortAssignment());
                var update = false;
                if (_host1NonSasReporting && !settings.Host1NonSasProgressiveHitReporting)
                {
                    if (_nonSasProgressiveWinDataControl.Host1Wins.Count > 0)
                    {
                        _nonSasProgressiveWinDataControl.Host1SentCount = 0;
                        _nonSasProgressiveWinDataControl.Host1Wins.Clear();
                        update = true;
                    }
                }

                if (_host2NonSasReporting && !settings.Host2NonSasProgressiveHitReporting)
                {
                    if (_nonSasProgressiveWinDataControl.Host2Wins.Count > 0)
                    {
                        _nonSasProgressiveWinDataControl.Host2SentCount = 0;
                        _nonSasProgressiveWinDataControl.Host2Wins.Clear();
                        update = true;
                    }
                }

                _host1NonSasReporting = settings.Host1NonSasProgressiveHitReporting;
                _host2NonSasReporting = settings.Host2NonSasProgressiveHitReporting;

                if (update)
                {
                    SaveNonSasProgressiveWinDataControl();
                }
            }
        }

        private void SaveNonSasProgressiveWinDataControl()
        {
            using var transaction = _persistentBlock.Transaction();
            transaction.SetValue(nameof(_nonSasProgressiveWinDataControl), _nonSasProgressiveWinDataControl);
            transaction.Commit();
        }

        // Refer: IGT Slot Accounting System Version 6.03 Table 18.6.1 Send Configured Progressive Controllers Response
        // Progressive controller type
        //  01 = SAS protocol
        //  02 = Mikohn protocol
        //  03 = IPP protocol
        //  04 = G2S protocol
        //  05 = WAP protocol
        //  06 = Standalone protocol (gaming machine internal)
        //  07 = Game-based protocol (gaming machine internal)
        //  08 = Other link protocol
        //  09-1F = Reserved
        private enum ProgressiveControllerType
        {
            //None,
            //SAS,
            //Mikohn,
            //Ipp,
            G2S = 4,
            //Wap,
            SAP = 6,
            //Game,
            OtherLinked = 8
        }
    }
}