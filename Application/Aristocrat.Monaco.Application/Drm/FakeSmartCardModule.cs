namespace Aristocrat.Monaco.Application.Drm
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using log4net;
    using Newtonsoft.Json;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Security;
    using SmartCard;
    using static Aristocrat.Monaco.Application.Drm.SmartCardModule;

    internal class FakeSmartCardModule : IProtectionModule
    {
        private const string DeveloperId = "DEVELOPER";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FakeSmartCardModule()
        {
        }

        public IEnumerable<IToken> Tokens { get; private set; }

        public bool IsDeveloper { get; private set; }

        public void Dispose()
        {
            Dispose(true);
        }

        public Task Initialize()
        {
            Tokens = GetTokens();
            tokens[0].InternalCounters.Add(Counter.LicenseCount, 2);

            IsDeveloper = Tokens.All(l => l.Name == DeveloperId);
            return Task.CompletedTask;
        }

        public bool IsConnected()
        {
            return true;
        }

        public bool DecrementCounter(IToken token, Counter counter, int value)
        {
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private Token[] tokens = new Token[] {
            new Token{ Id= 1, Name="Token1" },
        };


        private IEnumerable<IToken> GetTokens()
        {
            return tokens;
        }
    }
}