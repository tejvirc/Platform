namespace Stubs
{
    using Aristocrat.Monaco.Hardware.Contracts.IO;
    using Aristocrat.Monaco.Hardware.Contracts.SharedDevice;
    using System;

    public class StubIOImplementation : IIOImplementation
    {
        public Device DeviceConfiguration => throw new System.NotImplementedException();

        public int GetMaxInputs => 63;

        public int GetMaxOutputs => 16;

        public ulong GetInputs => 0;

        public InputEvent[] GetIntrusionEvents => Array.Empty<InputEvent>();

        public ulong GetOutputs => 0;

        public int GetWatchdogEnabled => 0;

        public bool WasCarrierBoardRemoved => false;

        public void Cleanup()
        {
        }

        public int ClearMechanicalMeter(int meterMask)
        {
            return meterMask;
        }

        public int EnableWatchdog(int seconds)
        {
            return 0;
        }

        public byte GetDoorSealValue()
        {
            return 0;
        }

        public byte[] GetFirmwareData(FirmwareData location)
        {
            return Array.Empty<Byte>();
        }

        public ulong GetFirmwareSize(FirmwareData location)
        {
            return 0;
        }

        public string GetFirmwareVersion(FirmwareData location)
        {
            return string.Empty;
        }

        public byte GetLogicDoorSealValue()
        {
            return 0;
        }

        public bool GetPhysicalDoorWasOpened(int physicalId)
        {
            return false;
        }

        public void Initialize()
        {
        }

        public void ResetDoorSeal()
        {
        }

        public void ResetPhysicalDoorWasOpened(int physicalId)
        {
        }

        public int ResetWatchdog()
        {
            return 0;
        }

        public bool SetBellState(bool ringBell)
        {
            return true;
        }

        public uint SetButtonLamp(uint lightIndex, bool lightStatusOn)
        {
            return (uint)(1 << (int)lightIndex);
        }

        public uint SetButtonLampByMask(uint lightBits, bool lightStatusOn)
        {
            return lightStatusOn ? lightBits : 0;
        }

        public void SetKeyIndicator(int keyMask, bool lightEnabled)
        {
        }

        public void SetLogicDoorSealValue(byte sealValue)
        {
        }

        public int SetMechanicalMeter(int meterMask)
        {
            return meterMask;
        }

        public void SetMechanicalMeterLight(bool lightEnabled)
        {
        }

        public bool SetRedScreenFreeSpinBankShow(bool bankOn)
        {
            return true;
        }

        public bool SetTowerLight(int lightIndex, bool lightOn)
        {
            return true;
        }

        public int StatusMechanicalMeter(int meterMask)
        {
            return meterMask;
        }

        public bool TestBattery(int batteryIndex)
        {
            return true;
        }

        public void TurnOutputsOff(ulong outputs)
        {
        }

        public void TurnOutputsOn(ulong outputs)
        {
        }
    }
}
