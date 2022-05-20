#include "stdafx.h"
#include "logger.h"
#include "Kortek.h"
#include "optionparser.h"
#include "Tovis.h"
#include <debugapi.h>

enum  optionIndex {
    UNKNOWN, HELP, DISPLAY, ARISTANDARD, WINSTANDARD, EXACT, PACKETMODE, SWITCH, RESET, KORTEKONLY, TOVISONLY, FILELOG, VERSION
};

using namespace std;
using namespace aristocrat;

const option::Descriptor usage[] =
{
    { UNKNOWN,      "",  "", option::Arg::None, "USAGE: TouchCoordConfig.exe [options]\n\n"
    "Options:" },
    { HELP,         "h", "help",        option::Arg::None,    "-h, --help          Prints usage and exits." },
    { DISPLAY,      "d", "display",     option::Arg::None,    "-d, --display       Displays info about the aprom." },
    { ARISTANDARD,  "a", "aristandard", option::Arg::None,    "-a, --aristandard   Set the coordinates to Aristocrat Standard." },
    { WINSTANDARD,  "w", "winstandard", option::Arg::None,    "-w, --winstandard   Set the coordinates to Windows Standard." },
    { EXACT,        "e", "exact",       option::Arg::String,  "-e, --exact         Set the firmware touch origin. Opts: [NoFlip | XFlip | YFlip | XYFlip]" },
    { PACKETMODE,   "p", "packetmode",  option::Arg::String,  "-p, --packetmode    Set the firmware mode. Opts: [None | Mouse | Digitizer1 | Digitizer10]" },
    { SWITCH,       "s", "switch",      option::Arg::String,  "-s, --switch        Set the firmware switch setting. Opts: [On | Off]"},
    { RESET,        "r", "reset",       option::Arg::Numeric, "-r, --reset         Reset the firmware to its default settings. Opts: [Millisecond Delay]"},
    { KORTEKONLY,   "k", "kortekonly",  option::Arg::None,    "-k, --kortekonly    Applies the specified settings to only Kortek devices."},
    { TOVISONLY,    "t", "tovisonly",   option::Arg::None,    "-t, --tovisonly     Applies the specified settings to only Tovis devices."},
    { FILELOG,      "f", "filelog",     option::Arg::String,  "-f, --filelog       Enables logging output to a file. Opts: [Filename]"},
    { VERSION,      "v", "version",     option::Arg::None,    "-v, --version       Display the executable Version Information." },
    { UNKNOWN,      "",  "",            option::Arg::None,    "\nExamples:\n"
    "  TouchCoordConfig.exe --winstandard --filelog \"D:\\Aristocrat-VLT\\Platform\\logs\\Log_TouchCoordConfig.log\" --version\n"
    "  TouchCoordConfig.exe --exact XYFlip\n"
    "  TouchCoordConfig.exe --packetmode Digitizer10 (Set to Digitizer mode with 10 finger touch support)\n"
    "  TouchCoordConfig.exe --packetmode Digitizer1  (Set to Digitizer mode with 1 finger touch support)\n"
    "  TouchCoordConfig.exe --switch On\n"
    "  TouchCoordConfig.exe --reset 5000 --tovisonly (Reset to default settings with 5000 millisecond delay)\n"
    "  TouchCoordConfig.exe --packetmode Digitizer10 --kortekonly\n"
    "  TouchCoordConfig.exe --packetmode Mouse --tovisonly\n"
    "  TouchCoordConfig.exe --display > D:\\CapturedOutput.txt\n\n"
    "Notes:\n"
    "  - You must specify at least one or more of these commands: display, aristandard, winstandard,\n"
    "    exact, packetmode, switch, reset, or version.\n"
    "  - Only one of these commands may be used at a time: aristandard, winstandard, or exact.\n"
    "  - Using '--display' will change the state of certain TOVIS firmware versions (like 43.B6) instead\n"
    "    of just displaying the state of the firmware.\n" },
    { 0, 0, 0, option::Arg::None, 0 }
};

//
// Retrieves the version number from the resource file
//

