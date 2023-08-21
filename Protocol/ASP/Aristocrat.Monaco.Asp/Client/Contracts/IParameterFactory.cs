namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System;
    using System.Collections.Generic;
    using Devices;

    /// <summary>
    ///     Interface to create asp parameters.
    /// </summary>
    public interface IParameterFactory
    {
        /// <summary>
        ///     Gets the protocol device definition
        /// </summary>
        Devices DeviceDefinition { get; }

        /// <summary>
        ///     Creates a parameter from the given device, type and parameter id.
        /// </summary>
        /// <param name="deviceClass">Device class from which to create parameter.</param>
        /// <param name="deviceType">Device type from which to create parameter.</param>
        /// <param name="parameter">Parameter id to be created.</param>
        /// <returns>IParameter if created null otherwise.</returns>
        IParameter Create(INamedId deviceClass, INamedId deviceType, INamedId parameter);

        /// <summary>
        ///     Creates a parameter from the given device, type and parameter id.
        /// </summary>
        /// <param name="deviceClass">Device class from which to create parameter.</param>
        /// <param name="deviceType">Device type from which to create parameter.</param>
        /// <param name="parameter">Parameter id to be created.</param>
        /// <returns>IParameter if created null otherwise.</returns>
        IParameter Create(int deviceClass, int deviceType, int parameter);

        /// <summary>
        ///     Checks if the given class, type, parameter combination exists in the factory.
        /// </summary>
        /// <param name="deviceClass">Device class to check.</param>
        /// <param name="deviceType">Device type within the device class to check.</param>
        /// <param name="parameter">Parameter id within device type to check.</param>
        /// <returns>
        ///     A tuple with DeviceClassExists set to true if class exists, DeviceTypeExists set to true if type exists,
        ///     ParameterExists set to true if
        ///     parameter exists.
        /// </returns>
        (bool DeviceClassExists, bool DeviceTypeExists, bool ParameterExists) Exists(
            int deviceClass,
            int deviceType,
            int parameter);

        /// <summary>
        ///     Iterates over all parameter prototypes in all device classes and types and selects for given predicate.
        /// </summary>
        /// <param name="predicate">Function that return true if the parameter should be selected.</param>
        /// <returns></returns>
        IList<IParameterPrototype> SelectParameterPrototypes(Func<IParameterPrototype, bool> predicate);
    }
}