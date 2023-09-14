namespace Aristocrat.Monaco.Hardware.Audio
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig]
        void GetCount(out uint cProps);

        [PreserveSig]
        void GetAt(uint iProp, out PropKey pkey);

        [PreserveSig]
        void GetValue(ref PropKey key, out PropVariant pvar);
    }
}
