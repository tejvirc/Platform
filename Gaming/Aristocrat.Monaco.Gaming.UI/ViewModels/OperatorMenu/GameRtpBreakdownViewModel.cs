namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Contracts.Progressives;
    using Contracts.Rtp;

    public class GameRtpBreakdownViewModel : OperatorMenuPageViewModelBase, IModalDialogViewModel
    {
        public GameRtpBreakdownViewModel(Args args)
        {
            StandaloneProgressiveResetRtpState = args.StandaloneProgressiveResetRtpState;
            StandaloneProgressiveIncrementRtpState = args.StandaloneProgressiveIncrementRtpState;
            LinkedProgressiveResetRtpState = args.LinkedProgressiveResetRtpState;
            LinkedProgressiveIncrementRtpState = args.LinkedProgressiveIncrementRtpState;

            BaseGameRtpMin = args.Breakdown.Base.Minimum.ToRtpString();
            BaseGameRtpMax = args.Breakdown.Base.Maximum.ToRtpString();
            StandaloneProgressiveResetRtpMin = args.Breakdown.Base.Minimum.ToRtpString();
            StandaloneProgressiveResetRtpMax = args.Breakdown.Base.Minimum.ToRtpString();
            StandaloneProgressiveIncrementRtpMin = args.Breakdown.Base.Minimum.ToRtpString();
            StandaloneProgressiveIncrementRtpMax = args.Breakdown.Base.Minimum.ToRtpString();
            LinkedProgressiveResetRtpMin = args.Breakdown.Base.Minimum.ToRtpString();
            LinkedProgressiveResetRtpMax = args.Breakdown.Base.Minimum.ToRtpString();
            LinkedProgressiveIncrementRtpMin = args.Breakdown.Base.Minimum.ToRtpString();
            LinkedProgressiveIncrementRtpMax = args.Breakdown.Base.Minimum.ToRtpString();
            TotalJurisdictionalRtpMin = args.Breakdown.Base.Minimum.ToRtpString();
            TotalJurisdictionalRtpMax = args.Breakdown.Base.Minimum.ToRtpString();
        }

        public string BaseGameRtpMin { get; }

        public string BaseGameRtpMax { get; }

        public string TotalJurisdictionalRtpMin { get; }

        public string TotalJurisdictionalRtpMax { get; }

        public string LinkedProgressiveIncrementRtpMax { get; set; }

        public string LinkedProgressiveIncrementRtpMin { get; set; }

        public string LinkedProgressiveResetRtpMax { get; set; }

        public string LinkedProgressiveResetRtpMin { get; set; }

        public string StandaloneProgressiveIncrementRtpMax { get; set; }

        public string StandaloneProgressiveIncrementRtpMin { get; set; }

        public string StandaloneProgressiveResetRtpMax { get; set; }

        public string StandaloneProgressiveResetRtpMin { get; set; }

        public RtpVerifiedState StandaloneProgressiveResetRtpState { get; }

        public RtpVerifiedState StandaloneProgressiveIncrementRtpState { get; }

        public RtpVerifiedState LinkedProgressiveResetRtpState { get; }

        public RtpVerifiedState LinkedProgressiveIncrementRtpState { get; }

        public class Args
        {
            public RtpBreakdown Breakdown { get; set; }

            public RtpVerifiedState StandaloneProgressiveResetRtpState { get; set; }

            public RtpVerifiedState StandaloneProgressiveIncrementRtpState { get; set; }

            public RtpVerifiedState LinkedProgressiveResetRtpState { get; set; }

            public RtpVerifiedState LinkedProgressiveIncrementRtpState { get; set; }
        }

        public bool? DialogResult => true;
    }
}