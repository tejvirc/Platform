using System;
using System.Runtime.InteropServices;

namespace Aristocrat.Monaco.Hardware.Audio
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct PropVariant
    {
        [FieldOffset(0)]
        public short _vt;

        [FieldOffset(2)]
        public short _wReserved1;

        [FieldOffset(4)]
        public short _wReserved2;

        [FieldOffset(6)]
        public short _wReserved3;

        [FieldOffset(8)]
        public sbyte _cVal;

        [FieldOffset(8)]
        public byte _bVal;

        [FieldOffset(8)]
        public short _iVal;

        [FieldOffset(8)]
        public ushort _uiVal;

        [FieldOffset(8)]
        public int _lVal;

        [FieldOffset(8)]
        public uint _ulVal;

        [FieldOffset(8)]
        public int _intVal;

        [FieldOffset(8)]
        public uint _uintVal;

        [FieldOffset(8)]
        public long _hVal;

        [FieldOffset(8)]
        public long _uhVal;

        [FieldOffset(8)]
        public float _fltVal;

        [FieldOffset(8)]
        public double _dblVal;

        [FieldOffset(8)]
        public short _boolVal;

        [FieldOffset(8)]
        public int _scode;

        [FieldOffset(8)]
        public System.Runtime.InteropServices.ComTypes.FILETIME _filetime;

        [FieldOffset(8)]
        public byte _blobVal;

        [FieldOffset(8)]
        public IntPtr _ptrVal;

        public VarEnum DataType => (VarEnum)_vt;

        /// <summary>
        ///     Retrieve the value from the PropVariant
        /// </summary>
        public object Value
        {
            get
            {
                // Only supports LPSTR right now
                if (DataType != VarEnum.VT_LPWSTR)
                {
                    throw new NotImplementedException();
                }

                return Marshal.PtrToStringUni(_ptrVal);
            }
        }

        /// <summary>
        ///     Clear the PropVariant. Should be called once done using the PropVariant.
        /// </summary>
        public void Clear()
        {
            PropVariantClear(ref this);
        }

        [PreserveSig]
        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant pvar);
    }
}