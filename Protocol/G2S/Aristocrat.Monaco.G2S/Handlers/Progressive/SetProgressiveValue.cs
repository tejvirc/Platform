namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <inheritdoc />
    public class SetProgressiveValue : ICommandHandler<progressive, setProgressiveValue>
    {
        public async Task<Error> Verify(ClassCommand<progressive, setProgressiveValue> command)
        {
            return await Task.FromResult(new Error(ErrorCode.G2S_APX008));

            //return await Sanction.OnlyOwner<IProgressiveDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<progressive, setProgressiveValue> command)
        {
            await Task.CompletedTask;
        }
    }
}