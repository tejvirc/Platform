#include "stdafx.h"
#include "Tovis.h"
#include "logger.h"

const UINT8 Tovis::PacketMode_Mouse = 0;
const UINT8 Tovis::PacketMode_Digitizer = 1;
const UINT8 Tovis::FlashSetting_FlipY = 0;
const UINT8 Tovis::FlashSetting_NormalY = 2;

//
// Created for easier readability in the code when these are referenced
//

const bool Tovis::Y_Coord_Is_Win_Standard = true;
const bool Tovis::Y_Coord_Not_Win_Standard = false;

//
// Table that maps the Kortek Monitor Firmware Version String to the Touch Firmware Version and describes how to interact with the device.
// The below table is sorted by the 'Touch Firmware' value, from highest to lowest.
//

const Tovis::FirmwareVerItem Tovis::Tovis_Firmware_Version_Map[] = {
    //  +---------------------------------+------------------+--------------------------+
    //  | Monitor Firmware Version String | Monitor Firmware | Y Coord Information      |
    //  +---------------------------------+------------------+--------------------------+
        { " 0.14",                           0.14,             Y_Coord_Is_Win_Standard  },  // Helix Plus 27" MAIN Monitor
        { "27.05",                          27.05,             Y_Coord_Not_Win_Standard },  // MarsX Upright 27" MAIN Monitor
        { "27.10",                          27.10,             Y_Coord_Not_Win_Standard },  // MarsX Upright 27" MAIN Monitor
        { "43.B3",                          43.00,             Y_Coord_Not_Win_Standard },  // MarsX Portrait 43" MAIN Monitor
        { "43.B6",                          43.00,             Y_Coord_Not_Win_Standard },  // MarsX Portrait 43" MAIN Monitor
        { "43.B8",                          43.00,             Y_Coord_Not_Win_Standard }   // MarsX Portrait 43" MAIN Monitor
};

Tovis::Tovis()
    :_pDevice(nullptr),
    _dev_info{},
    _packet_mode(0),
    _mirror_mode(0),
    _firmware_version(0.0),
    _get_status_succeeded(false)
{
    hid_init();
}

Tovis::~Tovis()
{
    if (_pDevice)
        hid_close(_pDevice);

    hid_exit();
}

bool Tovis::Connect(int restoreDelay)
{
    bool retValue = false;

    if (GetDeviceInfo())
    {
        if (Open())
        {
            char firmware_version[30];

            ZeroMemory(&firmware_version, sizeof(firmware_version));

            sprintf_s(firmware_version, sizeof(firmware_version) - 1, "%02X.%02X", (_dev_info.release_number >> 8) & 0xff, _dev_info.release_number & 0xff);
            _firmware_version = atof(firmware_version);
            Log("Tovis::Connect(): Firmware Version '%s'\n", firmware_version);

            //
            // NOTE/TODO/BUGBUG: Ideally, we return 'false' if GetStatus() fails, however, due to a firmware bug that causes the
            // GetStatus() call to fail, we need to proceed in order to be able to send commands to the firmware.  Once we are only
            // dealing with updated firmware that fixes this issue we should revert the code to it's initial statue.  (Remove the
            // 'else' condition)
            //
            // GetStatus calls HidD_GetFeature() which is what is failing to execute properly on certain Tovis firmware.
            //

            if (GetStatus(restoreDelay))
            {
                Log("Tovis::Connect(): GetStatus() Succeeded.\n");
                retValue = true;
            }
            else
            {
                Log("Tovis::Connect(): WARNING: GetStatus() FAILED!  Some or all settings may be incorrect, commands may fail or have unexpected results.\n");
                retValue =  true;
            }
        }
    }

    return retValue;
}

