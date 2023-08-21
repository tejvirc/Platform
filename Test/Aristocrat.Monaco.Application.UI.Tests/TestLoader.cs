namespace Aristocrat.Monaco.Application.UI.Tests
{
    using Contracts.OperatorMenu;
    using Loaders;

    public class TestLoader : OperatorMenuPageLoader
    {
        public override string PageName => "Test";

        protected override IOperatorMenuPage CreatePage()
        {
            return null;
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return null;
        }
    }
}