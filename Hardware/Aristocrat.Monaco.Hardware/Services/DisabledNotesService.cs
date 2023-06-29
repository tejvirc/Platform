namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Contracts.NoteAcceptor;
    using Contracts.Persistence;
    using Kernel;
    using log4net;

    public sealed class DisabledNotesService : IService, IDisabledNotesService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPersistenceProvider _persistenceProvider;

        public DisabledNotesService(IPersistenceProvider persistenceProvider)
        {
            _persistenceProvider = persistenceProvider ?? throw new ArgumentNullException(nameof(persistenceProvider));
        }

        /// <summary>
        ///     Gets or sets the disabled notes persistent block.
        /// </summary>
        private IPersistentBlock DisabledNotesPersistentBlock { get; set; }

        /// <summary>
        ///     Returns whether disabled notes service is initialized or not.
        /// </summary>
        private bool Initialized { get; set; }

        /// <inheritdoc />
        public NoteInfo NoteInfo
        {
            get
            {
                DisabledNotesPersistentBlock.GetValue(HardwareConstants.DisabledNotes, out NoteInfo noteInfo);
                return noteInfo;
            }

            set
            {
                using (var transaction = DisabledNotesPersistentBlock.Transaction())
                {
                    transaction.SetValue(HardwareConstants.DisabledNotes, value);
                    transaction.Commit();
                }
            }
        }

        public string Name => nameof(IDisabledNotesService);

        public ICollection<Type> ServiceTypes => new[] { typeof(IDisabledNotesService) };

        public void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            Logger.Info("Initializing");

            DisabledNotesPersistentBlock = _persistenceProvider.GetOrCreateBlock(
                HardwareConstants.DisabledNotes,
                PersistenceLevel.Critical);

            Initialized = true;

            Logger.Info("Finished initialization");
        }
    }
}