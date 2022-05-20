using Aristocrat.Monaco.Application.Contracts.Media;
using Aristocrat.Monaco.Kernel;

namespace Aristocrat.Monaco.Gaming.UI.ViewModels.MediaDisplay
{
    public class MediaPlayerPlaceholderViewModel : MediaPlayerViewModelBase
    {
        public MediaPlayerPlaceholderViewModel(IMediaPlayer model) : base(model)
        {
            Logger.Debug($"Creating VM for Placeholder Media Player ID {model.Id}");
        }

        protected override void OnVisChangeRequested(bool visibleState)
        {
            LatestVisibleState = visibleState;
        }

        protected override void SetLatestVisibility(bool? latestVisibleState)
        {
            if (latestVisibleState == null || !Model.Enabled) return;

            if (latestVisibleState == IsVisible)
            {
                Logger.Debug($"MediaPlayer {Id} SetLatestVisibility return -- IsVisible already set to LatestVisibleState ({IsVisible})");
                return;
            }

            if (IsAnimating)
            {
                Logger.Debug($"MediaPlayer {Id} SetLatestVisibility return -- IsAnimating=True");
                return;
            }

            if (latestVisibleState == true)
            {
                Logger.Debug($"MediaPlayer {Id} SetLatestVisibility to True");
                ServiceManager.GetInstance().GetService<IMediaProvider>().UpdatePlayer(Model, true);
            }
            else
            {
                Logger.Debug($"MediaPlayer {Id} SetLatestVisibility to False");
                ServiceManager.GetInstance().GetService<IMediaProvider>().UpdatePlayer(Model, false);
            }

            SetVisibility();
            LatestVisibleState = null;
        }

        /// <summary>
        ///     Set media player actual visibility using current states
        /// </summary>
        /// <param name="visible">Pass in default null to use Model.Visible or a value to override Model.Visible</param>
        public override void SetVisibility(bool? visible = null)
        {
            if (visible == false)
            {
                // We can set IsVisible to false immediately, but more checks are required for true in case states have changed
                IsVisible = false;
            }
            else
            {
                lock (DisplayLock)
                {
                    IsVisible = Model.Visible && Model.Enabled;
                }
            }
        }
    }
}
