namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.Diagnostics;
    using Aristocrat.Extensions.CommunityToolkit;
    using Models;

    /// <summary>
    ///     Designer data for the <see cref="HostConfigurationViewModel" />
    /// </summary>
    public partial class HostConfigurationViewModel
    {
        [Conditional("DESIGN")]
        private void WireDesignerData()
        {
            if (Execute.InDesigner)
            {
                Hosts.Add(new Host { Index = 0, Id = 0, Registered = true });
                Hosts.Add(
                    new Host
                    {
                        Index = 1,
                        Id = 1,
                        Registered = true,
                        Address = new Uri("http://localhost:31101/RGS/api-services/G2SAPI"),
                        RequiredForPlay = false,
                    });

                for (var index = 2; index < 8; index++)
                {
                    Hosts.Add(new Host { Index = index, Id = 0, Registered = false });
                }

                _port = Constants.DefaultPort;

                RegisteredHosts.View.Refresh();
            }
        }
    }
}