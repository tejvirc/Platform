namespace Aristocrat.Monaco.TestProtocol
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the TestCurrencyValidator class.
    /// </summary>
    public class TestCurrencyValidator : ICurrencyValidator, IService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public bool HostOnline => true;


        public string Name => typeof(ICurrencyValidator).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ICurrencyValidator) };

        public void Initialize()
        {
        }
        
        /// <inheritdoc />
        public Task<CurrencyInExceptionCode> ValidateNote(int note)
        {
            Log.Info($"Validating note {note}");

            SimulateHostDelay();

            return Task.FromResult(CurrencyInExceptionCode.None);
        }

        /// <inheritdoc />
        public Task<bool> StackedNote(int note)
        {
            Log.Info($"Stacked note {note}");
            
            return Task.FromResult(true);
        }

        private static async void SimulateHostDelay()
        {
            await Task.Delay(500);
        }
    }
}