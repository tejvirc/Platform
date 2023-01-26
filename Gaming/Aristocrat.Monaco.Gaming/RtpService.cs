namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Rtp;
    using Kernel;

    public class RtpService : IService, IRtpService
    {
        public RtpService()
        {
            throw new System.NotImplementedException();
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IRtpService) };

        public GameRtpReport GenerateRtpReportForGame(IGameProfile game) 
        {
            throw new System.NotImplementedException();
        }

        public GameRtpReport GenerateRtpReportForGame(string gameThemeId) 
        {
            // lookup game using gameProvider
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}