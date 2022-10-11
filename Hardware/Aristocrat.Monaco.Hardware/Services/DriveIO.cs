namespace Aristocrat
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class DriveIO : Stream
    {
        private const long SectorSize = 512;

        private readonly byte[] _sectorBuffer = new byte[SectorSize];
        private readonly bool _readOnly;

        private long _sectorSeek;

        private DriveIO(IntPtr driveHandle, int driveId, string path, bool readOnly = true)
        {
            _readOnly = readOnly;
            DriveHandle = driveHandle;
            DriveId = driveId;
            Path = path;
            IsDisposed = false;
            IsDisposed = false;

            _sectorSeek = 0;
        }

        public bool IsDisposed { get; private set; }

        public IntPtr DriveHandle { get; }
        public int DriveId { get; }

        public string Path { get; }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => !_readOnly;

        public override long Length
        {
            get
            {
                var current = CurPosition();
                var size = SeekEnd();
                SeekTo(current);
                return size;
            }
        }

        public override long Position
        {
            get => CurPosition() - _sectorSeek;

            set => SeekTo(value);
        }

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static DriveIO OpenDisk(int diskId, bool readOnly = true)
        {
            var path = $"\\\\.\\PhysicalDrive{diskId}";

            var hDrive = NativeMethods.CreateFile(
                path,
                NativeMethods.GenericRead | (readOnly ? 0 : NativeMethods.GenericWrite),
                NativeMethods.FileShareRead | (readOnly ? 0 : NativeMethods.FileShareWrite),
                IntPtr.Zero,
                NativeMethods.OpenExisting,
                0,
                IntPtr.Zero);
            return hDrive == NativeMethods.InvalidHandleValue ? null : new DriveIO(hDrive, diskId, path, readOnly);
        }

        private bool ReadSectorBuffer(bool peek)
        {
            var success = NativeMethods.ReadFile(
                DriveHandle,
                _sectorBuffer,
                (int)SectorSize,
                out _,
                IntPtr.Zero);
            var error = NativeMethods.GetLastError();

            if (!success)
            {
                return false;
            }
            if (!peek)
            {
                return true;
            }

            var sectorSeek = _sectorSeek;
            SeekTo((int)(CurPosition() - SectorSize));
            _sectorSeek = sectorSeek;

            return true;
        }

        public long Read([Out] byte[] buffer, int numberOfBytesToRead)
        {
            if (buffer.Length < numberOfBytesToRead)
            {
                return buffer.Length;
            }
            if (numberOfBytesToRead <= 0)
            {
                return 0;
            }

            var destIndex = 0L;
            var numSectors = (numberOfBytesToRead + _sectorSeek) / SectorSize;
            for (var i = 0; i < numSectors; ++i)
            {
                if (!ReadSectorBuffer(false))
                {
                    return destIndex;
                }

                var size = SectorSize - _sectorSeek;
                Array.Copy(_sectorBuffer, _sectorSeek, buffer, destIndex, size);
                _sectorSeek = 0; /* Aligned with sector */
                destIndex += size;
            }

            var remaining = numberOfBytesToRead % SectorSize;
            if (remaining <= 0)
            {
                return destIndex;
            }
            if (!ReadSectorBuffer(true))
            {
                return destIndex;
            }

            Array.Copy(_sectorBuffer, _sectorSeek, buffer, destIndex, remaining);
            _sectorSeek = remaining; /* re-aligned with buffer end */
            destIndex += remaining;

            return destIndex;
        }

        public long Write([In] byte[] buffer, long numberOfBytesToWrite)
        {
            if (buffer.Length < numberOfBytesToWrite) // Clamp
            {
                numberOfBytesToWrite = buffer.Length;
            }

            if (numberOfBytesToWrite <= 0)
            {
                return 0;
            }

            var bufferPos = 0L;
            var numSectors = (numberOfBytesToWrite + _sectorSeek) / SectorSize;
            for (var i = 0; i < numSectors; ++i)
            {
                var size = SectorSize - _sectorSeek;
                if (_sectorSeek != 0) /* Aligned with sectors */
                {
                    if (!ReadSectorBuffer(true))
                    {
                        return bufferPos;
                    }
                }

                Array.Copy(buffer, bufferPos, _sectorBuffer, _sectorSeek, size);

                if (!NativeMethods.WriteFile(
                    DriveHandle,
                    _sectorBuffer,
                    (int)SectorSize,
                    out _,
                    IntPtr.Zero))
                {
                    var err = NativeMethods.GetLastError();
                    return bufferPos;
                }

                _sectorSeek = 0; /* Aligned with sector */
                bufferPos += size;
            }

            numberOfBytesToWrite -= bufferPos;

            /* write last remaining buffer */
            if (numberOfBytesToWrite <= 0)
            {
                return bufferPos;
            }

            if (!ReadSectorBuffer(true))
            {
                return bufferPos;
            }

            Array.Copy(buffer, 0, _sectorBuffer, _sectorSeek, numberOfBytesToWrite);
            if (!NativeMethods.WriteFile(
                DriveHandle,
                _sectorBuffer,
                (int)SectorSize,
                out _,
                IntPtr.Zero))
            {
                var err = NativeMethods.GetLastError();
                return bufferPos;
            }

            bufferPos += numberOfBytesToWrite;
            _sectorSeek += numberOfBytesToWrite;
            SeekTo((int)(CurPosition() - SectorSize));

            return bufferPos;
        }

        public long SeekTo(long offset)
        {
            _sectorSeek = offset % SectorSize;

            var fullSector = offset / SectorSize;
            var distance = new NativeMethods.LARGE_INTEGER { QuadPart = fullSector * SectorSize };

            return NativeMethods.SetFilePointerEx(DriveHandle, distance, out var result, NativeMethods.FileBegin) != 1 ? CurPosition() : result.QuadPart;
        }

        public long CurPosition()
        {
            var distance = new NativeMethods.LARGE_INTEGER { QuadPart = 0 };

            NativeMethods.SetFilePointerEx(DriveHandle, distance, out var result, NativeMethods.FileCurrent);

            return result.QuadPart;
        }

        public long SeekEnd()
        {
            var distance = new NativeMethods.LARGE_INTEGER { QuadPart = 0 };

            NativeMethods.SetFilePointerEx(DriveHandle, distance, out var result, NativeMethods.FileEnd);

            return result.QuadPart;
        }

        public long WriteFileAt(string sourceFile, int destOffset, int sourceOffset, int bytesToWrite = 0)
        {
            if (!File.Exists(sourceFile))
            {
                return 0;
            }

            var fi = new FileInfo(sourceFile);
            if (fi.Length < sourceOffset + bytesToWrite)
            {
                return 0;
            }

            var fileBytes = File.ReadAllBytes(sourceFile);

            var subset = bytesToWrite == 0
                ? fileBytes.Skip(sourceOffset).ToArray()
                : fileBytes.Skip(sourceOffset).Take(bytesToWrite).ToArray();
            SeekTo(destOffset);
            return Write(subset, subset.Length);
        }

        public long ZeroBytes(int offset, int length)
        {
            SeekTo(offset);
            var bytesWritten = 0L;
            // a sector of zeroed bytes
            var sector = new byte[512];
            for (var i = 0; i < 512; ++i)
            {
                sector[i] = 0;
            }

            // write full sectors
            var fullSector = length / 512;
            for (var i = 0; i < fullSector; ++i)
            {
                bytesWritten += Write(sector, 512);
            }

            if (length % 512 != 0)
            {
                bytesWritten += Write(sector, length % 512);
            }

            return bytesWritten;
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                NativeMethods.CloseHandle(DriveHandle);
            }

            IsDisposed = true;
        }

        public override void Flush()
        {
            // Do nothing
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return SeekTo(offset);

                case SeekOrigin.Current:
                    return SeekTo(Position + offset);

                case SeekOrigin.End:
                    var endPos = SeekEnd();
                    return SeekTo(endPos + offset);
            }

            return CurPosition();
        }

        public override void SetLength(long value)
        {
            // Not Implemented
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var temp = new byte[count];
            var bytes_read = Read(temp, count);
            Array.Copy(temp, 0, buffer, offset, bytes_read);
            return (int)bytes_read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer.Skip(offset).ToArray(), count);
        }

        private static class NativeMethods
        {
            public const uint GenericRead = 0x80000000;
            public const uint GenericWrite = 0x40000000;
            public const uint GenericExecute = 0x20000000;
            public const uint GenericAll = 0x10000000;

            public const uint FileShareRead = 0x00000001;
            public const uint FileShareWrite = 0x00000002;
            public const uint FileShareDelete = 0x00000004;

            public const uint CreateNew = 0x00000001;
            public const uint CreateAlways = 0x00000002;
            public const uint OpenExisting = 0x00000003;
            public const uint OpenAlways = 0x00000004;
            public const uint TruncateExisting = 0x00000005;

            public const uint FileFlagWriteThrough = 0x80000000;
            public const uint FileFlagOverlapped = 0x40000000;
            public const uint FileFlagNoBuffering = 0x20000000;
            public const uint FileFlagRandomAccess = 0x10000000;
            public const uint FileFlagSequentialScan = 0x08000000;
            public const uint FileFlagDeleteOnClose = 0x04000000;
            public const uint FileFlagBackupSemantics = 0x02000000;
            public const uint FileFlagPosixSemantics = 0x01000000;
            public const uint FileFlagSessionAware = 0x00800000;
            public const uint FileFlagOpenReparsePoint = 0x00200000;
            public const uint FileFlagOpenNoRecall = 0x00100000;
            public const uint FileFlagFirstPipeInstance = 0x00080000;

            public const int FileBegin = 0x00000000;
            public const int FileCurrent = 0x00000001;
            public const int FileEnd = 0x00000002;
            public const int InvalidSetFilePointer = -1;

            public static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr CreateFile(
                string disk,
                uint access,
                uint shareMode,
                IntPtr securityAttributes,
                uint creationDisposition,
                uint flags,
                IntPtr templateFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int SetFilePointer(
                [In] IntPtr handle,
                [In] int distanceToMove,
                out int distanceToMoveHigh,
                [In] int moveMethod);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int SetFilePointerEx(
                [In] IntPtr handle,
                [In] LARGE_INTEGER distanceToMove,
                out LARGE_INTEGER distanceToMoveHigh,
                [In] int moveMethod);

            [SuppressMessage("Microsoft.Interoperability", "CA1415:DeclarePInvokesCorrectly", Justification = "Do not want to force unsafe NativeMethod Signature")]
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadFile(IntPtr handle, [Out] byte[] buffer, int numberOfBytesToRead, out int numberOfBytesRead, IntPtr overlapped);

            [SuppressMessage("Microsoft.Interoperability", "CA1415:DeclarePInvokesCorrectly", Justification = "Do not want to force unsafe NativeMethod Signature")]
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteFile(IntPtr handle, [Out] byte[] buffer, uint numberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr overlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr handle);

            [DllImport("kernel32.dll")]
            public static extern uint GetLastError();

            [StructLayout(LayoutKind.Explicit, Size = 8)]
            public struct LARGE_INTEGER
            {
                [FieldOffset(0)] public long QuadPart;

                [FieldOffset(0)] public readonly uint LowPart;
                [FieldOffset(4)] public readonly int HighPart;

                [FieldOffset(0)] public readonly int LowPartAsInt;
                [FieldOffset(0)] public readonly uint LowPartAsUInt;

                [FieldOffset(4)] public readonly int HighPartAsInt;
                [FieldOffset(4)] public readonly uint HighPartAsUInt;
            }
        }
    }
}

