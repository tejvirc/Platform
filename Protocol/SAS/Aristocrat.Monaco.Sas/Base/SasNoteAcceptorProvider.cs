namespace Aristocrat.Monaco.Sas.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Storage.Models;
    using Storage.Repository;
    using Contracts.Client;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Kernel;

    /// <inheritdoc />
    public class SasNoteAcceptorProvider : ISasNoteAcceptorProvider
    {
        private readonly INoteAcceptor _noteAcceptor;
        private readonly object _lockObject = new object();
        private readonly IStorageDataProvider<SasNoteAcceptorDisableInformation> _disableNoteAcceptorDataProvider;

        private bool _billDisableAfterAccept;
        private bool _diagnosticTestActive;

        /// <summary>
        ///     Creates the SasNoteAcceptorProvider instance
        /// </summary>
        public SasNoteAcceptorProvider(IStorageDataProvider<SasNoteAcceptorDisableInformation> disableNoteAcceptorDataProvider)
        {
            _noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            _disableNoteAcceptorDataProvider = disableNoteAcceptorDataProvider ?? throw new ArgumentNullException(nameof(disableNoteAcceptorDataProvider));

            var information = disableNoteAcceptorDataProvider.GetData();

            if ((information.DisableReasons & DisabledReasons.Backend) != 0)
            {
                _noteAcceptor?.Disable(DisabledReasons.Backend);
            }
        }

        /// <inheritdoc />
        public async Task EnableBillAcceptor()
        {
            _noteAcceptor?.Enable(EnabledReasons.Backend);
            await SaveDisableState(false);
        }

        private async Task SaveDisableState(bool isDisabling)
        {
            var information = _disableNoteAcceptorDataProvider.GetData();
            if (isDisabling)
            {
                information.DisableReasons |= DisabledReasons.Backend;
            }
            else
            {
                information.DisableReasons &= ~DisabledReasons.Backend;
            }

            await _disableNoteAcceptorDataProvider.Save(information);
        }

        /// <inheritdoc />
        public async Task DisableBillAcceptor()
        {
            _noteAcceptor?.Disable(DisabledReasons.Backend);
            await SaveDisableState(true);
        }

        /// <inheritdoc />
        public bool ConfigureBillDenominations(IEnumerable<ulong> denominations)
        {
            var noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            if (noteAcceptor == null)
            {
                return false;
            }

            var enumerable = denominations.ToList();

            foreach (var denom in noteAcceptor.GetSupportedNotes())
            {
                var denomInMinorUnits = (ulong)(denom * (long)CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit);

                noteAcceptor.UpdateDenom(denom, enumerable.Contains(denomInMinorUnits));
            }
            
            return true;
        }

        /// <inheritdoc />
        public bool BillDisableAfterAccept
        {
            get
            {
                lock (_lockObject)
                {
                    return _billDisableAfterAccept;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _billDisableAfterAccept = value;
                }
            }
        }

        /// <inheritdoc />
        public bool DiagnosticTestActive
        {
            get
            {
                lock (_lockObject)
                {
                    return _diagnosticTestActive;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _diagnosticTestActive = value;
                }
            }
        }
    }
}