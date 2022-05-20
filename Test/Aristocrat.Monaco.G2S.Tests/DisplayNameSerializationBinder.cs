namespace Aristocrat.Monaco.G2S.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    ///     test test
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.DefaultSerializationBinder" />
    public class DisplayNameSerializationBinder : DefaultSerializationBinder
    {
        /// <summary>
        ///     The name to type
        /// </summary>
        private readonly Dictionary<string, Type> _nameToType;

        /// <summary>
        ///     The type to name
        /// </summary>
        private readonly Dictionary<Type, string> _typeToName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayNameSerializationBinder" /> class.
        /// </summary>
        public DisplayNameSerializationBinder()
        {
            var customDisplayNameTypes = GetType().Assembly

                // concat with references if desired
                .GetTypes().Where(x => x.GetCustomAttributes(false).Any(y => y is DisplayNameAttribute));

            _nameToType =
                customDisplayNameTypes.ToDictionary(
                    t => t.GetCustomAttributes(false).OfType<DisplayNameAttribute>().First().DisplayName,
                    t => t);

            _typeToName = _nameToType.ToDictionary(t => t.Value, t => t.Key);
        }

        /// <summary>
        ///     When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (_typeToName.ContainsKey(serializedType) == false)
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
                return;
            }

            var name = _typeToName[serializedType];

            assemblyName = null;
            typeName = name;
        }

        /// <summary>
        ///     When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
        /// <returns>
        ///     The type of the object the formatter creates a new instance of.
        /// </returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (_nameToType.ContainsKey(typeName))
            {
                return _nameToType[typeName];
            }

            return base.BindToType(assemblyName, typeName);
        }
    }
}