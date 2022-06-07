namespace Aristocrat.Monaco.Hardware.Serial.Reel
{
    using System;
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using Contracts.Gds;
    using Contracts.Gds.Reel;
    using Contracts.Reel;
    using Protocols;
    using HomeReel = Contracts.Gds.Reel.HomeReel;
    using Nudge = Contracts.Gds.Reel.Nudge;

    public abstract class SerialReelController : SerialDeviceProtocol
    {
        private readonly ConcurrentDictionary<int, ReelStatus>
            _reelStatus = new ConcurrentDictionary<int, ReelStatus>();

        private FailureStatus _status = new FailureStatus();

        protected SerialReelController(int startingReelId, int maxReelId, IMessageTemplate newDefaultTemplate = null)
            : base(newDefaultTemplate)
        {
            for (var i = startingReelId; i <= maxReelId; ++i)
            {
                _reelStatus.TryAdd(startingReelId, new ReelStatus { ReelId = i });
            }
        }

        protected FailureStatus Status
        {
            get => _status;
            set
            {
                if (_status.Equals(value))
                {
                    return;
                }

                if (OnMessageReceived(value))
                {
                    _status = value;
                }
            }
        }

        protected void SetReelStatus(int reelId, ReelStatus status)
        {
            _reelStatus.AddOrUpdate(
                reelId,
                _ =>
                {
                    OnMessageReceived(status);
                    return status;
                },
                (_, reelStatus) =>
                {
                    if (!status.Equals(reelStatus))
                    {
                        OnMessageReceived(status);
                    }

                    return status;
                });
        }

        protected void SetReelLightsIdentifiers(ReelLightIdentifiersResponse response)
        {
            OnMessageReceived(response);
        }

        protected void UpdateReelStatus(int reelId, Func<ReelStatus, ReelStatus> statusUpdater)
        {
            _reelStatus.AddOrUpdate(
                reelId,
                _ => statusUpdater.Invoke(new ReelStatus { ReelId = reelId }),
                (_, reelStatus) =>
                {
                    var oldStatus = (ReelStatus)reelStatus.Clone();
                    var status = statusUpdater.Invoke(reelStatus);
                    if (!oldStatus.Equals(status))
                    {
                        OnMessageReceived(status);
                    }

                    return status;
                });
        }

        protected abstract void HomeReel(int reelId, int position);

        protected abstract void SetLights(params ReelLampData[] lampData);

        protected abstract void SetReelLightBrightness(int reelId, int brightness);

        protected abstract void SetReelSpeed(params ReelSpeedData[] speedData);

        protected abstract void SetReelOffsets(params int[] offsets);

        protected abstract void SpinReel(params ReelSpinData[] spinData);

        protected abstract void NudgeReel(params NudgeReelData[] nudgeData);

        protected abstract void TiltReels();

        protected abstract void GetReelLightIdentifiers();

        protected override void ProcessMessage(GdsSerializableMessage message, CancellationToken token)
        {
            switch (message)
            {
                case HomeReel homeReel:
                    HomeReel(homeReel.ReelId, homeReel.Stop);
                    break;
                case SetLamps setLamps:
                    var lampData = setLamps.ReelLampData.Select(x =>
                        new ReelLampData(Color.FromArgb(x.RedIntensity, x.GreenIntensity, x.BlueIntensity), x.IsLampOn, x.LampId)).ToArray();

                    SetLights(lampData);
                    break;
                case SetBrightness setBrightness:
                    SetReelLightBrightness(setBrightness.ReelId, setBrightness.Brightness);
                    break;
                case GetReelLightIdentifiers _:
                    GetReelLightIdentifiers();
                    break;
                case SetSpeed setSpeed:
                    var speedData = setSpeed.ReelSpeedData.Select(x =>
                        new ReelSpeedData(x.ReelId, x.Speed)).ToArray();
                    SetReelSpeed(speedData);
                    break;
                case SetOffsets setOffsets:
                    SetReelOffsets(setOffsets.ReelOffsets);
                    break;
                case SpinReels spinReels:
                    var spinData = spinReels.ReelSpinData.Select(x =>
                        new ReelSpinData(x.ReelId, x.Direction, x.Rpm, x.Step)).ToArray();
                    SpinReel(spinData);
                    break;
                case Nudge nudge:
                    var nudgeData = nudge.NudgeReelData.Select(x =>
                        new NudgeReelData(x.ReelId, x.Direction, x.Step, x.Rpm, x.Delay)).ToArray();
                    NudgeReel(nudgeData);
                    break;
                case TiltReels _:
                    TiltReels();
                    break;
                default:
                    base.ProcessMessage(message, token);
                    break;
            }
        }
    }
}