bool GetVersionFromRC(WCHAR* wstrProductVersion, UINT32 wstrProductionVersionLength)
{
    bool bSuccess = false;
    BYTE* rgData = NULL;

    // get the filename of the executable containing the version resource
    TCHAR szFilename[MAX_PATH + 1] = { 0 };
    if (GetModuleFileName(NULL, szFilename, MAX_PATH) == 0)
    {
        Log("GetVersionFromRC(): ERROR: GetModuleFileName failed with '%d'.\n", GetLastError());
        goto error;
    }

    // allocate a block of memory for the version info
    DWORD dummy;
    DWORD dwSize = GetFileVersionInfoSize(szFilename, &dummy);
    if (dwSize == 0)
    {
        Log("GetVersionFromRC(): ERROR: GetFileVersionInfoSize failed with '%d'.\n", GetLastError());
        goto error;
    }

    rgData = new BYTE[dwSize];
    if (rgData == NULL)
    {
        Log("GetVersionFromRC(): ERROR: Unable to allocate memory.\n");
        goto error;
    }

    // load the version info
    if (!GetFileVersionInfo(szFilename, NULL, dwSize, &rgData[0]))
    {
        Log("GetVersionFromRC(): ERROR: GetFileVersionInfo failed with '%d'.\n", GetLastError());
        goto error;
    }

    // get the name and version strings
    LPVOID pvProductVersion = NULL;
    unsigned int iProductVersionLen = 0;

    // "040904b0" is the language ID of the resources
    if (!VerQueryValue(&rgData[0], _T("\\StringFileInfo\\040904b0\\ProductVersion"), &pvProductVersion, &iProductVersionLen))
    {
        Log("GetVersionFromRC(): ERROR: VerQueryValue: Unable to get ProductVersion from the resources.\n");
        goto error;
    }

    if (0 != wcscpy_s(wstrProductVersion, wstrProductionVersionLength, (WCHAR*)pvProductVersion))
    {
        Log("GetVersionFromRC(): ERROR: wcscpy_s failed!\n");
        goto error;
    }

    bSuccess = true;

error:
    SAFE_ARRAY_DELETE(rgData)

        return bSuccess;
}

static void WaitForDebugger()
{
    while (!::IsDebuggerPresent())
        ::Sleep(100);
    ::DebugBreak();
}

