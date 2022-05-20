namespace Aristocrat.Monaco.Common.Tests.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.Validation;
    using FluentValidation.Results;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ValidationResult = Common.Validation.ValidationResult;

    [TestClass]
    public class ValidationResultTest
    {
        [TestMethod]
        public void WhenConstructWithAllParametersExpectSuccess()
        {
            var errors = new List<ValidationError>
            {
                new ValidationError("PropertyName", "Message")
            };

            var badValidationResult = new ValidationResult(false, errors);

            Assert.IsNotNull(badValidationResult);

            Assert.IsFalse(badValidationResult.IsValid);
            Assert.AreEqual(badValidationResult.Errors.Count(), 1);
            Assert.IsNotNull(
                badValidationResult.Errors.SingleOrDefault(
                    x => x.PropertyName == "PropertyName" && x.Message == "Message"));

            var goodValidationResult = new ValidationResult(true, null);

            Assert.IsNotNull(goodValidationResult);

            Assert.IsTrue(goodValidationResult.IsValid);
            Assert.IsNull(goodValidationResult.Errors);
        }

        [TestMethod]
        public void WhenConvertFluentToCommanValidationResultExpectSuccess()
        {
            var propertyName = "PropertyName";
            var errorMessage = "errorMessage";

            var fluentValidationResult =
                new FluentValidation.Results.ValidationResult(
                    new List<ValidationFailure> { new ValidationFailure(propertyName, errorMessage) });

            var commonValidationResult = fluentValidationResult.ConvertToCommonValidationResult();

            Assert.IsNotNull(commonValidationResult);
            Assert.IsFalse(commonValidationResult.IsValid);
            Assert.AreEqual(commonValidationResult.Errors.Count(), 1);
            Assert.IsNotNull(
                commonValidationResult.Errors.SingleOrDefault(
                    x => x.PropertyName == propertyName && x.Message == errorMessage));
        }
    }
}