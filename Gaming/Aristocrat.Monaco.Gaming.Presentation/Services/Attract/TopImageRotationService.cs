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

    public class TopImageRotationService : ITopImageRotationService, IDisposable
    {
        private const double RotateTopImageIntervalInSeconds = 10.0;

        private readonly IState<AttractState> _attractState;
        private readonly AttractOptions _attractOptions;
        private readonly IDispatcher _dispatcher;

        private readonly Timer _rotateTopImageTimer;


        public TopImageRotationService(
            IState<AttractState> attractState,
            IOptions<AttractOptions> attractOptions,
            IDispatcher dispatcher)
        {
            _attractState = attractState;
            _dispatcher = dispatcher;
            _attractOptions = attractOptions.Value;

            _rotateTopImageTimer = new Timer { Interval = TimeSpan.FromSeconds(RotateTopImageIntervalInSeconds).TotalMilliseconds };
            _rotateTopImageTimer.Elapsed += RotateTopImageTimerTick;
        }

        public void Dispose()
        {
            _rotateTopImageTimer.Stop();
        }

        public void RotateTopImage()
        {
            if (!(_attractOptions.TopImageRotation is { Count: > 0 }))
            {
                return;
            }

            var newIndex = _attractState.Value.ModeTopImageIndex + 1;

            if (newIndex < 0 || newIndex >= _attractOptions.TopImageRotation.Count)
            {
                newIndex = 0;
            }

            _dispatcher.Dispatch(new AttractUpdateTopImageIndexAction { Index = newIndex });
        }


        private void RotateTopImageTimerTick(object? sender, EventArgs e)
        {
            if (_attractState.Value.IsPlaying)
            {
                _dispatcher.Dispatch(new AttractRotateTopImageAction());
            }
        }
    }
}
