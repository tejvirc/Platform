#pragma once

#include <platform.h>
#include <string.h>
#include "logger.h"
#include "cabinetinfo.h"

class MonitorInfo
{
public:
    MonitorInfo(CabinetInfo* pCabinetInfoObj)
    {
        Log(LogTarget::File, "MonitorInfo(): Constructing.\n");

        _pCabinetInfo = pCabinetInfoObj;
        _GetDisplayData();
    }

    ~MonitorInfo()
    {
        Log(LogTarget::File, "MonitorInfo(): Destructing.\n");

        _pCabinetInfo = nullptr;

        if (_rgDisplayInfo)
        {
            delete [] _rgDisplayInfo;
            _rgDisplayInfo = nullptr;
        }
    }

    typedef struct DisplayInfo
    {
        DISPLAY_DEVICE ddAdapter;
        DISPLAY_DEVICE ddMonitor;
        DEVMODE devMode;
        long monitorIndex;
    } DisplayInfo;

    DisplayInfo* GetDisplayInfoByIndex(long monitorIndex)
    {
        DisplayInfo* pReturnValue = nullptr;

        Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByIndex(monitorIndex = '%ld')\n", monitorIndex);

        if (nullptr != _rgDisplayInfo)
        {
            for (UINT32 i = 0; i < _numRgDisplayInfo; ++i)
            {
                if (monitorIndex == _rgDisplayInfo[i].monitorIndex)
                {
                    Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByIndex(): Found display info for monitor index '%ld'.\n", monitorIndex);

                    pReturnValue = &_rgDisplayInfo[i];
                    break;
                }
            }
        }
        else
        {
            Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByIndex(): _rgDisplayInfo is NULL.\n");
        }

        if (nullptr == pReturnValue)
        {
            Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByIndex(): Returning NULL.\n");
        }

        return pReturnValue;
    }

    bool Compare(MonitorInfo* rhs)
    {
        bool retValue = false;

        Log(LogTarget::File, "MonitorInfo():Compare()\n");

        if (rhs->_numRgDisplayInfo != _numRgDisplayInfo)
        {
            retValue = false;
        }

        if (nullptr != _rgDisplayInfo)
        {
            for (UINT32 i = 0; i < _numRgDisplayInfo; ++i)
            {
                /*
                if (rhs->_rgDisplayInfo[i].devMode.dmPosition.x != _rgDisplayInfo[i].devMode.dmPosition.x)
                    return false;

                if (rhs->_rgDisplayInfo[i].devMode.dmPosition.y != _rgDisplayInfo[i].devMode.dmPosition.y)
                    return false;
                //
                // TODO: Compare more
                //
                */

                if (memcmp(&rhs->_rgDisplayInfo[i].devMode, &_rgDisplayInfo[i].devMode, sizeof(DEVMODE)) != 0)
                {
                    retValue = false;
                    break;
                }
            }
        }
        else
        {
            Log(LogTarget::File, "MonitorInfo():Compare(): _rgDisplayInfo is NULL.\n");
        }

        return retValue;
    }

    DisplayInfo* GetDisplayInfoFromHWND(HWND hWnd)
    {
        ::MONITORINFOEX monitor_info;

        Log(LogTarget::File, "MonitorInfo():GetDisplayInfoFromHWND(hWnd: %p)\n", hWnd);

        monitor_info.cbSize = sizeof(monitor_info);
        if (GetMonitorInfo(MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST), &monitor_info))
        {
            Log(LogTarget::File, "MonitorInfo():GetDisplayInfoFromHWND(): monitor_info.szDevice: '%s'.\n", monitor_info.szDevice);

            MonitorInfo info(_pCabinetInfo);

            return info.GetDisplayInfoByDeviceName(monitor_info.szDevice);
        }

