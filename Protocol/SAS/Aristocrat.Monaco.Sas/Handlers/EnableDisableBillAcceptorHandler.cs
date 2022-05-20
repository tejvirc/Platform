namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;

    /// <summary>
    ///     The handler to enable/disable the bill acceptor
    /// </summary>
    public class EnableDisableBillAcceptorHandler : ISasLongPollHandler<EnableDisableResponse, EnableDisableData>
    {
        private readonly ISasNoteAcceptorProvider _noteAcceptorProvider;

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.EnableBillAcceptor, LongPoll.DisableBillAcceptor };

        /// <summary>
        ///     Creates the EnableDisableBillAcceptorHandler instance
        /// </summary>
        /// <param name="noteAcceptorProvider">The note acceptor provider</param>
        public EnableDisableBillAcceptorHandler(ISasNoteAcceptorProvider noteAcceptorProvider)
        {
            _noteAcceptorProvider = noteAcceptorProvider ?? throw new ArgumentNullException(nameof(noteAcceptorProvider));
        }

        /// <inheritdoc/>
        public EnableDisableResponse Handle(EnableDisableData data)
        {
            // disabling bill acceptor could take a while so do it asynchronously
            EnableDisableBillAcceptorAsync(data.Enable);

            return new EnableDisableResponse { Succeeded = true };
        }

        private void EnableDisableBillAcceptorAsync(bool enable)
        {
            Task.Run(
                async () =>
                {
                    if (enable)
                    {
                        await _noteAcceptorProvider.EnableBillAcceptor();
                    }
                    else
                    {
                        await _noteAcceptorProvider.DisableBillAcceptor();
                    }
                });
        }
    }
}