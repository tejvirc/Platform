namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Messaging;
    using Common.Data.Models;
    using Protocol.Common.Storage.Entity;
    using Services.GamePlay;

    /// <summary>
    ///     Handles <see cref="ProgressiveWinner" /> message.
    /// </summary>
    public class ProgressiveWinnerHandler : MessageHandler<ProgressiveWinner>
    {
        private const int MinMessageValueCount = 3;
        private const int AmountIndex = 0;
        private const int PoolNameIndex = 1;
        private const int MachineIdIndex = 2;
        //private const int MessageIndex = 3;
        //private readonly Guid InfoBarOwnershipKey = new Guid("{2E5A3BDD-C474-46B8-92E0-483747BAB74B}");
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IProgressiveController _progressiveController;
        private string _machineId = string.Empty;

        /// <summary>
        ///     Construct a <see cref="ProgressiveWinnerHandler" /> object.
        /// </summary>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory" />.</param>
        /// <param name="progressiveController"><see cref="IProgressiveController" />.</param>
        public ProgressiveWinnerHandler(
            IUnitOfWorkFactory unitOfWorkFactory,
            IProgressiveController progressiveController)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _progressiveController =
                progressiveController ?? throw new ArgumentNullException(nameof(progressiveController));
        }

        /// <inheritdoc />
        public override Task<IResponse> Handle(ProgressiveWinner message)
        {
            // Show message in message line: WinAmountInPennies|ProgressivePoolName|WinnerMachineID|Optional Text Msg 
            var messageValues = message.Message.Split(new[] { '|' }, StringSplitOptions.None);

            if (messageValues.Length >= MinMessageValueCount)
            {
                if (!long.TryParse(messageValues[AmountIndex], out var amountInPennies))
                {
                    amountInPennies = 0;
                }

                if (CheckMachineId(messageValues[MachineIdIndex]))
                {
                    var poolName = messageValues[PoolNameIndex];
                    _progressiveController.AwardJackpot(poolName, amountInPennies);
                }

                //// NOTE: NYL no longer wants progressive messages being sent to InfoBar. If that changes again, uncomment this code
                //if (messageValues.Length > MessageIndex && !string.IsNullOrEmpty(messageValues[MessageIndex]))
                //{
                //var infoBarEvent = new InfoBarDisplayTransientMessageEvent(
                //    InfoBarMessageHandle,
                //    messageValues[MessageIndex],
                //    MgamConstants.PlayerMessageDefaultTextColor,
                //    MgamConstants.PlayerMessageDefaultBackgroundColor,
                //    InfoBarRegion.Center);
                //_bus.Publish(infoBarEvent);
                //}
            }

            return Task.FromResult(Ok<ProgressiveWinnerResponse>());
        }

        private bool CheckMachineId(string machineId)
        {
            if (string.IsNullOrEmpty(_machineId))
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    _machineId = unitOfWork.Repository<Device>().Queryable().Single().Name;
                }
            }

            return _machineId.Equals(machineId);
        }
    }
}