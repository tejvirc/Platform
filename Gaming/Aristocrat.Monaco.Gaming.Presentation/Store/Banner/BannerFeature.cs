namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

using Fluxor;

public class BannerFeature : Feature<BannerState>
{
    public override string GetName() => "Banner";

    protected override BannerState GetInitialState()
    {
        return new BannerState();
    }
}