bool Tovis::GetStatus(int restoreDelay)
{
    bool retValue = false;
    tDATA_GET_STATUS_DATA data;

    if (get_status(_pDevice, &data))
    {
        _get_status_succeeded = true;
        _packet_mode = data.Packet;
        _mirror_mode = data.Flip;
        retValue = true;
    }
    else
    {
        _get_status_succeeded = false;

        //
        // NOTE/TODO/BUGBUG: Due to a firmware bug that causes the get_status() call to fail, we need to restore the firmware
        // settings back to their defaults, only if it does fail.  Once we are only dealing with updated firmware that fixes
        // this issue we should remove this code to it's initial statue.  (Remove the RestoreDefaults() call)
        // 'else' condition)
        //
        // GetStatus calls HidD_GetFeature() which is what is failing to execute properly on certain Tovis firmware.
        //

        Log("Tovis::GetStatus(): Restoring the firmware settings to the defaults as the get_status() call has failed.\n");
        if (RestoreDefault())
        {
            Log("Tovis::GetStatus(): Successfully restored the firmware to its default settings.\n");

            //
            // We need to add a sleep after we call RestoreDefault() as if we send a command to the firmware
            // to quickly after the call can cause that command to fail
            //

            if (restoreDelay != 0)
            {
                Log("Tovis::GetStatus(): Delaying for '%d' milliseconds after the RestoreDefault() call.\n", restoreDelay);
                Sleep(restoreDelay);
            }
        }
        else
        {
            Log("Tovis::GetStatus(): ERROR: Failed to restore the firmware to its default settings!\n");
        }
    }

    return retValue;
}

bool Tovis::SetMirrormode(const UINT8 mode)
{
    bool retValue = true;

    //Log("Tovis: SetMirrormode(): _mirror_mode = %d, mode = %d\n", _mirror_mode, mode);
    if ((_mirror_mode != mode) || (!_get_status_succeeded))
    {
        if (!_get_status_succeeded)
        {
            Log("Tovis::SetMirrormode(): GetStatus() failed so we are ignoring cached firmware information and forcing the write of mirror mode.\n");
        }

        retValue = set_mirror_mode(_pDevice, mode);

        //
        // Update our cached mirror mode data since its possible new data was written to the firmware.
        //
        
        if (retValue)
        {
            Log("Tovis::SetMirrormode(): Updating our cached mirror mode data.\n");
            _mirror_mode = mode;
        }
    }
    else
    {
        Log("Tovis::SetMirrormode(): No changes to flash settings, ignoring write.\n");
    }

    return retValue;
}

bool Tovis::SetPacketmode(const UINT8 mode)
{
    bool retValue = true;

    //Log("Tovis: SetPacketmode(): _packet_mode = %d, mode = %d\n", _packet_mode, mode);
    if ((_packet_mode != mode) || (!_get_status_succeeded))
    {
        if (!_get_status_succeeded)
        {
            Log("Tovis::SetPacketmode(): GetStatus() failed so we are ignoring cached firmware information and forcing the write of packet mode.\n");
        }

        retValue = set_packet_mode(_pDevice, mode);

        //
        // Refresh the cached packet mode data since its possible new data was written to the firmware.
        //

        if (retValue)
        {
            Log("Tovis::SetPacketmode(): Updating our cached packet mode data.\n");
            _packet_mode = mode;
        }
    }
    else
    {
        Log("Tovis::SetPacketmode(): No changes to flash settings, ignoring write.\n");
    }

    return retValue;
}

bool Tovis::SetAristocratStandard()
{
    bool retValue = false;

    //
    // Some Tovis firmware versions are not consistent in how to setup the Y coordinates in Windows.
    // We'll see if the current firmware version is setup properly, or if it is backwards from our first
    // supported version of 0.14.
    //

    if (IsYCoordWinStandard())
    {
        Log("Tovis::SetAristocratStandard(): Setting the mirror mode to FlashSetting_FlipY.\n");

        retValue = SetMirrormode(FlashSetting_FlipY);
    }
    else
    {
        Log("Tovis::SetAristocratStandard(): Setting the mirror mode to FlashSetting_NormalY.\n");

        retValue = SetMirrormode(FlashSetting_NormalY);
    }

    return retValue;
}

