using Aristocrat.Monaco.Hhr.Client.Mappings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aristocrat.Monaco.Hhr.Tests.Mappings
{
    [TestClass]
    public class MapperProviderTests
    {
        [TestMethod]
        public void ValidateConfiguration()
        {
            _ = new MapperProvider(null).GetMapper();
        }
    }
}
