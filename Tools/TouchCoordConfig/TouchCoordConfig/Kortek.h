#pragma once

#include "HidDevice.h"
#include "DataFlash.h"
#include "logger.h"
#include <sstream>

#define STATUS_STR_NONE             _T("")
#define STATUS_STR_SUCCESS          _T("Success")
#define STATUS_STR_FAIL             _T("Fail")
#define STATUS_COLOR_NONE           0
#define STATUS_COLOR_SUCCESS        1
#define STATUS_COLOR_FAIL           2

#define LDROM_CMD_SYNC_PACKET_NO    0xa4
#define LDROM_CMD_GET_FWVER         0xa6
#define LDROM_CMD_ERASE_ALL         0xa3
#define LDROM_CMD_UPDATE_APROM      0xa0
#define LDROM_CMD_RUN_APROM         0xab

const int maxTry = 20;
const int intervalTry = 2000;

struct HidDeviceID
{
    HidDeviceID()  { }
    HidDeviceID(const char* vid_, const char* pid_, const char* interfaceNo_ = "", const char* columnIndex_ = "") :
        vid(vid_),
        pid(pid_),
        interfaceNo(interfaceNo_),
        columnIndex(columnIndex_)
    {
        _partial.reserve(255);
        _full.reserve(255);
    }

    HidDeviceID(const HidDeviceID& hdid):
            vid(hdid.vid),
            pid(hdid.pid),
            interfaceNo(hdid.interfaceNo),
            columnIndex(hdid.columnIndex)
    {
        _partial.reserve(255);
        _full.reserve(255);
    }
    const char* Partial()
    {
        std::stringstream ss;
        ss << "vid_" << vid << "&pid_" << pid;
        _partial = ss.str();
        return _partial.c_str();
    }

    const char* Full()
    {
        std::stringstream ss;
        ss << Partial();
        if (interfaceNo != "")
        {
            ss << "&mi_" << interfaceNo;
        }
        if (columnIndex != "")
        {
            ss << "&col" << columnIndex;
        }

        _full = ss.str();
        return _full.c_str();
    }

    std::string vid;
    std::string pid;
    std::string interfaceNo;
    std::string columnIndex;
private:
    std::string _partial;
    std::string _full;
};

class Kortek
{
public:
    static const bool Reset_Is_Supported;
    static const bool Reset_Not_Supported;
    static const bool Model_Is_Rotated;
    static const bool Model_Not_Rotated;

    enum class ApromVersion
    {
        Unknown = 0,
        v3_0_3  = 1,
        v3_1_7  = 2
    };

    enum class FlashSetting
    {
        NormalY = 1,
        FlipY   = 2,
        NormalX = 3,
        FlipX   = 4
    };

    enum class SwitchSetting
    {
        Off = 0,
        On  = 1
    };

    enum class ObjectVersion
    {
        KOV_NULL = 0,
        KOV_303,
        KOV_317
    };

    enum class FlipSetting
    {
        NoFlip = 0,
        XFlip,
        YFlip,
        XYFlip
    };

    enum class DeviceMode
    {
        None = 0,
        Digitizer_1,
        Digitizer_10,
        Mouse
    };

    struct FirmwareVerItem
    {
        char* monitor_Firmware_Version_String;
        double monitor_Firmware_Version;
        double touch_Firmware_Version;
        ObjectVersion objectVersion;
        FlipSetting windowsFlipSettings;
        FlipSetting aristocratFlipSettings;
        SwitchSetting windowsSwitchSetting;
        SwitchSetting aristocratSwitchSetting;
        bool resetSupported;
        bool rotatedModel;
    };

    static const Kortek::FirmwareVerItem Kortek_Firmware_Version_Map[];

    Kortek(HidDevice* device, FlipSetting windowsFlipSettings = FlipSetting::NoFlip, FlipSetting aristocratFlipSettings = FlipSetting::NoFlip, SwitchSetting windowsSwitchSetting = SwitchSetting::Off, SwitchSetting aristocratSwitchSetting = SwitchSetting::Off, bool supportsReset = false, bool rotatedModel = false) :
        _pHidDevice(device),
        _apromVersion(ApromVersion::Unknown),
        LdromID("0416", "a316"),
        _windowsFlipSettings(windowsFlipSettings),
        _aristocratFlipSettings(aristocratFlipSettings),
        _windowsSwitchSetting(windowsSwitchSetting),
        _aristocratSwitchSetting(aristocratSwitchSetting),
        _supportsReset(supportsReset),
        _rotatedModel(rotatedModel),
        _PacketNo(0) {}
    
    virtual ~Kortek();
    
    static Kortek* Connect(const char* pVID, const char* pPID, const char* pInterfaceNo);
    static Kortek* DetectVersion(HidDevice* pHidDevice);
    static double GetMonitorFirmwareVersion(HidDevice* pHidDevice);
    static double GetTouchFirmwareVersion(HidDevice* pHidDevice);
    static void PrintFirmwareVersionMap();

    void Release();
    void DisconnectAprom();
    bool ConnectAprom(const int maxTry, const int intervalTry, const char* connectedMsgString);
    bool SendRecvLdrom(const char* log, BYTE cmd, unsigned char *pFwVer = NULL);
    bool ConnectLdrom(const int maxTry, const int intervalTry);
    virtual ApromVersion Version() = 0;
    virtual bool ResetAprom(int resetDelay = 0);
    virtual bool ReadSettings() = 0;
    virtual bool WriteFlashSetting() = 0;
    virtual void PrintInfo() = 0;
    virtual void Set(FlashSetting setting) = 0;
    virtual void SetStandard(FlipSetting setting) = 0;
    virtual void SetAristocratStandard() = 0;
    virtual void SetWindowsStandard() = 0;
    virtual void SetInitialMode(DeviceMode setting) = 0;
    virtual void SetSwitch(SwitchSetting setting) = 0;

    HidDevice* _pHidDevice;
    ApromVersion _apromVersion;
    HidDeviceID ApromID;
    HidDeviceID LdromID;
    FlipSetting _windowsFlipSettings;
    FlipSetting _aristocratFlipSettings;
    SwitchSetting _windowsSwitchSetting;
    SwitchSetting _aristocratSwitchSetting;
    bool _supportsReset;
    bool _rotatedModel;
    int _PacketNo;
};

class KortekNull : public Kortek
{
    friend class Kortek;

    KortekNull(HidDevice* device) : Kortek(device) { }

public:
    ApromVersion Version() { return ApromVersion::Unknown; }
    bool ResetAprom(int resetDelay = 0) { return false; }
    bool ReadSettings() { return false; }
    bool WriteFlashSetting() { return false; }
    void PrintInfo() { Log("Kortek: Unsupported Kortek device.\n"); }
    void Set(FlashSetting setting) { };
    virtual void SetStandard(FlipSetting setting) { };
    void SetAristocratStandard() { };
    void SetWindowsStandard() { };
    void SetInitialMode(DeviceMode setting) { };
    virtual void SetSwitch(SwitchSetting setting) { };
};