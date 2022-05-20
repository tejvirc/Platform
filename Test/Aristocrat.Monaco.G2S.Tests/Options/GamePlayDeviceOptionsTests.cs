namespace Aristocrat.Monaco.G2S.Tests.Options
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Data.OptionConfig.ChangeOptionConfig;
    using G2S.Handlers.OptionConfig.Builders;
    using G2S.Options;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Validation;
    using Moq;

    [TestClass]
    public class GamePlayDeviceOptionsTests : BaseOptionsTests
    {
        private const int TotalPossibleInvalidValues = 15;

        private readonly Mock<IGameProvider> _gameProviderMock = new Mock<IGameProvider>();

        private GamePlayDeviceOptions _gamePlayDeviceOptions;

        [TestInitialize]
        public void TestInitialize()
        {
            _gamePlayDeviceOptions = new GamePlayDeviceOptions(_gameProviderMock.Object);
        }

        [TestMethod]
        public void TestCheckValidParameters()
        {
            var options = CreateOptionWithAllValidParameters();
            var errors = new List<ValidationError>();

            foreach (var option in options)
            {
                var validationErrors = _gamePlayDeviceOptions.Verify(option);
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
                var validationErrors = _gamePlayDeviceOptions.Verify(option);
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
                    x => x.PropertyName == G2SParametersNames.ConfigCompleteParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.GamePlayDevice.MaxWagerCreditsParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.GamePlayDevice.ProgAllowedParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.GamePlayDevice.SecondaryAllowedParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.GamePlayDevice.CentralAllowedParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.GamePlayDevice.StandardPlayParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.GamePlayDevice.TournamentPlayParameterName));
            Assert.IsTrue(totalValidationErrors.Errors.Any(x => x.PropertyName == OptionConstants.GameAccessOptionsId));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(x => x.PropertyName == OptionConstants.GameAccessibleOptionsId));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.GamePlayDevice.DenomIdParameterName));
            Assert.IsTrue(
                totalValidationErrors.Errors.Any(
                    x => x.PropertyName == G2SParametersNames.GamePlayDevice.DenomActiveParameterName));
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

        private List<Option> CreateOptionWithAllValidParameters()
        {
            var result = new List<Option>();

            result.Add(CreateG2SProtocolParameters(true));
            result.Add(CreateG2Sv3ProtocolParameters(true));
            result.Add(CreateGamePlayParameters(true));
            result.Add(CreateGameTypeParameters(true));
            result.Add(CreateGameAccessParameters(true));
            result.Add(CreateGameAccessibleParameters(true));
            result.Add(CreateDenomListParameters(true));

            return result;
        }

        private List<Option> CreateOptionWithAllInvalidParameters()
        {
            var result = new List<Option>();

            result.Add(CreateG2SProtocolParameters(false));
            result.Add(CreateG2Sv3ProtocolParameters(false));
            result.Add(CreateGamePlayParameters(false));
            result.Add(CreateGameTypeParameters(false));
            result.Add(CreateGameAccessParameters(false));
            result.Add(CreateGameAccessibleParameters(false));
            result.Add(CreateDenomListParameters(false));

            return result;
        }

        private Option CreateGamePlayParameters(bool isValid)
        {
            return CreateOptionWithParameters(
                OptionConstants.GamePlayOptionsId,
                isValid,
                list =>
                {
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.ThemeIdParameterName,
                        OptionConfigParameterType.String,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.PaytableIdParameterName,
                        OptionConfigParameterType.String,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.MaxWagerCreditsParameterName,
                        OptionConfigParameterType.Integer,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.ProgAllowedParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.SecondaryAllowedParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.CentralAllowedParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                });
        }

        private Option CreateGameTypeParameters(bool isValid)
        {
            return CreateOptionWithParameters(
                OptionConstants.GameTypeOptionsId,
                isValid,
                list =>
                {
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.StandardPlayParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.TournamentPlayParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                });
        }

        private Option CreateGameAccessParameters(bool isValid)
        {
            return CreateOptionWithParameters(
                OptionConstants.GameAccessOptionsId,
                isValid,
                list =>
                {
                    CreateParameter(
                        list,
                        OptionConstants.GameAccessOptionsId,
                        OptionConfigParameterType.Boolean,
                        isValid);
                });
        }

        private Option CreateGameAccessibleParameters(bool isValid)
        {
            return CreateOptionWithParameters(
                OptionConstants.GameAccessibleOptionsId,
                isValid,
                list =>
                {
                    CreateParameter(
                        list,
                        OptionConstants.GameAccessibleOptionsId,
                        OptionConfigParameterType.Boolean,
                        isValid);
                });
        }

        private Option CreateDenomListParameters(bool isValid)
        {
            return CreateOptionWithParameters(
                OptionConstants.GameDenomListOptionsId,
                isValid,
                list =>
                {
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.DenomIdParameterName,
                        OptionConfigParameterType.Integer,
                        isValid);
                    CreateParameter(
                        list,
                        G2SParametersNames.GamePlayDevice.DenomActiveParameterName,
                        OptionConfigParameterType.Boolean,
                        isValid);
                });
        }
    }
}