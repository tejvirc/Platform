namespace Aristocrat.Monaco.Gaming.UI.Factories
{
    using System;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.MeterPage;
    using Application.UI.MeterPage;
    using Contracts.Meters;
    using Contracts.Progressives;

    public static class ProgressiveDisplayMeterFactory
    {

        public static DisplayMeter Build(
            this IProgressiveMeterManager progressiveManager,
            IViewableProgressiveLevel progressiveLevel,
            MeterNode meterNode,
            bool showLifetime,
            long denomMillicents,
            long sharedHiddenTotal)
        {
            switch (meterNode.Name)
            {
                case ProgressiveMeters.ProgressivePackNameDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => prog.ProgressivePackName,
                        meterNode.Order);
                case ProgressiveMeters.LevelNameDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => prog.LevelName,
                        meterNode.Order);
                case ProgressiveMeters.CurrentValueDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => prog.CurrentValue.MillicentsToDollarsNoFraction().FormattedCurrencyString(),
                        meterNode.Order);
                case ProgressiveMeters.OverflowDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => prog.Overflow.MillicentsToDollarsNoFraction().FormattedCurrencyString(),
                        meterNode.Order);
                case ProgressiveMeters.OverflowTotalDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => prog.OverflowTotal.MillicentsToDollarsNoFraction().FormattedCurrencyString(),
                        meterNode.Order);
                case ProgressiveMeters.CeilingDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => prog.MaximumValue.MillicentsToDollarsNoFraction().FormattedCurrencyString(),
                        meterNode.Order);
                case ProgressiveMeters.HiddenIncrementDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => $"{prog.HiddenIncrementRate.ToPercentage()}%",
                        meterNode.Order);
                case ProgressiveMeters.HiddenValueDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => prog.HiddenValue.MillicentsToDollarsNoFraction().FormattedCurrencyString(),
                        meterNode.Order);
                case ProgressiveMeters.ProgressiveLevelHiddenTotal:
                    if(sharedHiddenTotal > 0)
                    {
                        return new ProxyDisplayMeter<long>(
                            meterNode.DisplayName,
                            sharedHiddenTotal,
                            v => v.MillicentsToDollarsNoFraction().FormattedCurrencyString(),
                            meterNode.Order);
                    }

                    return CreateValueDisplayMeter(
                        progressiveManager,
                        progressiveLevel,
                        meterNode,
                        showLifetime);
                case ProgressiveMeters.InitialValueDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => prog.InitialValue.MillicentsToDollarsNoFraction().FormattedCurrencyString(),
                        meterNode.Order);
                case ProgressiveMeters.IncrementDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => $"{prog.IncrementRate.ToPercentage()}%",
                        meterNode.Order);
                case ProgressiveMeters.StartupDisplayMeter:
                    return new ProxyDisplayMeter<IViewableProgressiveLevel>(
                        meterNode.DisplayName,
                        progressiveLevel,
                        prog => prog.ResetValue.MillicentsToDollarsNoFraction().FormattedCurrencyString(),
                        meterNode.Order);
                case ProgressiveMeters.WagerBetLevelsDisplayMeter:
                    return new ProxyDisplayMeter<long>(
                        meterNode.DisplayName,
                        progressiveLevel.CreationType != LevelCreationType.Default ? progressiveLevel.WagerCredits * denomMillicents : 0,
                        val => val.MillicentsToDollars().FormattedCurrencyString(),
                        meterNode.Order);
                case ProgressiveMeters.WageredAmount:
                case ProgressiveMeters.PlayedCount:
                case ProgressiveMeters.ProgressiveLevelBulkContribution:
                case ProgressiveMeters.ProgressiveLevelWinAccumulation:
                    return CreateValueDisplayMeter(
                        progressiveManager,
                        progressiveLevel,
                        meterNode,
                        showLifetime);
                case ProgressiveMeters.ProgressiveLevelWinOccurrence:
                    return CreateDisplayMeter(
                        progressiveManager,
                        progressiveLevel,
                        meterNode,
                        showLifetime);
                default:
                    throw new ArgumentException(
                        $"GetProgressiveProgressiveMeter called with invalid meterNode.Name {meterNode.Name}");
            }
        }

        private static IMeter GetIMeter(IProgressiveMeterManager progressiveManager, int deviceId, int levelId, string meterName)
        {
            return progressiveManager.IsMeterProvided(deviceId, levelId, meterName)
                ? progressiveManager.GetMeter(deviceId, levelId, meterName)
                : null;
        }

        private static DisplayMeter CreateValueDisplayMeter(
            IProgressiveMeterManager progressiveManager,
            IViewableProgressiveLevel progressiveLevel,
            MeterNode meterNode,
            bool showLifetime)
        {
            var progressiveMeter = GetIMeter(
                progressiveManager,
                progressiveLevel.DeviceId,
                progressiveLevel.LevelId,
                meterNode.Name);

            if (progressiveMeter != null)
            {
                return new ValueDisplayMeter(
                    meterNode.DisplayName,
                    progressiveMeter,
                    showLifetime,
                    meterNode.Order);
            }

            return new ProxyDisplayMeter<long>(
                meterNode.DisplayName,
                0,
                v => v.FormattedCurrencyString(),
                meterNode.Order);
        }

        private static DisplayMeter CreateDisplayMeter(
            IProgressiveMeterManager progressiveManager,
            IViewableProgressiveLevel progressiveLevel,
            MeterNode meterNode,
            bool showLifetime)
        {
            var progressiveMeter = GetIMeter(
                progressiveManager,
                progressiveLevel.DeviceId,
                progressiveLevel.LevelId,
                meterNode.Name);
            if (progressiveMeter != null)
            {
                return new DisplayMeter(
                    meterNode.DisplayName,
                    progressiveMeter,
                    showLifetime,
                    meterNode.Order);
            }

            return new ProxyDisplayMeter<long>(
                meterNode.DisplayName,
                0,
                v => v.ToString(),
                meterNode.Order);
        }
    }
}