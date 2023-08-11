namespace Aristocrat.Monaco.Application.UI.Tests
{
    using Contracts.OperatorMenu;
    using Loaders;

    public class TestLoader : OperatorMenuPageLoader
    {
        private string _pageName;
        public override string PageName => _pageName;

        public TestLoader()
        {
            _pageName = "Test";
        }
        public TestLoader(string pname)
        {
            _pageName = pname;
        }

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