namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig.ChangeOptionConfig;
    using ExpressMapper;
    using Monaco.Common.Validation;
    using Options;

    /// <summary>
    ///     Implementation of validator of option config change entries.
    /// </summary>
    public class SetOptionChangeValidateService : ISetOptionChangeValidateService
    {
        private readonly IG2SEgm _egm;
        private readonly IEnumerable<IDeviceOptions> _validators;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetOptionChangeValidateService" /> class.
        /// </summary>
        /// <param name="egm">A G2S Egm.</param>
        /// <param name="validators">List of validators for each device class.</param>
        public SetOptionChangeValidateService(
            IG2SEgm egm,
            IEnumerable<IDeviceOptions> validators)
        {
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public ValidationResult Verify(ClassCommand<optionConfig, setOptionChange> command)
        {
            var errors = new List<ValidationError>();

            var options = command.Command.option.Select(Mapper.Map<option, Option>).ToList();

            foreach (var option in options)
            {
                if (_egm.Devices.All(d => d.Id != option.DeviceId))
                {
                    errors.Add(
                        new ValidationError(
                            nameof(option.DeviceId),
                            "Device with Id = " + option.DeviceId + " was not found."));
                }

                VerifyValues(option, errors);
            }

            return new ValidationResult(errors.Count == 0, errors);
        }

        private void VerifyValues(Option option, List<ValidationError> errors)
        {
            var validator = _validators.FirstOrDefault(v => v.Matches(option.DeviceClass.DeviceClassFromG2SString()));

            var error = validator?.Verify(option);
            if (error != null && error.IsValid == false)
            {
                errors.AddRange(error.Errors);
            }
        }
    }
}