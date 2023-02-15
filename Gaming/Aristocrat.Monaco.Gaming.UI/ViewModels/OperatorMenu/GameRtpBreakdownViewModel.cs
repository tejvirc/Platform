namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
    using Models;

    public class GameRtpBreakdownViewModel : OperatorMenuSaveViewModelBase
    {
        public GameRtpBreakdownViewModel(GameRtpDisplay rtpDisplayValues)
        {
            RtpValues = rtpDisplayValues;
        }

        public GameRtpDisplay RtpValues { get; }
    }
}