namespace DiskTool
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BitInfoAttribute : Attribute
    {
        public BitInfoAttribute(byte length)
        {
            Length = length;
        }

        public byte Length { get; }
    }

    [Flags]
    public enum PartitionStatus : byte
    {
        Inactive = 0x00,
        Active = 0x80
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CHS
    {
        public byte Head;
        public byte Sector;
        public byte Cylindar;
        private ulong Address => ((ulong)Cylindar * 16 + Head) * 63 + Sector;
    }

#pragma warning disable CS3003 // Argument type is not CLS-compliant
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PartitionTable
    {
        public PartitionStatus Status;
        public CHS Start;
        public byte Type;
        public CHS End;
        public uint LBAStart;
        public uint NumSectors;

        public static PartitionTable[] FromByteArray(byte[] data)
        {
            var arr = new PartitionTable[4];
            if (data.Length != 64)
            {
                throw new Exception("Invalid data, input must be 64 bytes PartitionTable Data from MBR");
            }

            for (var i = 0; i < 4; ++i)
            {
                unsafe
                {
                    fixed (byte* packet = &data[i * 0x10])
                    {
                        arr[i] = *(PartitionTable*)packet;
                    }
                }
            }

            return arr;
        }

        public static PartitionTable[] FromMBR(byte[] data)
        {
            if (data.Length < Constants.MbrSize + Constants.PartitionTableSize)
            {
                throw new Exception($"Invalid data, input must be start of MBR with at least {Constants.MbrSize + Constants.PartitionTableSize} bytes");
            }

            return FromByteArray(data.Skip(Constants.MbrSize).Take(Constants.PartitionTableSize).ToArray());
        }
    }

    public static class Constants
    {
        public static int MbrSize = 0x1be;

        public static int PartitionTableSize = 64;

        public static int BootloaderSize = 515584;
    }

#pragma warning restore CS3003 // Argument type is not CLS-compliant
}