int main(int argc, char *argv[], char *envp[])
{
 //   WaitForDebugger();
    int retValue = 0;
    bool bDeviceFound = false;
    int RESTORE_SLEEP_TIME = 1000;  // In milliseconds, default value if RESET parameter not passed
    int RestorTime = 0;
    option::Options opts((option::Descriptor*)usage);

    if (!opts.Parse(&argv[1], argc))
    {
        Log("\nmain(): '%s'", opts.error_msg());
        return -1;
    }

    if (opts[FILELOG])
    {
        if (SetupFileLog(opts.GetValue(FILELOG)))
        {
            LogToFile("--- SESSION START ---\n");
        }
        else
        {
            Log("main(): ERROR: File logging setup failed!\n");
        }
    }

    if (opts[VERSION])
    {
        WCHAR wstrDisManVersion[MAX_PATH];

        if (GetVersionFromRC(wstrDisManVersion, MAX_PATH))
        {
            Log("Aristocrat TouchCoordConfig Version %ws\n", wstrDisManVersion);
        }
    }

    if(opts[HELP] || (argc <= 1))
    {
        printf("\n");
        opts.PrintHelp();

        return 0;
    }

    if ((!opts[DISPLAY]) && (!opts[ARISTANDARD]) && (!opts[WINSTANDARD]) && (!opts[EXACT]) && (!opts[PACKETMODE]) && (!opts[SWITCH]) && (!opts[RESET]))
    {
        if (!opts[VERSION])
        {
            Log("TouchCoordConfig: No commands were specified on the command line.  Use '-h' for more information.\n");
        }

        return 0;
    }

    //
    // Get the time to delay after a reset for all vendor devices.
    // If the call to GetArgument() fails we'll use the default value set at initialization.
    //

    if (opts[RESET] && opts.GetArgument(RESET, RestorTime))
    {
        RESTORE_SLEEP_TIME = RestorTime;
    }

    //
    // KORTEK display firmware settings
    //

    if (!opts[TOVISONLY])
    {
        typedef struct _USBDeviceInfo
        {
            char VID[5];
            char PID[5];
            char InterfaceNo[3];
        } USBDeviceInfo;

        USBDeviceInfo KortekDevices[] = {
            // VID     PID     Interface
            { "2965", "5058", "01" },
            { "2965", "5028", "01" },
            { "2965", "5027", "01" },
            { "2965", "5026", "01" },
            { "2965", "5025", "01" },
            { "2965", "5024", "01" },
            { "2965", "5023", "01" },
            { "2965", "5022", "01" },
            { "2965", "5021", "01" },
            { "2965", "3102", "00" }
        };

        USBDeviceInfo KortekDeviceWildcard = { "2965", "50**", "01" };
        int DIArraySize = sizeof(KortekDevices) / sizeof(KortekDevices[0]);

        for (int device = 0; device <= DIArraySize; ++device)
        {
            Kortek* pkortek = nullptr;

            if ((device == DIArraySize) && (!bDeviceFound))
            {
                //
                // We didn't find any known Kortek devices using our list.  We'll search using wildcard.
                //

                pkortek = Kortek::Connect(KortekDeviceWildcard.VID, KortekDeviceWildcard.PID, KortekDeviceWildcard.InterfaceNo);
            }
            else if (device == DIArraySize)
            {
                //
                // We're at the end of the device list and we've already found at least one device
                //

                continue;
            }
            else
            {
                //
                // We're iterating through our list
                //

                pkortek = Kortek::Connect(KortekDevices[device].VID, KortekDevices[device].PID, KortekDevices[device].InterfaceNo);
            }

            if (pkortek)
            {
                bool madeChanges = false;

                Log("Kortek: Connected to a device.\n");
                bDeviceFound = true;
                pkortek->ReadSettings();

                if (opts[DISPLAY])
                {
                    pkortek->PrintInfo();
                }

                if (opts[RESET])
                {
                    Log("Kortek: Resetting the APROM.\n");
                    if (pkortek->ResetAprom(RESTORE_SLEEP_TIME))
                    {
                        Log("Kortek: Successfully reset the APROM.\n");
                    }
                    else
                    {
                        Log("Kortek: ERROR: Unable to reset the APROM.\n");

                        retValue = 1;
                    }
                }

                if (opts[ARISTANDARD])
                {
                    LogEnabledSetting savedConsoleLogState = GetConsoleLogEnabledSetting();

                    madeChanges = true;

                    //
                    // Print the firmware settings to only the file log before setting the touch
                    // coordinate mode to Aristocrat Standard
                    //

                    SetLogToConsole(Log_Disabled);
                    Log("Kortek: Firmware settings before setting the touch coordinate mode to Aristocrat Standard.\n");
                    pkortek->PrintInfo();
                    SetLogToConsole(savedConsoleLogState);

                    Log("Kortek: Setting the touch coordinate mode to Aristocrat Standard.\n");
                    pkortek->SetAristocratStandard();
                    Log("Kortek: Setting the packet mode to Mouse.\n");
                    pkortek->SetInitialMode(Kortek::DeviceMode::Mouse);

                    if (!pkortek->WriteFlashSetting())
                    {
                        Log("Kortek: ERROR: Failed to write the flash settings trying to set mode to Aristocrat Standard.\n");

                        retValue = 1;
                    }

                    //
                    // Print the firmware settings to only the file log after setting the touch
                    // coordinate mode to Aristocrat Standard
                    //

                    SetLogToConsole(Log_Disabled);
                    Log("Kortek: Firmware settings after setting the touch coordinate mode to Aristocrat Standard.\n");
                    pkortek->PrintInfo();
                    SetLogToConsole(savedConsoleLogState);
                }
                else if (opts[WINSTANDARD])
                {
                    LogEnabledSetting savedConsoleLogState = GetConsoleLogEnabledSetting();

                    madeChanges = true;

                    //
                    // Print the firmware settings to only the file log before settings the touch
                    // coordinate mode to Windows Standard
                    //

                    SetLogToConsole(Log_Disabled);
                    Log("Kortek: Firmware settings before setting the touch coordinate mode to Windows Standard.\n");
                    pkortek->PrintInfo();
                    SetLogToConsole(savedConsoleLogState);

                    Log("Kortek: Setting the touch coordinate mode to Windows Standard.\n");
                    pkortek->SetWindowsStandard();
                    Log("Kortek: Setting the packet mode to Digitizer (10 finger touch).\n");
                    pkortek->SetInitialMode(Kortek::DeviceMode::Digitizer_10);
                    
                    if (!pkortek->WriteFlashSetting())
                    {
                        Log("Kortek: ERROR: Failed to write the flash settings trying to set mode to Windows Standard.\n");

                        retValue = 1;
                    }

                    //
                    // Print the firmware settings to only the file log after setting the touch
                    // coordinate mode to Windows Standard
                    //

                    SetLogToConsole(Log_Disabled);
                    Log("Kortek: Firmware settings after setting the touch coordinate mode to Windows Standard.\n");
                    pkortek->PrintInfo();
                    SetLogToConsole(savedConsoleLogState);
                }
                else if (opts[EXACT])
                {
                    const char* exactSetting = opts.GetValue(EXACT);

                    if (0 == _stricmp(exactSetting, "noflip"))
                    {
                        madeChanges = true;
                        pkortek->SetStandard(Kortek::FlipSetting::NoFlip);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to use 'exact' to set 'NoFlip'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Using The 'exact' setting to set X and Y to normal setting.\n");
                    }
                    else if (0 == _stricmp(exactSetting, "xflip"))
                    {
                        madeChanges = true;
                        pkortek->SetStandard(Kortek::FlipSetting::XFlip);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to use 'exact' to set 'XFlip'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Using the 'exact' setting to flip the X setting.\n");
                    }
                    else if (0 == _stricmp(exactSetting, "yflip"))
                    {
                        madeChanges = true;
                        pkortek->SetStandard(Kortek::FlipSetting::YFlip);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to use 'exact' to set 'YFlip'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Using the 'exact' setting to flip the Y setting.\n");
                    }
                    else if (0 == _stricmp(exactSetting, "xyflip"))
                    {
                        madeChanges = true;
                        pkortek->SetStandard(Kortek::FlipSetting::XYFlip);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to use the 'exact' to set 'XYFlip'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Using the 'exact' setting to flip both the X and Y settings.\n");
                    }
                    else
                    {
                        Log("Kortek: ERROR: The 'exact' parameter that was passed '%s' is unsupported.  Use '-h' for more information.\n", exactSetting);

                        retValue = 1;
                    }
                }

                if (opts[SWITCH])
                {
                    const char* switchSetting = opts.GetValue(SWITCH);

                    if (0 == _stricmp(switchSetting, "on"))
                    {
                        madeChanges = true;
                        pkortek->SetSwitch(Kortek::SwitchSetting::On);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to set the 'switch' setting to 'On'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Succeeded in setting the switch setting to On.\n");
                    }
                    else if (0 == _stricmp(switchSetting, "off"))
                    {
                        madeChanges = true;
                        pkortek->SetSwitch(Kortek::SwitchSetting::Off);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to set the 'switch' setting to 'Off'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Succeeded in setting the switch setting to Off.\n");
                    }
                    else
                    {
                        Log("Kortek: ERROR: The 'switch' parameter that was passed '%s' is unsupported.  Use '-h' for more information.\n", switchSetting);

                        retValue = 1;
                    }
                }

                if (opts[PACKETMODE])
                {
                    const char* exactSetting = opts.GetValue(PACKETMODE);

                    if (0 == _stricmp(exactSetting, "none"))
                    {
                        madeChanges = true;
                        pkortek->SetInitialMode(Kortek::DeviceMode::None);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to set the 'packetmode' to 'None'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Succeeded in setting the initial mode to None.\n");
                    }
                    else if (0 == _stricmp(exactSetting, "mouse"))
                    {
                        madeChanges = true;
                        pkortek->SetInitialMode(Kortek::DeviceMode::Mouse);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to set the 'packetmode' to 'Mouse'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Succeeded in setting the initial mode to Mouse.\n");
                    }
                    else if (0 == _stricmp(exactSetting, "digitizer1"))
                    {
                        madeChanges = true;
                        pkortek->SetInitialMode(Kortek::DeviceMode::Digitizer_1);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to set the 'packetmode' to 'Digitizer 1'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Succeeded in setting the initial mode to Digitizer (1 finger touch).\n");
                    }
                    else if (0 == _stricmp(exactSetting, "digitizer10"))
                    {
                        madeChanges = true;
                        pkortek->SetInitialMode(Kortek::DeviceMode::Digitizer_10);

                        if (!pkortek->WriteFlashSetting())
                        {
                            Log("Kortek: ERROR: Failed to write the flash settings trying to set the 'packetmode' to 'Digitizer 10'.\n");

                            retValue = 1;
                        }

                        Log("Kortek: Succeeded in setting the initial mode to Digitizer (10 finger touch).\n");
                    }
                    else
                    {
                        Log("Kortek: ERROR: Unsupported packet mode specified '%s'.  Use '-h' for more information.\n", exactSetting);

                        retValue = 1;
                    }
                }

                if (madeChanges)
                {
                    Log("Kortek: Ressetting the APROM.\n");
                    pkortek->ResetAprom();
                }

                pkortek->Release();
                pkortek = nullptr;
            }
        }

        if (!bDeviceFound)
        {
            Log("Kortek: Did not find a supported device. (Possibly none connected, or firmware not supported.)\n");
        }

        if (opts[DISPLAY])
        {
            Log("\n");
            Kortek::PrintFirmwareVersionMap();
        }
    }

    //
    // TOVIS display firmware settings
    //

    if (!opts[KORTEKONLY])
    {
        Tovis tovis;
        if (tovis.Connect(RESTORE_SLEEP_TIME))
        {
            Log("Tovis: Connected to a device.\n");

            bDeviceFound = true;

            if (opts[DISPLAY])
            {
                tovis.PrintInfo();
            }

            if (opts[RESET])
            {
                Log("Tovis: Restoring the firmware to its default settings.\n");
                if (!tovis.RestoreDefault())
                {
                    Log("Tovis: ERROR: Unable to restore the firmware to its default settings.  (Possibly not supported)\n");

                    retValue = 1;
                }
                else
                {
                    Log("Tovis: Successfully restored the firmware settings to its default settings.\n");
                }

                if (RESTORE_SLEEP_TIME != 0)
                {
                    Log("Tovis: Delaying for '%d' milliseconds after the RestoreDefault() call.\n", RESTORE_SLEEP_TIME);
                    Sleep(RESTORE_SLEEP_TIME);
                }
            }

            if (opts[ARISTANDARD])
            {
                LogEnabledSetting savedConsoleLogState = GetConsoleLogEnabledSetting();

                //
                // Print the firmware settings to only the file log before setting the touch
                // coordinate mode to Aristocrat Standard
                //

                SetLogToConsole(Log_Disabled);
                Log("Tovis: Firmware settings before setting the touch coordinate mode to Aristocrat Standard.\n");
                tovis.PrintInfo();
                SetLogToConsole(savedConsoleLogState);

                Log("Tovis: Setting the touch coordinate mode to Aristocrat Standard.\n");
                if (!tovis.SetAristocratStandard())
                {
                    Log("Tovis: ERROR: Failed to set the touch coordinate mode to Aristocrat Standard!\n");

                    retValue = 1;
                }
                else
                {
                    Log("Tovis: Successfully set the touch coordinate mode to Aristocrat Standard.\n");
                }

                Log("Tovis: Setting the packet mode to Mouse.\n");
                if (!tovis.SetPacketmode(Tovis::PacketMode_Mouse))
                {
                    Log("Tovis: ERROR: Failed to set the packet mode to Mouse!\n");

                    retValue = 1;
                }
                else
                {
                    Log("Tovis: Successfully set the packet mode to Mouse.\n");
                }

                //
                // Print the firmware settings to only the file log after setting the touch
                // coordinate mode to Aristocrat Standard
                //

                SetLogToConsole(Log_Disabled);
                Log("Tovis: Firmware settings after setting the touch coordinate mode to Aristocrat Standard.\n");
                tovis.PrintInfo();
                SetLogToConsole(savedConsoleLogState);
            }
            else if (opts[WINSTANDARD])
            {
                LogEnabledSetting savedConsoleLogState = Log_Enabled;

                //
                // Print the firmware settings to only the file log before setting the touch
                // coordinate mode to Windows Standard
                //

                SetLogToConsole(Log_Disabled);
                Log("Tovis: Firmware settings before setting the touch coordinate mode to Windows Standard.\n");
                tovis.PrintInfo();
                SetLogToConsole(savedConsoleLogState);

                Log("Tovis: Setting the touch coordinate mode to Windows Standard.\n");
                if (!tovis.SetWindowsStandard())
                {
                    Log("Tovis: ERROR: Failed to set the touch coordinate mode to Windows Standard!\n");

                    retValue = 1;
                }
                else
                {
                    Log("Tovis: Successfully set the touch coordinate mode to Windows Standard.\n");
                }

                Log("Tovis: Setting the packet mode to Digitizer (10 finger touch).\n");
                if (!tovis.SetPacketmode(Tovis::PacketMode_Digitizer))
                {
                    Log("Tovis: ERROR: Failed to set the packet mode to Digitizer (10 finger touch)!\n");

                    retValue = 1;
                }
                else
                {
                    Log("Tovis: Successfully set the packet mode to Digitizer (10 finger touch).\n");
                }
 
                //
                // Print the firmware settings to only the file log after setting the touch
                // coordinate mode to Windows Standard
                //

                SetLogToConsole(Log_Disabled);
                Log("Tovis: Firmware settings after setting the touch coordinate mode to Windows Standard.\n");
                tovis.PrintInfo();
                SetLogToConsole(savedConsoleLogState);
            }
            else if (opts[EXACT])
            {
                Log("Tovis: UNSUPPORTED: The 'exact' setting is not supported for Tovis displays.\n");
            }

            if (opts[SWITCH])
            {
                Log("Tovis: UNSUPPORTED: The 'switch' setting is not supported for Tovis displays.\n");
            }

            if (opts[PACKETMODE])
            {
                const char* exactSetting = opts.GetValue(PACKETMODE);

                if (0 == _stricmp(exactSetting, "none"))
                {
                    Log("Tovis: UNSUPPORTED: Does not support setting packet mode to None!\n");
                }
                else if (0 == _stricmp(exactSetting, "mouse"))
                {
                    Log("Tovis: Setting the packet mode to Mouse.\n");
                    if (!tovis.SetPacketmode(Tovis::PacketMode_Mouse))
                    {
                        Log("Tovis: ERROR: Failed to set the packet mode to Mouse!\n");

                        retValue = 1;
                    }
                    else
                    {
                        Log("Tovis: Successfully set the packet mode to Mouse.\n");
                    }
                }
                else if (0 == _stricmp(exactSetting, "digitizer1"))
                {
                    Log("Tovis: UNSUPPORTED: Does not support setting packet mode to Digitizer (1 finger touch)!\n");
                }
                else if (0 == _stricmp(exactSetting, "digitizer10"))
                {
                    Log("Tovis: Setting the packet mode to Digitizer (10 finger touch).\n");
                    if (!tovis.SetPacketmode(Tovis::PacketMode_Digitizer))
                    {
                        Log("Tovis: ERROR: Failed to set the packet mode to Digitizer (10 finger touch)!\n");

                        retValue = 1;
                    }
                    else
                    {
                        Log("Tovis: Successfully set the packet mode to Digitizer (10 finger touch).\n");
                    }
                }
                else
                {
                    Log("Tovis: ERROR: Unsupported packet mode specified - '%s'.  Use '-h' for more information.\n", exactSetting);

                    retValue = 1;
                }
            }
        }
        else
        {
            Log("Tovis: Did not find a supported device. (Possibly none connected, or firmware not supported.)\n");
        }

        if (opts[DISPLAY])
        {
            Log("\n");
            Tovis::PrintFirmwareVersionMap();
        }
    }

    if (!bDeviceFound)
    {
        Log("TouchCoordConfig: Did not find a supported device from any vendor.\n");
    }

    Log("TouchCoordConfig: Return Code = '%d'.\n", retValue);

    if (opts[FILELOG])
    {
        LogToFile("--- SESSION END ---\n\n");
    }

    return retValue;
}