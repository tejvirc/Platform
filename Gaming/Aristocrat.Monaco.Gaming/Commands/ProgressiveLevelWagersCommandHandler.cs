namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Progressives;

    public class ProgressiveLevelWagersCommandHandler : ICommandHandler<ProgressiveLevelWagers>
    {
        private readonly IProgressiveGameProvider _progressiveGameProvider;

        public ProgressiveLevelWagersCommandHandler(IProgressiveGameProvider progressiveGameProvider)
        {
            _progressiveGameProvider = progressiveGameProvider ??
                                       throw new ArgumentNullException(nameof(progressiveGameProvider));
        }

        public void Handle(ProgressiveLevelWagers command)
        {
            _progressiveGameProvider.SetProgressiveWagerAmounts(
                command.LevelWagers.Select(x => x.CentsToMillicents()).ToList());
        }
    }
}