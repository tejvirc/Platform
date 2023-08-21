namespace Aristocrat.Monaco.Hardware.Serial.NoteAcceptor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using Contracts.Gds;
    using Contracts.Gds.NoteAcceptor;
    using log4net;
    using Protocols;

    /// <summary>A serial note acceptor.</summary>
    public abstract class SerialNoteAcceptor : SerialDeviceProtocol
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private StackerStatus _stackerStatus = new StackerStatus();
        private NoteOrTicketStatus _noteOrTicketStatus = new NoteOrTicketStatus();
        protected readonly Dictionary<int, ReadNoteTable> NoteTable = new Dictionary<int, ReadNoteTable>();
        protected const int DefaultMsInEscrow = 5000; // GDS spec
        protected readonly Stopwatch EscrowWatch = new Stopwatch();

        /// <summary>
        ///     Construct
        /// </summary>
        protected SerialNoteAcceptor(IMessageTemplate newDefaultTemplate = null) : base(newDefaultTemplate)
        {
        }

        protected StackerStatus StackerStatus
        {
            get => _stackerStatus;
            set
            {
                if (_stackerStatus.Equals(value))
                {
                    return;
                }

                if (OnMessageReceived(value))
                {
                    _stackerStatus = value;
                }
            }
        }

        protected NoteOrTicketStatus NoteOrTicketStatus
        {
            get => _noteOrTicketStatus;
            set
            {
                if (_noteOrTicketStatus.Equals(value))
                {
                    return;
                }

                if (OnMessageReceived(value))
                {
                    _noteOrTicketStatus = value;
                }
            }
        }

        /// <inheritdoc/>
        protected override bool HasDisablingFault =>
            StackerStatus.Fault || StackerStatus.Disconnect || StackerStatus.Full || StackerStatus.Jam;

        /// <inheritdoc/>
        protected override void ProcessMessage(GdsSerializableMessage message, CancellationToken token)
        {
            switch (message.ReportId)
            {
                // For GDS note acceptors
                case GdsConstants.ReportId.NoteAcceptorNumberOfNoteDataEntries:
                    GetCurrencyAssignment();
                    break;
                case GdsConstants.ReportId.NoteAcceptorReadNoteTable:
                    foreach (var note in NoteTable.Values)
                    {
                        OnMessageReceived(note);
                    }
                    break;
                case GdsConstants.ReportId.NoteAcceptorExtendTimeout:
                    if (!HasDisablingFault)
                    {
                        HoldInEscrow();
                    }
                    break;
                case GdsConstants.ReportId.NoteAcceptorAcceptNoteOrTicket:
                    if (!HasDisablingFault)
                    {
                        Accept();
                    }
                    break;
                case GdsConstants.ReportId.NoteAcceptorReturnNoteOrTicket:
                    Return();
                    break;
                case GdsConstants.ReportId.NoteAcceptorReadMetrics:
                    OnMessageReceived(new Metrics { Data = string.Empty });
                    break;

                // Other
                default:
                    base.ProcessMessage(message, token);
                    break;
            }
        }

        /// <summary>
        ///     Get currency data from device.
        /// </summary>
        protected abstract void GetCurrencyAssignment();

        /// <summary>
        ///     Accept the note/ticket that's currently in escrow
        /// </summary>
        protected abstract void Accept();

        /// <summary>
        ///     Return the note/ticket that's currently in escrow
        /// </summary>
        protected abstract void Return();

        /// <summary>
        ///     Extend the escrow period
        /// </summary>
        protected abstract void HoldInEscrow();
    }
}