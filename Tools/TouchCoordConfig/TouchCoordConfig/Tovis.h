#pragma once

#include <algorithm>
#include <cctype>
#include <string>
#include "hidapi.h"

class Tovis
{
public:
    static const UINT8 PacketMode_Mouse;
    static const UINT8 PacketMode_Digitizer;
    static const UINT8 FlashSetting_FlipY;
    static const UINT8 FlashSetting_NormalY;

    static const bool Y_Coord_Is_Win_Standard;
    static const bool Y_Coord_Not_Win_Standard;

    struct FirmwareVerItem
    {
        char* monitor_Firmware_Version_String;
        double monitor_Firmware_Version;
        bool YCoordWinStandard;
    };

    static const Tovis::FirmwareVerItem Tovis_Firmware_Version_Map[];

    Tovis();
    ~Tovis();
    bool Connect(int restoreDelay = 0);
    bool SetMirrormode(const UINT8 mode);
    bool SetPacketmode(const UINT8 mode);
    bool SetAristocratStandard();
    bool SetWindowsStandard();
    bool RestoreDefault();
    void PrintInfo() const;
    static void PrintFirmwareVersionMap();

private:
    bool Open();
    bool GetDeviceInfo();
    bool GetPacketmode(UINT8& packet_mode);
    bool GetStatus(int restoreDelay = 0);
    void SaveDeviceInfo(const hid_device_info*);
    bool IsYCoordWinStandard();
    
    UINT8 _packet_mode;
    UINT8 _mirror_mode;
    double _firmware_version;
    bool _get_status_succeeded;

    struct _device_info
    {
        std::string path;
        unsigned short vendor_id;
        unsigned short product_id;
        std::wstring serial_number;
        unsigned short release_number;
        std::wstring manufacturer_string;
        std::wstring product_string;
        unsigned short usage_page;
        unsigned short usage;

        //
        // The USB interface which this logical device represents.
        // Valid on both Linux implementations in all cases, and valid on the
        // Windows implementation only if the device contains more than one interface.
        //

        int interface_number;
    } _dev_info;

    hid_device* _pDevice;
    
    static std::wstring str_toupper(std::wstring& s)
    {
        std::transform(s.begin(), s.end(), s.begin(), [](unsigned char c) { return std::toupper(c); });

        return s;
    }

    static std::wstring str_tolower(std::wstring& s)
    {
        std::transform(s.begin(), s.end(), s.begin(), [](unsigned char c) { return std::tolower(c); });

        return s;
    }
};