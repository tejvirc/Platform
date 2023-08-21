namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Xml.Serialization;
    using log4net;

    /// <summary>
    ///     A method which converts a byte array to an object of some type.
    /// </summary>
    /// <param name="value"> The byte array to convert. </param>
    /// <returns> The resulting object. </returns>
    public delegate object FieldConverter(byte[] value);

    /// <summary>
    ///     The block format class stores the format of the block in a series of
    ///     Field description classes stored in a list.
    /// </summary>
    [XmlRoot("BlockFormat")]
    public sealed class BlockFormat
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly UTF8Encoding Encoder = new UTF8Encoding();

        private static readonly Dictionary<FieldType, FieldConverter> FieldConverters =
            new Dictionary<FieldType, FieldConverter>
            {
                { FieldType.Int16, Persistence.FieldConverters.ShortConvert },
                { FieldType.Int32, Persistence.FieldConverters.IntConvert },
                { FieldType.Int64, Persistence.FieldConverters.LongConvert },
                { FieldType.Bool, Persistence.FieldConverters.BoolConvert },
                { FieldType.Byte, Persistence.FieldConverters.ByteConvert },
                { FieldType.DateTime, Persistence.FieldConverters.DateTimeConvert },
                { FieldType.String, Persistence.FieldConverters.StringConvert },
                { FieldType.Guid, Persistence.FieldConverters.GuidConvert },
                { FieldType.UInt16, Persistence.FieldConverters.UInt16Convert },
                { FieldType.UInt32, Persistence.FieldConverters.UInt32Convert },
                { FieldType.UInt64, Persistence.FieldConverters.UInt64Convert },
                { FieldType.TimeSpan, Persistence.FieldConverters.TimeSpanConvert },
                { FieldType.UnboundedString, Persistence.FieldConverters.StringConvert },
                { FieldType.Float, Persistence.FieldConverters.FloatConvert },
                { FieldType.Double, Persistence.FieldConverters.DoubleConvert }
            };

        private bool _finalizedLayout;

        private Dictionary<string, int> _namedFieldDescriptionIndices = new Dictionary<string, int>();

        /// <summary>
        ///     Gets the field ...
        /// </summary>
        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<FieldDescription> FieldDescriptions { get; } = new List<FieldDescription>();

        /// <summary>
        ///     Gets or sets the Name property.
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the ElementSize property.
        /// </summary>
        [XmlAttribute]
        public int ElementSize { get; set; }

        /// <summary>
        ///     Gets or sets the Name property.
        /// </summary>
        [XmlAttribute(AttributeName = "version")]
        public int Version { get; set; }

        /// <summary>
        ///     When reading in from an XML file, the dictionary is not automatically made.
        ///     This routine updates the dictionary.
        /// </summary>
        public void UpdateDictionary()
        {
            _namedFieldDescriptionIndices = new Dictionary<string, int>();
            FinalizeLayout();

            // things can get screwy if some of the fields have offsets and others don't
            for (var i = 0; i < FieldDescriptions.Count; ++i)
            {
                _namedFieldDescriptionIndices.Add(FieldDescriptions[i].FieldName, i);
            }
        }

        /// <summary>
        ///     go through each field element and find out where it needs to be located, if it has
        ///     a dynamic location
        /// </summary>
        public void FinalizeLayout()
        {
            if (!_finalizedLayout)
            {
                var placed = new List<FieldDescription>();
                var unplaced = new List<FieldDescription>();

                foreach (var fd in FieldDescriptions)
                {
                    if (fd.DefaultOffset)
                    {
                        unplaced.Add(fd);
                    }
                    else
                    {
                        placed.Add(fd);
                    }

                    if (fd.Size == 0)
                    {
                        fd.Size = fd.DataType == FieldType.String ? 1024 : FieldDescription.StandardFieldLength[(int)fd.DataType];
                    }
                }

                placed.Sort(CompareOffset);
                unplaced.Sort(CompareSize);

                var offsetAt = 0;
                var placedIndex = 0;
                while (unplaced.Count > 0)
                {
                    // if we have more on the placed list to go through
                    if (placedIndex < placed.Count)
                    {
                        // if there is space between the current location and the next on the placed list
                        if (offsetAt < placed[placedIndex].Offset)
                        {
                            var spaceAvailable = placed[placedIndex].Offset - offsetAt;

                            // go through the unplaced list to find all that will fit
                            for (var j = unplaced.Count - 1; j >= 0; --j)
                            {
                                // if this one will fit
                                if (spaceAvailable >= unplaced[j].Length)
                                {
                                    unplaced[j].Offset = offsetAt;
                                    offsetAt += unplaced[j].Length;
                                    spaceAvailable -= unplaced[j].Length;
                                    unplaced.RemoveAt(j);
                                }
                            }
                        }

                        // once we have placed everything we can before the fixed position field,
                        //  set the offset to the end of the fixed position field.
                        offsetAt = placed[placedIndex].Offset + placed[placedIndex].Length;
                    }
                    else
                    {
                        // there are no more on the placed list, just add the rest in order
                        //  that the user specified when they added them
                        foreach (var fd in FieldDescriptions)
                        {
                            if (unplaced.Contains(fd))
                            {
                                fd.Offset = offsetAt;
                                offsetAt += fd.Length;
                            }
                        }

                        unplaced.Clear();
                    }

                    ++placedIndex;
                }

                _finalizedLayout = true;
            }
        }

        /// <summary>
        ///     Adds a field description.
        /// </summary>
        /// <param name="fieldDescription">The field description to add to the list.</param>
        public void AddFieldDescription(FieldDescription fieldDescription)
        {
            // count of 0 has 1 element, but its not an array
            FieldDescriptions.Add(fieldDescription);
            _namedFieldDescriptionIndices.Add(fieldDescription.FieldName, FieldDescriptions.Count - 1);
            _finalizedLayout = false;
        }

        /// <summary>
        ///     Retrieves the FieldType of a field
        /// </summary>
        /// <param name="fieldName">The field name for which a FieldType is desired</param>
        /// <returns>The FieldType value</returns>
        public FieldType GetFieldType(string fieldName)
        {
            var returnValue = FieldType.Byte;
            var fd = GetFieldDescription(fieldName);
            if (fd != null)
            {
                returnValue = fd.DataType;
            }

            return returnValue;
        }

        /// <summary>
        ///     Retrieves the FieldDescription of a field
        /// </summary>
        /// <param name="fieldName">The field name for which a FieldType is desired</param>
        /// <returns>The FieldDescription value</returns>
        public FieldDescription GetFieldDescription(string fieldName)
        {
            return _namedFieldDescriptionIndices.TryGetValue(fieldName, out var index)
                ? FieldDescriptions[index]
                : null;
        }

        /// <summary>
        ///     Converts something to something else.  A byte array to an appropriate object.
        /// </summary>
        /// <remarks>
        ///     The summary is appropriate: The field name is used to get an index from the namedFieldDescriptionIndices
        ///     dictionary.
        ///     The index is used to get a FieldDescription from the _fieldDescriptions list.
        ///     If that lookup returns an object with a count of zero, the appropriate field converter will convert the array to an
        ///     object for return.
        ///     Otherwise, the array is converted to an array of field descriptors and returned as an object.
        /// </remarks>
        /// <param name="fieldName">The field name for which conversion is desired.</param>
        /// <param name="array">A byte array to hold the conversion data.</param>
        /// <returns>An object.</returns>
        public object Convert(string fieldName, byte[] array)
        {
            if (!_finalizedLayout)
            {
                var message = "Block Layout not finalized, call FinalizeLayout() before use";
                Logger.Fatal(message);
                throw new ArgumentException(message);
            }

            if (array == null)
            {
                Logger.Warn($"{MethodBase.GetCurrentMethod().Name} {fieldName} attempting to convert null bytes.");
            }

            object returnObject = null;

            var fd = GetFieldDescription(fieldName);
            if (fd != null)
            {
                if (fd.Count == 0)
                {
                    returnObject = FieldConverters[fd.DataType](array);
                }
                else
                {
                    returnObject = Persistence.FieldConverters.GetConvertedArray(
                        array,
                        fd,
                        FieldConverters[fd.DataType]);
                }
            }

            return returnObject;
        }

        /// <summary>
        ///     Converts stuff to other stuff.  Specifically, an object to a byte array.
        /// </summary>
        /// <remarks>
        ///     Uses the fieldName parameter to get a byte conversion method.
        /// </remarks>
        /// <param name="fieldName">The field to convert.</param>
        /// <param name="value">The value to use.</param>
        /// <returns>The byte array representation.</returns>
        public byte[] ConvertTo(string fieldName, object value)
        {
            if (!_finalizedLayout)
            {
                Logger.Fatal("Block Layout not finalized, call FinalizeLayout() before use");
                throw new ArgumentException("Block Layout not finalized, call FinalizeLayout() before use");
            }

            byte[] returnValue = { };

            if (value == null)
            {
                return returnValue;
            }

            var fd = GetFieldDescription(fieldName);
            if (fd == null)
            {
                Logger.Warn($"{MethodBase.GetCurrentMethod().Name} FieldDescription for {fieldName} not found.");
                return returnValue;
            }

            byte[] tempValue = null;

            if (fd.Count == 0)
            {
                tempValue = GetBytes(fd.DataType, value);

                // A block with single field could have unbounded length (fd.Size == -1)
                if (fd.Size > 0)
                {
                    returnValue = new byte[fd.Size];
                    var returnSize = tempValue.Length;

                    if (returnSize > fd.Size)
                    {
                        returnSize = fd.Size;
                    }

                    Buffer.BlockCopy(tempValue, 0, returnValue, 0, returnSize);
                }
                else
                {
                    returnValue = tempValue;
                }
            }
            else
            {
                var returnArray = new byte[fd.Length];
                var breakLoopForBytes =
                    false; // If we have a byte array, we do not need to go through every element in the array

                for (var i = 0; i < fd.Count; ++i)
                {
                    switch (fd.DataType)
                    {
                        case FieldType.String:
                            tempValue = Encoder.GetBytes(((string[])value)[i]);
                            break;
                        case FieldType.DateTime:
                            tempValue = BitConverter.GetBytes(((DateTime[])value)[i].ToBinary());
                            break;
                        case FieldType.TimeSpan:
                            tempValue = BitConverter.GetBytes(((TimeSpan[])value)[i].TotalMilliseconds);
                            break;
                        case FieldType.Guid:
                            tempValue = ((Guid[])value)[i].ToByteArray();
                            break;

                        // No need to convert each byte in an array of bytes
                        // Set the return value and exit loop
                        case FieldType.Byte:
                            returnArray = (byte[])value;
                            breakLoopForBytes = true;
                            break;
                        default:
                            tempValue = GetBytes(fd.DataType, ((Array)value).GetValue(i));
                            break;
                    }

                    if (breakLoopForBytes)
                    {
                        break;
                    }

                    // we only want to return the number of bytes specified in the format
                    returnValue = new byte[fd.Size];
                    var returnSize = tempValue.Length;
                    if (returnSize > fd.Size)
                    {
                        returnSize = fd.Size;
                    }

                    // I use two block copies here, the first is needed in case the tempValue is
                    // smaller than the format size.  In that case, extra zeroes in the high order
                    // bytes will be copied.
                    Buffer.BlockCopy(tempValue, 0, returnValue, 0, returnSize);
                    Buffer.BlockCopy(returnValue, 0, returnArray, i * fd.Size, fd.Size);
                }

                returnValue = returnArray;
            }

            return returnValue;
        }

        private static int CompareOffset(FieldDescription a, FieldDescription b)
        {
            return a.Offset - b.Offset;
        }

        private static int CompareSize(FieldDescription a, FieldDescription b)
        {
            return a.Length - b.Length;
        }

        private static byte[] GetBytes(FieldType type, object value)
        {
            byte[] result = null;

            switch (type)
            {
                case FieldType.String:
                case FieldType.UnboundedString:
                    result = Encoder.GetBytes((string)value);
                    break;
                case FieldType.DateTime:
                    result = BitConverter.GetBytes(((DateTime)value).ToBinary());
                    break;
                case FieldType.TimeSpan:
                    result = BitConverter.GetBytes(((TimeSpan)value).TotalMilliseconds);
                    break;
                case FieldType.Guid:
                    result = ((Guid)value).ToByteArray();
                    break;
                case FieldType.Byte:
                    result = new [] { (byte)value };
                    break;
                case FieldType.Int32:
                    result = BitConverter.GetBytes((int)value);
                    break;
                case FieldType.Int16:
                    result = BitConverter.GetBytes((short)value);
                    break;
                case FieldType.Int64:
                    result = BitConverter.GetBytes((long)value);
                    break;
                case FieldType.Bool:
                    result = BitConverter.GetBytes((bool)value);
                    break;
                case FieldType.UInt16:
                    result = BitConverter.GetBytes((ushort)value);
                    break;
                case FieldType.UInt32:
                    result = BitConverter.GetBytes((uint)value);
                    break;
                case FieldType.UInt64:
                    result = BitConverter.GetBytes((ulong)value);
                    break;
                case FieldType.Float:
                    result = BitConverter.GetBytes((float)value);
                    break;
                case FieldType.Double:
                    result = BitConverter.GetBytes((double)value);
                    break;
                case FieldType.Unused:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }
    }
}
