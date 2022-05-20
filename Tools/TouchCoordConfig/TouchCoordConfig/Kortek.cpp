//
// TouchFlipDlg.cpp : implementation file
//

#include "stdafx.h"

#include "Kortek.h"
#include "Utils.h"
#include "DataFlash.h"
#include "CommunicationProtocol.h"
#include "logger.h"
#include <sstream>
#include <vector>
#include <string>
#include <cmath>

//
// Created for easier readability in the code when these are referenced
//

const bool Kortek::Reset_Is_Supported   = true;
const bool Kortek::Reset_Not_Supported  = false;
const bool Kortek::Model_Is_Rotated     = true;
const bool Kortek::Model_Not_Rotated    = false;

//
// Table that maps the Kortek Monitor Firmware Version String to the Touch Firmware Version and describes how to interact with the device.
// The below table is sorted by the 'Touch Firmware' value, from highest to lowest.
//

const Kortek::FirmwareVerItem Kortek::Kortek_Firmware_Version_Map[] = {
//  +---------------------------------------+------------------+----------------+-------------------------+----------------------+-------------------------+----------------------+----------------------+-----------------------+-----------------------+
//  | Kortek Monitor Firmware Ver String    | Monitor Firmware | Touch Firmware | Kortek Object Version   | Windows Flip Setting | Aristocrat Flip Setting | Win Switch Setting   | ATI Switch Setting   | Reset Support         | Model Type            |
//  +---------------------------------------+------------------+----------------+-------------------------+----------------------+-------------------------+----------------------+----------------------+-----------------------+-----------------------+
    { "Version 4.00.14, 2019-09-26 (USB)",    4.0014,            195.102,         ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // MarsX Single-Bash VBD Monitor
    { "Version 4.00.43, 2020-10-21 (USB)",    4.0043,            15.49,           ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Helix XT VBD Monitor
    { "Version 4.00.14, 2019-09-26 (USB)",    4.0014,            15.40,           ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Helix XT VBD Monitor
    { "Version 4.00.07, 2018-08-10 (USB)",    4.0007,            15.34,           ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Helix XT VBD Monitor
    { "Version 4.00.10, 2019-04-19 (USB)",    4.0010,            7.30,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::YFlip,       SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Helix+ 27" MAIN Monitor
    { "Version 4.00.07, 2018-08-10 (USB)",    4.0007,            7.24,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::YFlip,       SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Helix+ 27" MAIN Monitor
    { "Version 4.00.02, 2018-01-17",          4.0002,            7.22,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::YFlip,       SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Helix+ 27" MAIN Monitor
    { "Version 3.05.13, 2017-03-17",          3.0513,            7.12,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::YFlip,       SwitchSetting::Off,    SwitchSetting::Off,    Reset_Not_Supported,    Model_Not_Rotated    },  // Helix+ and Helix Slant 27" MAIN Monitor
    { "Version 3.05.08, 2016-09-26",          3.0508,            7.01,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::YFlip,       SwitchSetting::Off,    SwitchSetting::Off,    Reset_Not_Supported,    Model_Not_Rotated    },  // Helix+ 27" MAIN Monitor
    { "Version 6.00.00, 2017-11-28",          6.0000,            6.35,            ObjectVersion::KOV_317,   FlipSetting::XYFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::On,     Reset_Is_Supported,     Model_Not_Rotated    },  // Helix XT Portrait 43" MAIN Monitor
    { "Version 5.03.06, 2016-10-13",          5.0306,            6.34,            ObjectVersion::KOV_317,   FlipSetting::XYFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::On,     Reset_Is_Supported,     Model_Not_Rotated    },  // Helix XT Portrait 43" MAIN Monitor
    { "Version 5.03.06, 2016-10-13",          5.0306,            6.32,            ObjectVersion::KOV_317,   FlipSetting::XYFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::On,     Reset_Is_Supported,     Model_Not_Rotated    },  // Helix XT Portrait 43" MAIN Monitor
    { "Version Unknown",                      0.0,               3.70,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::YFlip,       SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Helix 23" MAIN Monitor
    { "Version 3.1.8, 2015-11-24",            3.18,              3.13,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::YFlip,       SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Helix 23" MAIN Monitor
    { "Version 3.2.6, 2018-01-18",            3.26,              3.02,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::YFlip,       SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Helix 23" MAIN Monitor
    { "Version 3.0.3, 2014-05-14",            3.03,              3.00,            ObjectVersion::KOV_303,   FlipSetting::NoFlip,   FlipSetting::YFlip,       SwitchSetting::Off,    SwitchSetting::Off,    Reset_Not_Supported,    Model_Not_Rotated    },  // Helix 23" MAIN Monitor
    { "Version 4.00.05, 2018-06-08 (USB)",    4.0005,            2.96,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // Bartop 23" MAIN Dev-Rig Monitor
    { "Version 4.00.10, 2019-04-19 (USB)",    4.0010,            2.130,           ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Is_Rotated     },  // Bartop 23" MAIN EGM Monitor
    { "Version Unknown",                      0.0,               2.124,           ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Is_Rotated     },  // Bartop 23" MAIN EGM Monitor
    { "Version 4.00.07, 2018-08-10 (USB)",    4.0007,            2.120,           ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Is_Rotated     },  // Bartop 23" MAIN EGM Monitor
    { "Version 4.00.07, 2018-08-10 (USB)",    4.0007,            1.101,           ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    },  // MarsX 27" VBD Monitor
    { "Version 3.1.7, 2015-09-03",            3.17,              0.00,            ObjectVersion::KOV_317,   FlipSetting::NoFlip,   FlipSetting::NoFlip,      SwitchSetting::Off,    SwitchSetting::Off,    Reset_Is_Supported,     Model_Not_Rotated    }
};

void Kortek::PrintFirmwareVersionMap()
{
    int numFirmwareMapItems = (sizeof(Kortek_Firmware_Version_Map) / sizeof(FirmwareVerItem));

    Log("Known Supported Kortek Devices:\n\n");
    Log("  Version String                          | Monitor Firmware | Touch Firmware\n");
    Log("  ----------------------------------------+------------------+---------------\n");
    for (int x = 0; x < numFirmwareMapItems; ++x)
    {
        Log("  ");
        LogOmitTime("%-40s| %-17f| %f\n", Kortek_Firmware_Version_Map[x].monitor_Firmware_Version_String, Kortek_Firmware_Version_Map[x].monitor_Firmware_Version, Kortek_Firmware_Version_Map[x].touch_Firmware_Version);
    }
}

class Kortek303 : public Kortek
{
    friend class Kortek;
    Kortek303(HidDevice* device, FlipSetting windowsFlipSettings = FlipSetting::NoFlip, FlipSetting aristocratFlipSettings = FlipSetting::NoFlip, SwitchSetting windowsSwitchSetting = SwitchSetting::Off, SwitchSetting aristocratSwitchSetting = SwitchSetting::Off, bool supportsReset = false, bool rotatedModel = false) : Kortek(device, windowsFlipSettings, aristocratFlipSettings, windowsSwitchSetting, aristocratSwitchSetting, supportsReset, rotatedModel)
    {
        ZeroMemory(&_dataFlashOld, sizeof(_dataFlashOld));
        ZeroMemory(&_original_dataFlashOld, sizeof(_original_dataFlashOld));
    }

    ~Kortek303() {}

public:
    virtual ApromVersion Version() { return ApromVersion::v3_0_3; }
    virtual bool ReadSettings();
    virtual bool WriteFlashSetting();
    virtual void PrintInfo() { _dataFlashOld.print(); }
    virtual void Set(FlashSetting setting);
    virtual void SetStandard(FlipSetting setting);
    virtual void SetAristocratStandard();
    virtual void SetWindowsStandard();
    virtual void SetInitialMode(DeviceMode setting);
    virtual void SetSwitch(SwitchSetting setting) { };

private:
    struct DataFlashOld _dataFlashOld;
    struct DataFlashOld _original_dataFlashOld;
};

class Kortek317 : public Kortek
{
    friend class Kortek;
    Kortek317(HidDevice* device, FlipSetting windowsFlipSettings = FlipSetting::NoFlip, FlipSetting aristocratFlipSettings = FlipSetting::NoFlip, SwitchSetting windowsSwitchSetting = SwitchSetting::Off, SwitchSetting aristocratSwitchSetting = SwitchSetting::Off, bool supportsReset = false, bool rotatedModel = false) : Kortek(device, windowsFlipSettings, aristocratFlipSettings, windowsSwitchSetting, aristocratSwitchSetting, supportsReset, rotatedModel)
    {
        ZeroMemory(&_dataFlashTouchFunc, sizeof(_dataFlashTouchFunc));
        ZeroMemory(&_original_dataFlashTouchFunc, sizeof(_original_dataFlashTouchFunc));
    }
    ~Kortek317() {}

public:
    virtual ApromVersion Version() { return ApromVersion::v3_1_7; }
    virtual bool ReadSettings();
    virtual bool WriteFlashSetting();
    virtual void PrintInfo() { _dataFlashTouchFunc.print(); }
    virtual void Set(FlashSetting setting);
    virtual void SetStandard(FlipSetting setting);
    virtual void SetAristocratStandard();
    virtual void SetWindowsStandard();
    virtual void SetInitialMode(DeviceMode setting);
    virtual void SetSwitch(SwitchSetting setting);

private:
    struct DataFlashTouchFunc _dataFlashTouchFunc;
    struct DataFlashTouchFunc _original_dataFlashTouchFunc;
};

//
// Kortek
//

void Kortek::Release()
{
    delete this;
}

Kortek::~Kortek()
{
    _pHidDevice->DisconnectDevice();
    delete _pHidDevice;
}

Kortek* Kortek::Connect(const char* pVID, const char* pPID, const char* pInterfaceNo)
{
    Kortek* pKortek = nullptr;
    HidDevice* hidDevice = new HidDevice();
    HidDeviceID Aprom(pVID, pPID, pInterfaceNo);

    Log("Kortek::Connect(): Looking for VID: '%s' | PID: '%s' | Interface Num: '%s' | ...", Aprom.vid.c_str(), Aprom.pid.c_str(), Aprom.interfaceNo.c_str());

    if (hidDevice->ConnectDevice(Aprom.Partial(), Aprom.Full()) == TRUE)
    {
        std::string  usbPidStr = Utils::GetDeviceInstancePid(hidDevice->GetDeviceInstancePath());
        if (usbPidStr.length() != 0)
        {
            LogOmitTime(" Found\n");

            Aprom.pid = usbPidStr;
            pKortek = DetectVersion(hidDevice);
            pKortek->ApromID = Aprom;
        }
    }
    else
    {
        LogOmitTime(" NOT Found\n");
    }

    return pKortek;
}

//
// Kortek 303
//

void Kortek303::Set(FlashSetting setting)
{
    switch (setting)
    {
        case FlashSetting::NormalY:
        {
            _dataFlashOld.yflip = 0;
        }
        break;

        case FlashSetting::FlipY:
        {
            _dataFlashOld.yflip = 1;
        }
        break;

        case FlashSetting::NormalX:
        {
            _dataFlashOld.xflip = 0;
        }
        break;

        case FlashSetting::FlipX:
        {
            _dataFlashOld.xflip = 1;
        }
        break;
    }
}

void Kortek303::SetStandard(FlipSetting setting)
{
    Log("Kortek303::SetStandard(): Received setting '%d'\n", setting);

    switch (setting)
    {
        case FlipSetting::NoFlip:
        {
            Log("Kortek303::SetStandard(): NoFlip\n");
            Set(FlashSetting::NormalY);
            Set(FlashSetting::NormalX);
        }
        break;

        case FlipSetting::XFlip:
        {
            Log("Kortek303::SetStandard(): XFlip\n");
            Set(FlashSetting::NormalY);
            Set(FlashSetting::FlipX);
        }
        break;

        case FlipSetting::YFlip:
        {
            Log("Kortek303::SetStandard(): YFlip\n");
            Set(FlashSetting::FlipY);
            Set(FlashSetting::NormalX);
        }
        break;

        case FlipSetting::XYFlip:
        {
            Log("Kortek303::SetStandard(): XYFlip\n");
            Set(FlashSetting::FlipY);
            Set(FlashSetting::FlipX);
        }
        break;
    }

    if (_rotatedModel)
    {
        Log("Kortek303::SetStandard(): Display is a 'Rotated Model'\n");
        if (_dataFlashOld.yflip == 0)
        {
            Log("Kortek303::SetStandard(): FlipY\n");
            Set(FlashSetting::FlipY);
        }
        else
        {
            Log("Kortek303::SetStandard(): NormalY\n");
            Set(FlashSetting::NormalY);
        }

        if (_dataFlashOld.xflip == 0)
        {
            Log("Kortek303::SetStandard(): FlipX\n");
            Set(FlashSetting::FlipX);
        }
        else
        {
            Log("Kortek303::SetStandard(): NormalX\n");
            Set(FlashSetting::NormalX);
        }
    }
}

void Kortek303::SetAristocratStandard()
{
    //
    // Set the Touch Origin Point
    //

    SetStandard(_aristocratFlipSettings);

    //
    // Set the 'Switch' setting in the firmware
    //

    SetSwitch(_aristocratSwitchSetting);
}

void Kortek303::SetWindowsStandard()
{
    //
    // Set the Touch Origin Point
    //

    SetStandard(_windowsFlipSettings);

    //
    // Set the 'Switch' setting in the firmware
    //

    SetSwitch(_windowsSwitchSetting);
}

void Kortek303::SetInitialMode(DeviceMode setting)
{
    switch (setting)
    {
        case DeviceMode::None:
        {
            _dataFlashOld.initialDeviceMode = 0;
            _dataFlashOld.maxTouchCount = 10;
        }
        break;

        case DeviceMode::Digitizer_1:
        {
            _dataFlashOld.initialDeviceMode = 2;
            _dataFlashOld.maxTouchCount = 1;
        }
        break;

        case DeviceMode::Digitizer_10:
        {
            _dataFlashOld.initialDeviceMode = 2;
            _dataFlashOld.maxTouchCount = 10;
        }
        break;

        case DeviceMode::Mouse:
        {
            _dataFlashOld.initialDeviceMode = 7;
        }
        break;

        default:
        {
            _dataFlashOld.initialDeviceMode = 2;
            _dataFlashOld.maxTouchCount = 10;
        }
        break;
    }
}

//
// Kortek 317
//

void Kortek317::Set(FlashSetting setting)
{
    switch (setting)
    {
        case FlashSetting::NormalY:
        {
            _dataFlashTouchFunc.yflip = 0;
        }
        break;

        case FlashSetting::FlipY:
        {
            _dataFlashTouchFunc.yflip = 1;
        }
        break;

        case FlashSetting::NormalX:
        {
            _dataFlashTouchFunc.xflip = 0;
        }
        break;

        case FlashSetting::FlipX:
        {
            _dataFlashTouchFunc.xflip = 1;
        }
        break;
    }
}

void Kortek317::SetStandard(FlipSetting setting)
{
    Log("Kortek317::SetStandard(): Received setting '%d'\n", setting);
    switch (setting)
    {
        case FlipSetting::NoFlip:
        {
            Log("Kortek317::SetStandard(): NoFlip\n");
            Set(FlashSetting::NormalY);
            Set(FlashSetting::NormalX);
        }
        break;

        case FlipSetting::XFlip:
        {
            Log("Kortek317::SetStandard(): XFlip\n");
            Set(FlashSetting::NormalY);
            Set(FlashSetting::FlipX);
        }
        break;

        case FlipSetting::YFlip:
        {
            Log("Kortek317::SetStandard(): YFlip\n");
            Set(FlashSetting::FlipY);
            Set(FlashSetting::NormalX);
        }
        break;

        case FlipSetting::XYFlip:
        {
            Log("Kortek317::SetStandard(): XYFlip\n");
            Set(FlashSetting::FlipY);
            Set(FlashSetting::FlipX);
        }
        break;
    }

    if (_rotatedModel)
    {
        Log("Kortek317::SetStandard(): Display is a 'Rotated Model'\n");
        if (_dataFlashTouchFunc.yflip == 0)
        {
            Log("Kortek317::SetStandard(): FlipY\n");
            Set(FlashSetting::FlipY);
        }
        else
        {
            Log("Kortek317::SetStandard(): NormalY\n");
            Set(FlashSetting::NormalY);
        }

        if (_dataFlashTouchFunc.xflip == 0)
        {
            Log("Kortek317::SetStandard(): FlipX\n");
            Set(FlashSetting::FlipX);
        }
        else
        {
            Log("Kortek317::SetStandard(): NormalX\n");
            Set(FlashSetting::NormalX);
        }
    }
}

void Kortek317::SetAristocratStandard()
{
    //
    // Set the Touch Origin Point
    //

    SetStandard(_aristocratFlipSettings);

    //
    // Set the 'Switch' setting in the firmware
    //

    SetSwitch(_aristocratSwitchSetting);
}

void Kortek317::SetWindowsStandard()
{
    //
    // Set the Touch Origin Point
    //

    SetStandard(_windowsFlipSettings);

    //
    // Set the 'Switch' setting in the firmware
    //

    SetSwitch(_windowsSwitchSetting);
}

void Kortek317::SetInitialMode(DeviceMode setting)
{
    //
    // initialDeviceMode Values (2 == Digitizer, 7 == Mouse)
    //

    switch (setting)
    {
        case DeviceMode::None:
        {
            _dataFlashTouchFunc.initialDeviceMode = 0;
            _dataFlashTouchFunc.maxTouchCount = 10;
        }
        break;

        case DeviceMode::Digitizer_1:
        {
            _dataFlashTouchFunc.initialDeviceMode = 2;
            _dataFlashTouchFunc.maxTouchCount = 1;
        }
        break;

        case DeviceMode::Digitizer_10:
        {
            _dataFlashTouchFunc.initialDeviceMode = 2;
            _dataFlashTouchFunc.maxTouchCount = 10;
        }
        break;

        case DeviceMode::Mouse:
        {
            _dataFlashTouchFunc.initialDeviceMode = 7;
        }
        break;

        default:
        {
            _dataFlashTouchFunc.initialDeviceMode = 2;
            _dataFlashTouchFunc.maxTouchCount = 10;
        }
        break;
    }
}

void Kortek317::SetSwitch(SwitchSetting setting)
{
    Log("Kortek317::SetSwitch(): Setting the xySwitch setting to '%d'\n", setting);

    _dataFlashTouchFunc.xySwitch = (int)setting;
}

bool Kortek::SendRecvLdrom(const char* log, BYTE cmd, unsigned char *pFwVer)
{

    BYTE sendBuf[64 + 4];
    BYTE recvBuf[64 + 4];
    unsigned int *piSendBuf = (unsigned int *) &sendBuf[4];
    unsigned int *piRecvBuf = (unsigned int *) &recvBuf[4];

    ZeroMemory(sendBuf, sizeof(sendBuf));
    ZeroMemory(recvBuf, sizeof(recvBuf));

    if (cmd == LDROM_CMD_SYNC_PACKET_NO)
    {
        piSendBuf[2] = _PacketNo = 1;
    }
    else if (cmd == LDROM_CMD_ERASE_ALL || cmd == LDROM_CMD_RUN_APROM || cmd == LDROM_CMD_GET_FWVER)
    {
    }
    else if (cmd == LDROM_CMD_UPDATE_APROM)
    {
    }
    else
    {
        return false;
    }

    piSendBuf[0] = cmd;
    piSendBuf[1] = _PacketNo;
    if (_pHidDevice->WriteDevice(sendBuf+3, sizeof(sendBuf)-3) == -1)
    {
        if (cmd == LDROM_CMD_RUN_APROM) // LDROM -> APROM, We would better to ignore the error here.
            return true;
        else
            return false;
    }

    if (cmd == LDROM_CMD_RUN_APROM)
        return true;

    _PacketNo++;

    while (1)
    {
        if (_pHidDevice->ReadDevice(recvBuf+3, sizeof(recvBuf)-3) == -1)
            return false;

        if (piRecvBuf[1] == _PacketNo)
        {
            if (cmd == LDROM_CMD_GET_FWVER)
            {
                if (pFwVer)
                    *pFwVer = recvBuf[12];
                break;
            }
            else if (cmd != LDROM_CMD_UPDATE_APROM)
            {
                break;
            }
            else
            {
            }
        }
    }

    _PacketNo++;

    return true;
}

double Kortek::GetMonitorFirmwareVersion(HidDevice* pHidDevice)
{
    double version = 0.0;
    BYTE buf[64];
    std::string ver_str;
    std::istringstream streamStr;
    std::vector<std::string> strings;
    std::string subStr;

    //
    // Check if the HID Device is connected.  If not, we cannot proceed.
    //

    if (!pHidDevice->IsConnected())
    {
        Log("Kortek::GetMonitorFirmwareVersion(): pHidDevice is NOT connected.  Unable to proceed.\n");

        goto Error;
    }

    //
    // Get the Kortek Monitor firmware Version String
    //

    ZeroMemory(buf, sizeof(buf));
    buf[0] = REPORTID_VENDOR;
    buf[1] = VENDOR_MAGIC;
    buf[2] = VENDOR_CMD_GET_FIRMWARE_VERSION;
    pHidDevice->WriteDevice(buf, sizeof(buf));
    pHidDevice->ReadDevice(buf, sizeof(buf));
    ver_str = (char*)&buf[8];
    Log("Kortek::GetMonitorFirmwareVersion(): Monitor Firmware '%s'\n", ver_str.c_str());

    //
    // First separate on the ","
    //

    streamStr.str(ver_str);
    while (std::getline(streamStr, subStr, ',')) {
        break;
    }

    if (subStr.empty())
    {
        Log("Kortek::GetMonitorFirmwareVersion(): (1) Unable to parse version number properly!\n");

        goto Error;
    }

    //
    // Now separate on the " "
    //

    streamStr.clear();
    streamStr.str(subStr);
    while (std::getline(streamStr, subStr, ' ')) {
        strings.push_back(subStr);
    }

    if ((strings.size() < 2) || (strings[1].empty()))
    {
        Log("Kortek::GetMonitorFirmwareVersion(): (2) Unable to parse version number properly!\n");

        goto Error;
    }

    version = atof(strings[1].c_str());

    //
    // Now separate on the "."
    //

    streamStr.clear();
    streamStr.str(strings[1]);
    strings.clear();
    while (std::getline(streamStr, subStr, '.')) {
        strings.push_back(subStr);
    }

    if (strings.size() < 3)
    {
        Log("Kortek::GetMonitorFirmwareVersion(): (3) Unable to parse version number properly!\n");

        goto Error;
    }

    // double major = atoi(strings[0].c_str());
    // double minor = atoi(strings[1].c_str());
    double patch = atoi(strings[2].c_str());

    version += patch / (pow(10.0, strings[1].length()) * pow(10.0, strings[2].length()));

Error:
    return version;
}

double Kortek::GetTouchFirmwareVersion(HidDevice* pHidDevice)
{
    double version = 0.0;
    struct HidReportVendor cmd(VENDOR_CMD_I2C_READ);
    struct HidReportVendor result(0);

    //
    // Check if the HID Device is connected.  If not, we cannot proceed.
    //

    if (!pHidDevice->IsConnected())
    {
        Log("Kortek::GetTouchFirmwareVersion(): pHidDevice is NOT connected.  Unable to proceed.\n");
        goto Error;
    }

    //
    // Setup and run the command that will return the version of the Kortek Touch controller
    //

    cmd.startAddress[0] = HITOUCH_VERSION_ADDR_MASTER & 0xff;
    cmd.startAddress[1] = (HITOUCH_VERSION_ADDR_MASTER & 0xff00) >> 8;
    cmd.len = 4;

    if ((pHidDevice->SendRecv((BYTE*)&cmd, sizeof(cmd), (BYTE*)&result, sizeof(result), 100 /*SendRecvTimeout*/) == false) || (result.response != VENDOR_CMD_RESPONSE_OK))
    {
        Log("Kortek::GetTouchFirmwareVersion(): pHidDevice->SendRecv() Failed!\n");
    }
    else
    {
        char ver_str[40];

        ZeroMemory(&ver_str, sizeof(char) * 40);
        sprintf_s(ver_str, 40, "%d.%02d", result.buf[0] | (result.buf[1] << 8), result.buf[2] | (result.buf[3] << 8));
        version = atof(ver_str);
    }

Error:
    //Log("Kortek::GetTouchFirmwareVersion(): Returning version '%f'.\n", version);
    return version;
}

Kortek* Kortek::DetectVersion(HidDevice* pHidDevice)
{
    Kortek* returnKortekObject = NULL;
    double monitorFirmwareVersion = 0.0;
    double touchFirmwareVersion = 0.0;
    ObjectVersion kortekObjectVersion = ObjectVersion::KOV_NULL;
    FlipSetting windowsFlipSettings = FlipSetting::NoFlip;
    FlipSetting aristocratFlipSettings = FlipSetting::YFlip;
    SwitchSetting windowsSwitchSetting = SwitchSetting::Off;
    SwitchSetting aristocratSwitchSetting = SwitchSetting::Off;
    bool resetSupported = Reset_Not_Supported;
    bool rotatedModel = Model_Not_Rotated;
    bool firmwareFound = false;
    int numFirmwareMapItems = (sizeof(Kortek_Firmware_Version_Map) / sizeof(FirmwareVerItem));

    //
    // Get the Kortek monitor firmware version string and the touch firmware version number
    //

    monitorFirmwareVersion = GetMonitorFirmwareVersion(pHidDevice);
    touchFirmwareVersion = GetTouchFirmwareVersion(pHidDevice);
    Log("Kortek::DetectVersion(): Monitor Firmware (%f), Touch Firmware (%f)\n", monitorFirmwareVersion, touchFirmwareVersion);

    //
    // If we could not determine either the Monitor or the Touch firmware version properly
    // we should just return the defaults and not attempt to the configure the device.
    //

    if ((monitorFirmwareVersion == 0.0) || (touchFirmwareVersion == 0.0))
    {
        Log("Kortek::DetectVersion(): Monitor or Touch Firmware is not supported.  Unable to properly query device.\n");

        goto Error;
    }

    //
    // Search our Firmware Version map and see if we are dealing with firmware we know how to support
    //

    for (int x = 0; x < numFirmwareMapItems; ++x)
    {
        if (touchFirmwareVersion == Kortek_Firmware_Version_Map[x].touch_Firmware_Version)
        {
            Log("Kortek::DetectVersion(): Firmware found in map.\n");

            firmwareFound = true;
            kortekObjectVersion = Kortek_Firmware_Version_Map[x].objectVersion;
            windowsFlipSettings = Kortek_Firmware_Version_Map[x].windowsFlipSettings;
            aristocratFlipSettings = Kortek_Firmware_Version_Map[x].aristocratFlipSettings;
            windowsSwitchSetting = Kortek_Firmware_Version_Map[x].windowsSwitchSetting;
            aristocratSwitchSetting = Kortek_Firmware_Version_Map[x].aristocratSwitchSetting;
            resetSupported = Kortek_Firmware_Version_Map[x].resetSupported;
            rotatedModel = Kortek_Firmware_Version_Map[x].rotatedModel;

            Log("Kortek::DetectVersion():     Object Version             = '%d'.\n", kortekObjectVersion);
            Log("Kortek::DetectVersion():     Windows Flip Setting       = '%d'.\n", windowsFlipSettings);
            Log("Kortek::DetectVersion():     Aristocrat Flip Setting    = '%d'.\n", aristocratFlipSettings);
            Log("Kortek::DetectVersion():     Windows Switch Setting     = '%d'.\n", windowsSwitchSetting);
            Log("Kortek::DetectVersion():     Aristocrat Switch Setting  = '%d'.\n", aristocratSwitchSetting);
            Log("Kortek::DetectVersion():     Reset Supported            = '%s'.\n", resetSupported ? "Yes" : "No");
            Log("Kortek::DetectVersion():     Rotated Model              = '%s'.\n", rotatedModel ? "Yes" : "No");

            break;
        }
    }

    //
    // If we didn't find the firmware in our map, we can try some 'generic' settings based on the
    // Monitor and Touch Firmware Versions.  This may get it 'wrong' in some cases.
    //

    if (!firmwareFound)
    {
        Log("Kortek::DetectVersion(): Firmware NOT found in map.");

        //
        // Determine the overall Object type to use based on the Monitor Firmware Version.
        // Currently there are only 2 types based on before and after version 3.0.3
        //

        if (monitorFirmwareVersion <= 3.03)
        {
            LogOmitTime("  Using settings for monitor firmware 3.03 and older.\n");

            kortekObjectVersion = ObjectVersion::KOV_303;
        }
        else
        {
            LogOmitTime("  Using settings for monitor firmware higher than 3.03.\n");

            kortekObjectVersion = ObjectVersion::KOV_317;
        }

        //
        // Determine if 'Reset' is supported based on the Monitor Firmware Version.
        // Currently believe this is based on before and after version 3.1.7
        //

        if (monitorFirmwareVersion >= 3.17)
        {
            resetSupported = Reset_Is_Supported;
        }

        //
        // Bartop MAIN Display
        //

        if ((touchFirmwareVersion >= 2.100) && (touchFirmwareVersion <= 2.199))
        {
            Log("Kortek::DetectVersion(): Found Bartop MAIN Display.\n");

            rotatedModel = Model_Is_Rotated;
            aristocratFlipSettings = FlipSetting::NoFlip;
        }

        //
        // Helix XT Portriat MAIN Display
        //
        // Not sure if 6.000 -> 6.999 is the correct range for this display.  The
        // Hardware Team (James Rist) says the valid range is touch firmware 6.000 to 6.099,
        // however when I query the Helix XT I'm seeing 6.32, 6.34, 6.35, etc.
        //

        if ((touchFirmwareVersion >= 6.000) && (touchFirmwareVersion <= 6.999))
        {
            Log("Kortek::DetectVersion(): Found Helix XT Portrait MAIN Display.\n");

            windowsFlipSettings = FlipSetting::XYFlip;
            aristocratFlipSettings = FlipSetting::NoFlip;
            windowsSwitchSetting = SwitchSetting::Off;
            aristocratSwitchSetting = SwitchSetting::On;
        }

        //
        // Helix XT VBD Display
        //

        if ((touchFirmwareVersion >= 15.000) && (touchFirmwareVersion <= 15.830))
        {
            Log("Kortek::DetectVersion(): Found Helix XT VBD Display.\n");

            windowsFlipSettings = FlipSetting::NoFlip;
            aristocratFlipSettings = FlipSetting::NoFlip;
        }

        //
        // Flame VBD Display
        // Touch Firmware Range: 15.199 through 15.840, newer versions expected be 15.105 through UNKNOWN
        //

        if (((touchFirmwareVersion >= 15.840) && (touchFirmwareVersion <= 15.999)) || ((touchFirmwareVersion >= 15.100) && (touchFirmwareVersion <= 15.199)))
        {
            Log("Kortek::DetectVersion(): Found Flame VBD Display.\n");

            //
            // TODO: Not supported yet, Aristocrat Monaco Platform hasn't yet supported the Flame cabinet type.
            //
        }

        //
        // MarsX Single-Bash and Dual-Bash VBD Displays
        // Single-Bash VBDs use the 195.1xx pattern.
        // Dual-Bash VBDs use the 195.2xx pattern.
        //

        if ((touchFirmwareVersion >= 195.000) && (touchFirmwareVersion < 195.255))
        {
            Log("Kortek::DetectVersion(): Found MarsX Single or Dual-Bash VBD Display.\n");

            windowsFlipSettings = FlipSetting::NoFlip;
            aristocratFlipSettings = FlipSetting::NoFlip;
        }
    }

    //
    // Create the return Kortek object based upon the parameters set based upon
    // the version that was returned by the firmware
    //

    switch (kortekObjectVersion)
    {
        case ObjectVersion::KOV_303:
        {
            Log("Kortek::DetectVersion(): Returning Kortek303, (Windows: '%d' | Aristocrat: '%d' | Win Switch: '%s' | ATI Switch: '%s' | %s | %s)\n", windowsFlipSettings, aristocratFlipSettings, (windowsSwitchSetting == SwitchSetting::On) ? "On" : "Off", (aristocratSwitchSetting == SwitchSetting::On) ? "On" : "Off", resetSupported ? "Reset Supported" : "Reset Not Supported", rotatedModel ? "Rotated Model" : "Not Rotated Model");
            returnKortekObject = new Kortek303(pHidDevice, windowsFlipSettings, aristocratFlipSettings, windowsSwitchSetting, aristocratSwitchSetting, resetSupported, rotatedModel);
        }
        break;

        case ObjectVersion::KOV_317:
        {
            Log("Kortek::DetectVersion(): Returning Kortek317, (Windows: '%d' | Aristocrat '%d' | Win Switch: '%s' | ATI Switch: '%s'| %s | %s)\n", windowsFlipSettings, aristocratFlipSettings, (windowsSwitchSetting == SwitchSetting::On) ? "On" : "Off", (aristocratSwitchSetting == SwitchSetting::On) ? "On" : "Off", resetSupported ? "Reset Supported" : "Reset Not Supported", rotatedModel ? "Rotated Model" : "Not Rotated Model");
            returnKortekObject = new Kortek317(pHidDevice, windowsFlipSettings, aristocratFlipSettings, windowsSwitchSetting, aristocratSwitchSetting, resetSupported, rotatedModel);
        }
        break;

        case ObjectVersion::KOV_NULL:
        default:
            break;
    }

Error:
    //
    // If we get here and the object version is NULL, we were unable
    // to determine how to configure this Kortek device.
    //

    if (kortekObjectVersion == ObjectVersion::KOV_NULL)
    {
        Log("Kortek::DetectVersion(): Returning KortekNull.\n");
        returnKortekObject = new KortekNull(pHidDevice);
    }

    return returnKortekObject;
}

bool Kortek::ConnectLdrom(const int maxTry, const int intervalTry)
{
    int i; 

    Log("Kortek::ConnectLdrom(): Connecting to LDROM ");

    for (i = 0; i < maxTry; i++)
    {
        if (_pHidDevice->ConnectDevice(LdromID.Partial(), LdromID.Full()) == FALSE)
        {
            LogOmitTime("-");
        }
        else
        {
            break;
        }

        if (_pHidDevice->ConnectDevice(ApromID.Partial(), ApromID.Full()) == FALSE)
        {
            LogOmitTime("-");
        }
        else
        {
            BYTE buf[64];

            ZeroMemory(buf, sizeof(buf));
            buf[0] = REPORTID_VENDOR;
            buf[1] = VENDOR_MAGIC;
            buf[2] = VENDOR_CMD_SET_LDROM;
            _pHidDevice->WriteDevice(buf, sizeof(buf));
            _pHidDevice->DisconnectDevice();
            LogOmitTime("-> Restarting to LDROM from APROM ");

            i = 0; // Reset retry counter
        }

        Sleep(intervalTry);
    }

    if (i == maxTry)
        return false;

    if (SendRecvLdrom( ""/*_T("Syncing packet no")*/, LDROM_CMD_SYNC_PACKET_NO) == false)
        return false;

    unsigned char fwVer;
    if (SendRecvLdrom(""/*_T("Getting firmware version of LDROM")*/, LDROM_CMD_GET_FWVER, &fwVer) == false)
        return false;

    LogOmitTime("-> Fetched Firmware Version from LDROM v%d.%d\n", (fwVer & 0xf0) >> 4, (fwVer & 0x0f));
    Log("Kortek::ConnectLdrom(): Restarted -> OK\n");
    
    return true;
}

bool Kortek::ConnectAprom(const int maxTry, const int intervalTry, const char* connectedMsgString)
{
    Log("Kortek::ConnectAprom(): Connecting to APROM");

    for (int i = 0; i < maxTry; i++)
    {
        Sleep(intervalTry/2);

        if (_pHidDevice->ConnectDevice(ApromID.Partial(), ApromID.Full()) == FALSE)
        {
            LogOmitTime(".");
        }
        else
        {
            LogOmitTime(" -> %s\n", connectedMsgString);

            return true;
        }
    }

    return false;
}

void Kortek::DisconnectAprom()
{
    _pHidDevice->DisconnectDevice();
}

bool Kortek::ResetAprom(int resetDelay)
{
    bool retValue = false;

    if (_supportsReset)
    {
        BYTE buf[64];

        ZeroMemory(buf, sizeof(buf));
        buf[0] = REPORTID_VENDOR;
        buf[1] = VENDOR_MAGIC;
        buf[2] = VENDOR_CMD_RESET_APROM;

        //
        // If we're not connected then the WriteDevice call will fail
        //

        if (!_pHidDevice->IsConnected())
        {
            if (ConnectAprom(maxTry, intervalTry, "OK") == false)
            {
                //
                // TODO: Should we continue on if this fails?
                //

                Log("Kortek::ResetAprom(): ERROR - Cannot connect to APROM\n");
            }
        }

        if (_pHidDevice->WriteDevice(buf, sizeof(buf)) == -1)
        {
            Log("Kortek::ResetAprom(): _pHidDevice->WriteDevice failed!\n");
            retValue = false; // We would better to ignore the error here.

            goto Done;
        }
    }
    else
    {
        if (ConnectLdrom(maxTry, intervalTry) == false)
        {
            retValue = false;

            goto Done;
        }

        if (SendRecvLdrom("Restarting APROM", LDROM_CMD_RUN_APROM) == false)
        {
            retValue = false;

            goto Done;
        }

        _pHidDevice->DisconnectDevice();
        if (ConnectAprom(maxTry, intervalTry, "Connected to APROM") == false)
        {
            retValue = false;

            goto Done;
        }
    }

    if (_pHidDevice->IsConnected())
    {
        DisconnectAprom();
    }

Done:
    if (resetDelay != 0)
    {
        Log("Kortek::ResetAprom(): Delaying after reset for '%d' milliseconds.\n", resetDelay);
        Sleep(resetDelay);
    }

    return retValue;
}

bool Kortek303::ReadSettings()
{
    if (ConnectAprom(maxTry, intervalTry, "OK") == false)
    {
        Log("Kortek303::ReadSettings(): ERROR: Failed to connect to APROM.\n");

        return false;
    }

    if (CDataFlash::ReadStructsOld(*_pHidDevice, &_dataFlashOld) == false)
    {
        Log("Kortek303::ReadSettings(): ERROR: Failed to read DataFlash\n");

        return false;
    }

    memcpy(&_original_dataFlashOld, &_dataFlashOld, sizeof(DataFlashOld));
    DisconnectAprom();

    return true;
}

bool  Kortek317::ReadSettings()
{
    if (ConnectAprom(maxTry, intervalTry, "Connected to APROM") == false)
    {
        Log("Kortek317::ReadSettings(): ERROR: Failed to connect to APROM\n");

        return false;
    }

    if (CDataFlash::ReadStructs(*_pHidDevice, NULL, &_dataFlashTouchFunc, NULL) == false)
    {
        Log("Kortek317::ReadSettings(): ERROR: Failed to read DataFlash\n");

        return false;
    }

    memcpy(&_original_dataFlashTouchFunc, &_dataFlashTouchFunc, sizeof(DataFlashTouchFunc));
    DisconnectAprom();

    return true;
}

bool Kortek317::WriteFlashSetting()
{
    bool success = false;

    Log("Kortek317::WriteFlashSetting(): Accessing DATAFLASH\n");

    if (ConnectAprom(maxTry, intervalTry, "OK") == false)
    {
        Log("Kortek317::WriteFlashSetting(): ERROR: Failed to connect to APROM\n");
        
        goto out;
    }

    bool HasChanges = memcmp(&_original_dataFlashTouchFunc, &_dataFlashTouchFunc, sizeof(DataFlashTouchFunc)) != 0;
    if (HasChanges)
    {
        if (CDataFlash::WriteStructs(*_pHidDevice, NULL, &_dataFlashTouchFunc, NULL) == false)
        {
            Log("Kortek317::WriteFlashSetting(): ERROR: Failed to write DataFlash\n");
            
            goto out;
        }

        Log("Kortek317::WriteFlashSetting(): Updated Flash Settings\n");
        memcpy(&_original_dataFlashTouchFunc, &_dataFlashTouchFunc, sizeof(DataFlashTouchFunc));
    }
    else
    {
        Log("Kortek317::WriteFlashSetting(): No changes to flash settings, ignoring write.\n");
        success = true;

        goto out;
    }

    success = true;
    DisconnectAprom();

out:
    _pHidDevice->DisconnectDevice();

    return success;
}

bool Kortek303::WriteFlashSetting()
{
    bool success = false;

    Log("Kortek303::WriteFlashSetting(): Accessing DATAFLASH\n");

    if (ConnectAprom(maxTry, intervalTry, "OK") == false)
    {
        Log("Kortek303::WriteFlashSetting(): ERROR - Cannot connect to APROM\n");

        goto out;
    }

    bool HasChanges = memcmp(&_original_dataFlashOld, &_dataFlashOld, sizeof(DataFlashOld)) != 0;
    if (HasChanges)
    {

        if (CDataFlash::WriteStructsOld(*_pHidDevice, &_dataFlashOld) == false)
        {
            Log("Kortek303::WriteFlashSetting(): ERROR - Failed to write DataFlash\n");

            goto out;
        }

        Log("Kortek303::WriteFlashSetting(): Updated Flash Settings\n");
        memcpy(&_original_dataFlashOld, &_dataFlashOld, sizeof(DataFlashOld));
    }
    else
    {
        Log("Kortek303::WriteFlashSetting(): No changes to flash settings, ignoring write.\n");
        success = true;

        goto out;
    }

    success = true;
    DisconnectAprom();

out:
    _pHidDevice->DisconnectDevice();

    return success;
}