namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using Aristocrat.Extensions.CommunityToolkit;
    using System.Diagnostics;

    public partial class EditHostViewModel
    {
        [Conditional("DESIGN")]
        private void WireDesignerData()
        {
            if (Execute.InDesigner)
            {
                HostId = 7;
                Address = "http://localhost:31101/RGS/api-services/G2SAPI";
                Registered = true;
                RequiredForPlay = false;
            }
        }
    }
}