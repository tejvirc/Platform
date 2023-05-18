namespace Stubs
{
    using System.Reflection;
    using log4net;
    using Vgt.Client12.Application.OperatorMenu;
    
    public sealed class StubOperatorMenu : IOperatorMenu
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Show()
        {
            Logger.Debug("Faking Operator Menu launch");
        }

        public void Hide()
        {
            Logger.Debug("Faking Operator Menu hide");
        }

        public void Close()
        {
            Logger.Debug("Faking Operator Menu close");
        }

        public void Activate()
        {
            Logger.Debug("Faking Operator Menu activate");
        }
    }
}
