namespace Aristocrat.Monaco.Gaming.Presentation.Services.Attract
{
    using Options;
    using Store.Attract;
    using Store;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fluxor;
    using Microsoft.Extensions.Logging;
    using Kernel;
    using System.Timers;
    using Extensions.Fluxor;
    using Accounting.Contracts;
    using Microsoft.Extensions.Options;

    public class TopperImageRotationService : ITopperImageRotationService, IDisposable
    {
        private const double RotateTopperImageIntervalInSeconds = 10.0;

        private readonly IState<AttractState> _attractState;
        private readonly AttractOptions _attractOptions;
        private readonly IDispatcher _dispatcher;

        private readonly Timer _rotateTopperImageTimer;


        public TopperImageRotationService(
            IState<AttractState> attractState,
            IOptions<AttractOptions> attractOptions,
            IDispatcher dispatcher)
        {
            _attractState = attractState;
            _dispatcher = dispatcher;
            _attractOptions = attractOptions.Value;

            _rotateTopperImageTimer = new Timer { Interval = TimeSpan.FromSeconds(RotateTopperImageIntervalInSeconds).TotalMilliseconds };
            _rotateTopperImageTimer.Elapsed += RotateTopperImageTimerTick;
        }

        public void Dispose()
        {
            _rotateTopperImageTimer.Stop();
        }

        public void RotateTopperImage()
        {
            if (!(_attractOptions.TopperImageRotation is { Count: > 0 }))
            {
                return;
            }

            var newIndex = _attractState.Value.ModeTopperImageIndex + 1;

            if (newIndex < 0 || newIndex >= _attractOptions.TopperImageRotation.Count)
            {
                newIndex = 0;
            }

            _dispatcher.Dispatch(new AttractUpdateTopperImageIndexAction { Index = newIndex });
        }


        private void RotateTopperImageTimerTick(object? sender, EventArgs e)
        {
            if (_attractState.Value.IsPlaying)
            {
                _dispatcher.Dispatch(new AttractRotateTopperImageAction());
            }
        }
    }
}
