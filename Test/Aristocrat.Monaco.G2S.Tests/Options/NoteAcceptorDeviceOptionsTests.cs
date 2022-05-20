namespace Aristocrat.Monaco.G2S.Tests.Options
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Data.OptionConfig.ChangeOptionConfig;
    using G2S.Handlers.OptionConfig.Builders;
    using G2S.Options;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Validation;
    using Moq;

    [TestClass]
    public class NoteAcceptorDeviceOptionsTests : BaseOptionsTests
    {
        private const int TotalPossibleInvalidValues = 13;

        private NoteAcceptorDeviceOptions _noteAcceptorDeviceOptions;

        [TestInitialize]
        public void Initialize()
        {
            var registry = new Mock<IDeviceRegistryService>();
            var noteAcceptorServiceMock = new Mock<INoteAcceptor>();
            registry.Setup(m => m.GetDevice<INoteAcceptor>()).Returns(noteAcceptorServiceMock.Object);
            var persistence = new Mock<IPersistenceProvider>();
            var property = new Mock<IPropertiesManager>();

            noteAcceptorServiceMock.Setup(m => m.GetSupportedNotes(It.IsAny<string>()))
                .Returns(
                    new Collection<int>()
                    {
                        1,
                        5,
                        10,
                        20,
                        50,
                        100
                    });

            persistence.Setup(a => a.GetOrCreateBlock(It.IsAny<string>(), It.IsAny<PersistenceLevel>())).Returns(new Mock<IPersistentBlock>().Object);

            _noteAcceptorDeviceOptions = new NoteAcceptorDeviceOptions(registry.Object, persistence.Object, property.Object);
        }

        [TestMethod]
        public void TestCheckValidParameters()
        {
            var options = CreateOptionWithAllValidParameters();
            var errors = new List<ValidationError>();

            foreach (var option in options)
            {
                var validationErrors = _noteAcceptorDeviceOptions.Verify(option);
                errors.AddRange(validationErrors.Errors);
            }

            var totalValidationErrors = new ValidationResult(errors.Count == 0, errors);
            Assert.IsTrue(totalValidationErrors.IsValid);
            Assert.AreEqual(totalValidationErrors.Errors.Count(), 0);
        }

        [TestMethod]
        public void TestCheckInvalidParameters()
        {
            var options = CreateOptionWithAllInvalidParameters();
            var errors = new List<ValidationError>();

            foreach (var option in options)
            {
                var validationErrors = _noteAcceptorDeviceOptions.Verify(option);
                errors.AddRange(validationErrors.Errors);
            }

            var totalValidationErrors = new ValidationResult(errors.Count == 0, errors);
            Assert.IsFalse(totalValidationErrors.IsValid);
            Assert.AreEqual(totalValidationErrors.Errors.Count(), TotalPossibleInvalidValues);
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.ConfigurationIdParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(x => x.PropertyName == G2SParametersNames.RestartStatusParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.UseDefaultConfigParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.RequiredForPlayParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.NoteAcceptor.NoteEnabledParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.NoteAcceptor.VoucherEnabledParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.NoteAcceptor.DenomIdParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.NoteAcceptor.BaseCashableAmountParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.NoteAcceptor.NoteActiveParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.NoteAcceptor.TokenParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.NoteAcceptor.BasePromoAmountParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.NoteAcceptor.BaseNonCashAmountParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.ConfigCompleteParameterName));
        }

        protected override void AddAdditionalG2SProtocolParameters(
            List<OptionCurrentValue> parent,
            bool isValid)
        {
            CreateParameter(
                parent,
                G2SParametersNames.RestartStatusParameterName,
                OptionConfigParameterType.Boolean,
                isValid);
            CreateParameter(
                parent,
                G2SParametersNames.UseDefaultConfigParameterName,
                OptionConfigParameterType.Boolean,
                isValid);
            CreateParameter(
                parent,
                G2SParametersNames.RequiredForPlayParameterName,
                OptionConfigParameterType.Boolean,
                isValid);
        }

        private IEnumerable<Option> CreateOptionWithAllValidParameters()
        {
            return new List<Option>
            {
                CreateG2SProtocolParameters(true),
                CreateG2Sv3ProtocolParameters(true),
                CreateNoteAcceptorOptionsParameters(true),
                CreateNoteAcceptorDataTableOptionsParameters(true)
            };
        }

        private IEnumerable<Option> CreateOptionWithAllInvalidParameters()
        {
            return new List<Option>
            {
                CreateG2SProtocolParameters(false),
                CreateG2Sv3ProtocolParameters(false),
                CreateNoteAcceptorOptionsParameters(false),
                CreateNoteAcceptorDataTableOptionsParameters(false)
            };
        }

        private Option CreateNoteAcceptorOptionsParameters(bool isValid)
        {
            return CreateOptionWithParameters(
                OptionConstants.NoteAcceptorOptionsId,
                isValid,
                list =>
                {
                    CreateParameter(
                        list,
                        G2SParametersNames.NoteAcceptor.NoteEnabledParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.NoteAcceptor.VoucherEnabledParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                });
        }

        private Option CreateNoteAcceptorDataTableOptionsParameters(bool isValid)
        {
            return CreateOptionWithParameters(
                OptionConstants.NoteAcceptorDataTable,
                isValid,
                list =>
                {
                    CreateParameter(
                        list,
                        G2SParametersNames.CurrencyIdParameterName,
                        OptionConfigParameterType.String,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.NoteAcceptor.DenomIdParameterName,
                        OptionConfigParameterType.Integer,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.NoteAcceptor.BaseCashableAmountParameterName,
                        OptionConfigParameterType.Integer,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.NoteAcceptor.NoteActiveParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.NoteAcceptor.TokenParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.NoteAcceptor.BasePromoAmountParameterName,
                        OptionConfigParameterType.Integer,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.NoteAcceptor.BaseNonCashAmountParameterName,
                        OptionConfigParameterType.Integer,
                        isValid);
                });
        }
    }
}