bool Tovis::SetWindowsStandard()
{
    bool retValue = false;

    //
    // Some Tovis firmware versions are not consistent in how to setup the Y coordinates in Windows.
    // We'll see if the current firmware version is setup properly, or if it is backwards from our first
    // supported version of 0.14.
    //

    if (IsYCoordWinStandard())
    {
        Log("Tovis::SetWindowsStandard(): Setting the mirror mode to FlashSetting_NormalY.\n");

        retValue = SetMirrormode(FlashSetting_NormalY);
    }
    else
    {
        Log("Tovis::SetWindowsStandard(): Setting the mirror mode to FlashSetting_FlipY.\n");

        retValue = SetMirrormode(FlashSetting_FlipY);
    }

    return retValue;
}

bool Tovis::GetPacketmode(UINT8& packet_mode)
{
    bool retValue = false;
    tDATA_GET_PACKET_DATA bufGetPacket = {};

    if (get_packet_mode(_pDevice, &bufGetPacket))
    {
        packet_mode = bufGetPacket.packet;
        retValue = true;
    }

    return retValue;
}

bool Tovis::RestoreDefault()
{
    bool retValue = false;

    if (set_restore_default(_pDevice, 0x01))
    {
        retValue = true;

        //
        // TODO/BUGBUG: We should Re-read settings from the firmware after the set_restore_default() call in
        // order to ensure our cached data is up to date.  We currently cannot do that due to current
        // firmware versions having a problem with GetStatus() failing.  We put the 'RestoreDefault()'
        // call in to the GetStatus() method when it fails, so if we do it here it will cause a recursive
        // problem
        //

        /*
        Log("Tovis::RestoreDefault(): Refreshing cached firmware settings.\n");
        if (GetStatus())
        {
            Log("Tovis::RestoreDefault(): GetStatus() Succeeded, firmware settings refreshed.\n");
        }
        else
        {
            Log("Tovis::RestoreDefault(): WARNING: GetStatus() FAILED! - Firmware settings were not refreshed.\n");
        }
        */
    }

    return retValue;
}

bool Tovis::Open()
{
    bool retValue = false;

    Log("Tovis::Open(): Opening HID device. | VID: '0x%04hx' | PID: '0x%04hx'\n", _dev_info.vendor_id, _dev_info.product_id);
    if (_pDevice = hid_open(_dev_info.vendor_id, _dev_info.product_id, NULL, 0x0D, 0x04))
    {
        retValue = true;
    }
    else if (_pDevice = hid_open(_dev_info.vendor_id, _dev_info.product_id, NULL, 0x01, 0x02))
    {
        retValue = true;
    }

    return retValue;
}

bool Tovis::GetDeviceInfo()
{
    const int vid_list[] = { 0x2619, 0x2b7f, 0x0914, 0x0596 };
    hid_device_info* devs = hid_enumerate(0x0, 0x0);
    auto* cur_dev = devs;
    bool found = false;

    while (cur_dev && !found)
    {
        for (int i = 0; i <= 3; ++i)
        {
            if (cur_dev->vendor_id == vid_list[i])
            {
                if (str_tolower(std::wstring(cur_dev->manufacturer_string)).find(L"nanots") >= 0)
                {
                    Log("Tovis::GetDeviceInfo(): Found VID: '0x%04hx' | PID: '0x%04hx' | InterfaceNo: '%d' | Product String: '%ls' | Manufacturer String: '%ls'\n", cur_dev->vendor_id, cur_dev->product_id, cur_dev->interface_number, cur_dev->product_string, cur_dev->manufacturer_string);
                    SaveDeviceInfo(cur_dev);
                    found = true;
                    break;
                }
            }
        }

        cur_dev = cur_dev->next;
    }

    hid_free_enumeration(devs);

    return found;
}

