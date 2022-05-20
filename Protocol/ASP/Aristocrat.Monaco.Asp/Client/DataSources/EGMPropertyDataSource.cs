namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Application.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Contracts;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Vgt.Client12.Application.OperatorMenu;

    public class EgmPropertyDataSource : IDisposableDataSource
    {
        private readonly Dictionary<string, Func<object>> _handlers;
        private readonly IPropertiesManager _propertyManager;
        private readonly IPersistentStorageAccessor _persistentStorageAccessor;
        private readonly IEventBus _eventBus;
        private readonly IAspGameProvider _aspGameProvider;
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;
        private bool _disposed;

        private string SerialNumber => _propertyManager.GetProperty(ApplicationConstants.SerialNumber, string.Empty).ToString();
        private string FirmwareId => _propertyManager.GetProperty(KernelConstants.SystemVersion, string.Empty).ToString().Replace(".", "");
        private string FirmwareVersionNumber => FirmwareId;
        private string CurrencyName => _propertyManager.GetProperty(ApplicationConstants.CurrencyId, string.Empty).ToString();
        private byte EgmSecurityMode => 1;
        private byte CardInsertedValue => (byte)_persistentStorageAccessor["CardInsertedField"];

        private const int DefaultSelectedGame = 0;
        private long _selectedDenom;
        private int _selectedGameId;
        private bool _denomSelected;

        public string Name { get; } = "EGMProperty";
        public IReadOnlyList<string> Members => _handlers.Keys.ToList();
        public event EventHandler<Dictionary<string, object>> MemberValueChanged;
        public EgmPropertyDataSource(
            IAspGameProvider aspGameProvider,
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            IPersistentStorageManager persistentStorageManager,
            IOperatorMenuLauncher operatorMenuLauncher)
        {
            _handlers = new Dictionary<string, Func<object>>
            {
                { "Serial_Number", () => SerialNumber },
                { "Firmware_ID", () => FirmwareId },
                { "Firmware_VerNo", () => FirmwareVersionNumber },
                { "Currency", () => CurrencyName },
                { "EGM_Security_Mode", () => EgmSecurityMode },
                { "Card_Inserted", () => CardInsertedValue },
                { "Game_Being_Played", GetCurrentActiveGame },
            };

            _aspGameProvider = aspGameProvider ?? throw new ArgumentNullException(nameof(aspGameProvider));
            _propertyManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            persistentStorageManager = persistentStorageManager ?? throw new ArgumentNullException(nameof(persistentStorageManager));

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<GameConnectedEvent>(this, GameConnectedEventHandler);
            _eventBus.Subscribe<GameSelectedEvent>(this, GameSelectedEventHandler);
            _eventBus.Subscribe<DenominationSelectedEvent>(this, InGameDenomChangedEventHandler);
            _eventBus.Subscribe<GameExitedNormalEvent>(this, _ => GameExitedNormalEventHandler());
            _eventBus.Subscribe<GameProcessExitedEvent>(this, _ => GameProcessExitedEventHandler());

            string storageName = GetType().ToString();

            _persistentStorageAccessor = persistentStorageManager.BlockExists(storageName) ? persistentStorageManager.GetBlock(storageName) :
                persistentStorageManager.CreateBlock(PersistenceLevel.Critical, storageName, 1);
            _operatorMenuLauncher = operatorMenuLauncher ?? throw new ArgumentNullException(nameof(operatorMenuLauncher));
        }

        public object GetMemberValue(string member)
        {
            return _handlers[member]();
        }

        public void SetMemberValue(string member, object value)
        {
            if (member == "Card_Inserted")
            {
                byte cardId = Convert.ToByte(value);
                byte insertedCardId = cardId;

                if (insertedCardId > CardInserted.HighestTechnicianCard)
                    throw new Exception($"Id: {cardId} is not a valid Id number");

                using (var persistentStorageTransaction = _persistentStorageAccessor.StartTransaction())
                {
                    persistentStorageTransaction["CardInsertedField"] = cardId;
                    persistentStorageTransaction.Commit();
                }
            }
        }

        private void GameExitedNormalEventHandler()
        {
            if (!_operatorMenuLauncher.IsShowing)
            {
                _denomSelected = false;
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Game_Being_Played"));
            }
        }

        private void GameProcessExitedEventHandler()
        {
            if (_selectedGameId > 0)
            {
                _denomSelected = false;
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Game_Being_Played"));
            }
        }

        private void GameConnectedEventHandler(GameConnectedEvent theEvent)
        {
            var selectedGameId = _propertyManager.GetValue(GamingConstants.SelectedGameId, DefaultSelectedGame);
            var selectedDenom = _propertyManager.GetValue(GamingConstants.SelectedDenom, (long) DefaultSelectedGame);
            if (selectedGameId != _selectedGameId || selectedDenom != _selectedDenom)
            {
                _denomSelected = false;
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Game_Being_Played"));
            }
        }

        private void GameSelectedEventHandler(GameSelectedEvent theEvent)
        {
            _selectedGameId = theEvent.GameId;
            _selectedDenom = theEvent.Denomination;
            _denomSelected = true;
            MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Game_Being_Played"));
        }

        private void InGameDenomChangedEventHandler(DenominationSelectedEvent theEvent)
        {
            _selectedGameId = theEvent.GameId;
            _selectedDenom = theEvent.Denomination;
            _denomSelected = true;
            MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Game_Being_Played"));
        }

        private object GetCurrentActiveGame()
        {
            var isGameRunning = (bool)_propertyManager.GetProperty(GamingConstants.IsGameRunning, false);
            if (!_denomSelected)
            {
                _selectedGameId = isGameRunning
                    ? (int)_propertyManager.GetProperty(GamingConstants.SelectedGameId, DefaultSelectedGame)
                    : DefaultSelectedGame;
                _selectedDenom = isGameRunning
                    ? _propertyManager.GetValue(GamingConstants.SelectedDenom, (long) DefaultSelectedGame)
                    : DefaultSelectedGame;
            }

            return _aspGameProvider.GetEnabledGames().ToList().FindIndex(
                0,
                t => t.game.Id == _selectedGameId && t.denom.Value == _selectedDenom) + 1;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