        return nullptr;
    }

    DisplayInfo* GetPrimaryDisplay()
    {
        DisplayInfo* pReturnValue = nullptr;

        Log(LogTarget::File, "MonitorInfo():GetPrimaryDisplay()\n");

        if (nullptr != _rgDisplayInfo)
        {
            for (UINT32 i = 0; i < _numRgDisplayInfo; ++i)
            {
                if (_rgDisplayInfo[i].ddAdapter.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)
                {
                    pReturnValue = &_rgDisplayInfo[i];
                    break;
                }
            }
        }
        else
        {
            Log(LogTarget::File, "MonitorInfo():GetPrimaryDisplay(): _rgDisplayInfo is NULL.\n");
        }

        return pReturnValue;
    }

     //
     // monitorRole can be "Main", "Top", "VBD", "Topper", "Topper_1600x900", or "Topper_1920x1080"
     //

    int GetDisplayInfoByRole(const char* monitorRole)
    {
        int retValue = -1;

        if (0 == _stricmp(monitorRole, "main"))
        {
            retValue = 0;
            Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByRole(): Returning - '%d' (main).\n", retValue);
        }
        else if (0 == _stricmp(monitorRole, "top"))
        {
            retValue = 1;
            Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByRole(): Returning - '%d' (top).\n", retValue);
        }
        else if (0 == _stricmp(monitorRole, "vbd"))
        {
            retValue = 2;
            Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByRole(): Returning - '%d' (vbd).\n", retValue);
        }
        else if (0 == _stricmp(monitorRole, "topper"))
        {
            retValue = 3;
            Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByRole(): Returning - '%d' (topper).\n", retValue);
        }
        else
        {
            Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByRole(): Returning - '%d' (none).\n", retValue);
        }

        return retValue;
     }

    DisplayInfo* GetDisplayInfoByDeviceName(const char* deviceName)
    {
        DisplayInfo* pReturnValue = nullptr;

        Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByDeviceName(%s)\n", deviceName);

        if (nullptr != _rgDisplayInfo)
        {
            for (UINT32 i = 0; i < _numRgDisplayInfo; ++i)
            {
                if (0 == strcmp(_rgDisplayInfo[i].ddAdapter.DeviceName, deviceName))
                {
                    pReturnValue = &_rgDisplayInfo[i];
                    break;
                }
            }
        }
        else
        {
            Log(LogTarget::File, "MonitorInfo():GetDisplayInfoByDeviceName(): _rgDisplayInfo is NULL.\n");
        }

        return pReturnValue;
    }

    static UINT GetNumActiveMonitors(void)
    {
        Log(LogTarget::File, "MonitorInfo():GetNumActiveMonitors()\n");

        UINT32 numActiveMonitors = 0;
        UINT32 counter = 0;
        DISPLAY_DEVICE disDeviceAdapter = { 0 };
    
        disDeviceAdapter.cb = sizeof(DISPLAY_DEVICE);

        while (EnumDisplayDevices(NULL, counter, &disDeviceAdapter, 0))
        {
            if ((disDeviceAdapter.StateFlags & DISPLAY_DEVICE_ACTIVE) == DISPLAY_DEVICE_ACTIVE)
            {
                ++numActiveMonitors;
            }

            ++counter;
        }

        Log(LogTarget::File, "MonitorInfo():GetNumActiveMonitors(): Complete.  Returning '%d'.\n", numActiveMonitors);

        return numActiveMonitors;
    }

    UINT Count()
    {
        return _numRgDisplayInfo;
    }

    static std::map<std::string, std::string>& GetDisplayToRoleMap()
    {
        static std::map<std::string, std::string> displayToRoleMap;

        return displayToRoleMap;
    }