void Tovis::SaveDeviceInfo(const hid_device_info* dev)
{
    _dev_info.manufacturer_string = dev->manufacturer_string ? dev->manufacturer_string : L"Doesn't exist.";
    _dev_info.vendor_id = dev->vendor_id;
    _dev_info.product_id = dev->product_id;
    _dev_info.interface_number = dev->interface_number;
    _dev_info.path = dev->path ? dev->path : "";
    _dev_info.release_number = dev->release_number;
    _dev_info.product_string = dev->product_string ? dev->product_string : L"Doesn't exist.";
    _dev_info.serial_number = dev->serial_number ? dev->serial_number : L"Doesn't exist.";
    _dev_info.usage = dev->usage;
    _dev_info.usage_page = dev->usage_page;
}

bool Tovis::IsYCoordWinStandard()
{
    bool retValue = false;
    bool firmwareSupported = false;

    int numFirmwareMapItems = (sizeof(Tovis_Firmware_Version_Map) / sizeof(FirmwareVerItem));

    //
    // Search our Firmware Version map and see if we are dealing with firmware we know how to support
    //

    for (int x = 0; x < numFirmwareMapItems; ++x)
    {
        if (_firmware_version == Tovis_Firmware_Version_Map[x].monitor_Firmware_Version)
        {
            Log("Tovis::IsYCoordWinStandard(): Firmware found in map.\n");
            firmwareSupported = true;
            retValue = Tovis_Firmware_Version_Map[x].YCoordWinStandard;
            break;
        }
    }

    //
    // If we don't directly support the firmware found, we'll try to guess at the Y Coord info based
    // upon the monitor firmware version
    //

    if (!firmwareSupported)
    {
        Log("Tovis::IsYCoordWinStandard(): Firmware NOT found in map.");

        if (_firmware_version >= 27.05)
        {
            retValue = Tovis::Y_Coord_Not_Win_Standard;
        }
        else
        {
            retValue = Tovis::Y_Coord_Is_Win_Standard;
        }
    }

    return retValue;
}

void Tovis::PrintInfo() const
{
    Log("\n");
    Log("Tovis Device Information:\n\n");
    Log("  Item             | Value\n");
    Log("  -----------------+-------------------------------------------------------------------------------------\n");
    Log("  Mirror mode      | %d (Default-Linux:0, X-Flip:1, Y-Flip:2, X-Y-Flip:3)\n", _mirror_mode);
    Log("  Packet mode      | %d (Mouse:0, Digitizer:1)\n", _packet_mode);
    Log("  Vendor  VID      | 0x%04X\n", _dev_info.vendor_id);
    Log("  Product PID      | 0x%04X\n", _dev_info.product_id);
    Log("  Release VER      | %02X.%02X\n", (_dev_info.release_number >> 8) & 0xff, _dev_info.release_number & 0xff);
    Log("  Product          | %S\n", _dev_info.product_string.c_str());
    Log("  Manufacturer     | %S\n", _dev_info.manufacturer_string.c_str());
    Log("  Path             | %s\n", _dev_info.path.c_str());
    Log("  Interface number | %d\n", _dev_info.interface_number);
    Log("  Serial number    | %S\n", _dev_info.serial_number.c_str());
    Log("\n");
}

void Tovis::PrintFirmwareVersionMap()
{
    int numFirmwareMapItems = (sizeof(Tovis_Firmware_Version_Map) / sizeof(FirmwareVerItem));

    Log("Known Supported Tovis Devices:\n\n");
    Log("  Monitor Firmware | Y Coord Win Standard \n");
    Log("  -----------------+---------------------\n");
    for (int x = 0; x < numFirmwareMapItems; ++x)
    {
        Log("  ");
        LogOmitTime("%-17s| %s\n", Tovis_Firmware_Version_Map[x].monitor_Firmware_Version_String, Tovis_Firmware_Version_Map[x].YCoordWinStandard ? "Yes" : "No");
    }
}