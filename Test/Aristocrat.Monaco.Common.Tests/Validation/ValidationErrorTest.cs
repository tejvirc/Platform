namespace Aristocrat.Monaco.Common.Tests.Validation
{
    using Common.Validation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ValidationErrorTest
    {
        [TestMethod]
        public void WhenConstructWithAllParametersExpectSuccess()
        {
            var propertyName = "PropertyName";
            var message = "message";

            var validationError = new ValidationError(propertyName, message);

            Assert.IsNotNull(validationError);

            Assert.IsFalse(string.IsNullOrWhiteSpace(validationError.PropertyName));
            Assert.IsFalse(string.IsNullOrWhiteSpace(validationError.Message));

            Assert.AreEqual(validationError.PropertyName, propertyName);
            Assert.AreEqual(validationError.Message, message);
        }
    }
}