private:
    void _GetDisplayData()
    {
        Log(LogTarget::File, "MonitorInfo():_GetDisplayData()\n");

        _numRgDisplayInfo = GetNumActiveMonitors();

        for (auto retries = 0; retries < 5; ++retries)
        {
            _numRgDisplayInfo = GetNumActiveMonitors();
            if (_numRgDisplayInfo > 0)
            {
                break;
            }
            else
            {
                Log(LogTarget::File, "MonitorInfo():_GetDisplayData(): GetNumActiveMonitors() returned 0, retrying. (#%d of 5).\n", retries + 1);
            }

            ::Sleep(1500);
        }

        UpdateDisplayRoleMap();

        if (_numRgDisplayInfo == 0)
        {
            return;
        }

        _rgDisplayInfo = new DisplayInfo[_numRgDisplayInfo];
        if (nullptr != _rgDisplayInfo)
        {
            ZeroMemory(_rgDisplayInfo, sizeof(DisplayInfo) * _numRgDisplayInfo);
            GetActiveMonitorInformation(_rgDisplayInfo, _numRgDisplayInfo);

            for (UINT32 i = 0; i < _numRgDisplayInfo; ++i)
            {
                auto it = _roleToDisplayMap.find(_rgDisplayInfo[i].ddAdapter.DeviceName);
                if (it != _roleToDisplayMap.end())
                {
                    _rgDisplayInfo[i].monitorIndex = GetDisplayInfoByRole(it->second.c_str());
                }
            }
        }
        else
        {
            Log(LogTarget::File, "MonitorInfo():_GetDisplayData(): _rgDisplayInfo is NULL.  Failed to allocate memory.\n");
        }

        Log(LogTarget::File, "MonitorInfo():_GetDisplayData(): Complete.\n");
    }

    bool GetActiveMonitorInformation(DisplayInfo* rgDisplayInfo, UINT32 numRgDisplayInfo)
    {
        bool returnValue = false;
        UINT32 displayInfoIndex = 0;
        UINT32 adapterIndex = 0;

        Log(LogTarget::File, "MonitorInfo():GetActiveMonitorInformation()\n");

        //
        // Loop will exit on the following conditions:
        //    Enumerated through all adapters on the machine
        //    Found the number of active monitors returned by GetNumActiveMonitors
        //    API Call fails
        //

        while (true)
        {
            ZeroMemory(&rgDisplayInfo[displayInfoIndex].ddAdapter, sizeof(DISPLAY_DEVICE));
            rgDisplayInfo[displayInfoIndex].ddAdapter.cb = sizeof(DISPLAY_DEVICE);
            if (!EnumDisplayDevices(NULL, adapterIndex, &rgDisplayInfo[displayInfoIndex].ddAdapter, 0))
            {
                //
                // Finished enumerating through the adapters
                //

                break;
            }

            if ((rgDisplayInfo[displayInfoIndex].ddAdapter.StateFlags & DISPLAY_DEVICE_ACTIVE) != DISPLAY_DEVICE_ACTIVE)
            {
                //
                // Not one of our active monitors, we need to go to the next one but preserve the displayInfoIndex value
                //

                ++adapterIndex;
                continue;
            }

            rgDisplayInfo[displayInfoIndex].ddMonitor.cb = sizeof(DISPLAY_DEVICE);
            returnValue = EnumDisplayDevices(rgDisplayInfo[displayInfoIndex].ddAdapter.DeviceName, 0, &rgDisplayInfo[displayInfoIndex].ddMonitor, 0);
            if (!returnValue)
            {
                break;
            }

            rgDisplayInfo[displayInfoIndex].devMode.dmSize = sizeof(DEVMODE);
            rgDisplayInfo[displayInfoIndex].devMode.dmSpecVersion = DM_SPECVERSION;
            returnValue = EnumDisplaySettingsEx(rgDisplayInfo[displayInfoIndex].ddAdapter.DeviceName, ENUM_REGISTRY_SETTINGS, &rgDisplayInfo[displayInfoIndex].devMode, 0);
            if (!returnValue)
            {
                break;
            }

            //
            // Store the monitor index so we can issue commands based on what the user passed to us
            //

            char* pChar = strrchr(rgDisplayInfo[displayInfoIndex].ddAdapter.DeviceKey, '\\');
            if (nullptr == pChar)
            {
                break;
            }

            ++pChar; // Skip the '\' character and get to the number
            errno = 0;
            rgDisplayInfo[displayInfoIndex].monitorIndex = strtoul(pChar, NULL, 10);
            if (errno == ERANGE)
            {
                break;
            }

            ++displayInfoIndex;
            if (displayInfoIndex >= numRgDisplayInfo)
            {
                //
                // If this happens it means we found all the active displays.  We can leave this loop now as any other
                // displays on the machine should be non-active
                //

                returnValue = true;
                break;
            }

            ++adapterIndex;
        }

        Log(LogTarget::File, "MonitorInfo():GetActiveMonitorInformation(): Complete.  Returning '%s'.\n", returnValue ? "true" : "false");

        return returnValue;
    }

    void UpdateDisplayRoleMap() 
    {
        if (_pCabinetInfo != nullptr)
        {
            Log(LogTarget::File, "MonitorInfo():UpdateDisplayRoleMap()\n");

            _roleToDisplayMap.clear();
            auto value = _pCabinetInfo->GetCabinetInfo();
            auto&& displays = value["Screens"];
            for (int i = 0; i < static_cast<int>(displays.size()); i++)
            {
                auto role = displays[i]["Role"].asString();
                std::transform(role.begin(), role.end(), role.begin(), ::tolower);
                _roleToDisplayMap[displays[i]["DeviceName"].asString()] = role;
            }

            Log(LogTarget::File, "MonitorInfo():UpdateDisplayRoleMap(): Complete.\n");
        }
        else
        {
            Log(LogTarget::File, "MonitorInfo():UpdateDisplayRoleMap(): Ignored.\n");
        }
    }

private:
    DisplayInfo* _rgDisplayInfo = nullptr;
    UINT32       _numRgDisplayInfo = 0;
    CabinetInfo* _pCabinetInfo;
    std::map<std::string, std::string> _roleToDisplayMap;
};