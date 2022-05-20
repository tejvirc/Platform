//#define TEST_FOR_MEMORY_LEAKS

#include <platform.h>
#include <timer.h>
#include <algorithm>
#include <cctype>
#include <string>
#include "logger.h"
#include "shellmanager.h"
#include "shellwindow.h"
#include "MonitorInfo.h"
#include "optionparser.h"
#include "cabinetinfo.h"
#include "displaychangednotification.h"
#include "MiniDump.h"

enum  optionIndex { UNKNOWN, HELP, START, STOP};

using namespace aristocrat;
using namespace aristocrat::option;

const option::Descriptor usage[] =
{
    { UNKNOWN,      "",  "",      Arg::None,   "USAGE: Shell [options]\n\n"
    "Options:" },
    { HELP,         "h", "help",  Arg::None,    "-h, --help   Print usage and exit." },
    { START,        "s", "start", Arg::String,  "-s, --start  <name> start an application." },
    { STOP,         "k", "stop",  Arg::String,  "-k, --stop   <name> stop an application." },
    { UNKNOWN,      "",  "",      Arg::None,    "\nExamples:\n"
    "  MonacoShell -s platform \n"
    "  MonacoShell -k tester \n" },
    { 0, 0, 0, option::Arg::None, 0 }
};

int global_num_touches = -1;

//
// Retrieves the version number from the resource file
//

bool GetVersionFromRC(CHAR* strProductVersion, UINT32 strProductionVersionLength)
{
    bool bSuccess = false;
    BYTE* rgData = NULL;
    DWORD dummy = 0;
    DWORD dwSize = 0;
    LPVOID pvProductVersion = NULL;
    unsigned int iProductVersionLen = 0;

    //
    // get the filename of the executable containing the version resource
    //

    CHAR szFilename[MAX_PATH + 1] = { 0 };
    if (GetModuleFileName(NULL, szFilename, MAX_PATH) == 0)
    {
        printf("ERROR: GetVersionFromRC(): GetModuleFileName failed with %d\n", GetLastError());
        goto error;
    }

    //
    // allocate a block of memory for the version info
    //

    dwSize = GetFileVersionInfoSize(szFilename, &dummy);
    if (dwSize == 0)
    {
        printf("ERROR: GetVersionFromRC(): GetFileVersionInfoSize failed with %d\n", GetLastError());
        goto error;
    }

    rgData = new BYTE[dwSize];
    if (rgData == NULL)
    {
        printf("ERROR: GetVersionFromRC(): Unable to allocate memory.\n");
        goto error;
    }

    //
    // load the version info
    //

    if (!GetFileVersionInfo(szFilename, NULL, dwSize, &rgData[0]))
    {
        printf("ERROR: GetVersionFromRC(): GetFileVersionInfo failed with %d\n", GetLastError());
        goto error;
    }

    //
    // get the name and version strings
    //
    // "040904b0" is the language ID of the resources
    //

    if (!VerQueryValue(&rgData[0], "\\StringFileInfo\\040904b0\\ProductVersion", &pvProductVersion, &iProductVersionLen))
    {
        printf("ERROR: GetVersionFromRC(): VerQueryValue: Unable to get ProductVersion from the resources.\n");
        goto error;
    }

    if (0 != strcpy_s(strProductVersion, strProductionVersionLength, (CHAR*)pvProductVersion))
    {
        printf("ERROR: GetVersionFromRC(): strcpy_s failed!\n");
        goto error;
    }

    bSuccess = true;

error:
    if (NULL != rgData)
    {
        delete[] rgData;
        rgData = NULL;
    }

    return bSuccess;
}

static std::string ToLower(const std::string& original)
{
    std::string data = original;

    std::transform(data.begin(), data.end(), data.begin(),
        [](unsigned char c) { return std::tolower(c); });

    return data;
}

static std::string GetConfigFilePath(Json::Value& value, CabinetInfo* pCabinetInfoObj)
{
    std::map<std::string, std::string> roleToDisplayMap;
    std::string dir1;
    MonitorInfo mInfo(pCabinetInfoObj);
    auto displayInfo = mInfo.GetDisplayInfoByIndex(mInfo.GetDisplayInfoByRole("main"));
    auto cabinetTypeName = value.get("CabinetType", "").asString();
    std::string landScapeOrPortrait;

    if (nullptr != displayInfo)
    {
        if (displayInfo->devMode.dmDisplayOrientation == DMDO_90 || displayInfo->devMode.dmDisplayOrientation == DMDO_270)
        {
            landScapeOrPortrait = displayInfo->devMode.dmPelsWidth > displayInfo->devMode.dmPelsHeight ? "Portrait" : "Landscape";
        }
        else
        {
            landScapeOrPortrait = displayInfo->devMode.dmPelsHeight > displayInfo->devMode.dmPelsWidth ? "Portrait" : "Landscape";
        }
    }
    else
    {
        Log(LogTarget::File, "GetConfigFilePath(): displayInfo is NULL.\n");

        //
        // We don't know if we should load the PORTRAIT or LANDSCAPE configuration.  We'll default to PORTRAIT,
        // but this could cause the Shell windows to be setup incorrectly on a LANDSCAPE EGM/Rig.
        //

        landScapeOrPortrait = "Portrait";
    }

    //
    //Perform initial Cabinet Display Setup
    //

    auto pathsToTry = {
        cabinetTypeName,
        landScapeOrPortrait
    };

    for (auto&& path : pathsToTry)
    {
        if (path.empty())
            continue;

        std::string configFile = "Monaco_Configurations\\" + path + "\\config.json";
        if (PathFileExistsA(configFile.c_str()))
        {
            Log(LogTarget::File, "GetConfigFilePath(): Loading Configuration File: '%s'.\n", configFile.c_str());

            return configFile;
        }
    }

    Log(LogTarget::File, "GetConfigFilePath(): Could not find configuration file path. Loading DEFAULT Configuration File: 'Monaco_Configurations\\config.json'.\n");

    return "Monaco_Configurations\\config.json";
}

static std::string GetCabinetConfigString(Json::Value& value)
{
    auto cabinetType = value.get("CabinetType", "None").asString();
    auto&& displays = value["Screens"];
    std::vector<std::string> vectorData { cabinetType, "None", "None", "None", "None" };
    std::map<std::string, int> roleToIndex{ {"main", 1},{"top", 2},{"vbd", 3},{"topper", 4}, {"", -1} };

    for (int i = 0; i < static_cast<int>(displays.size()); i++)
    {
        auto&& display = displays[i];
        auto index = roleToIndex[ToLower(display.get("Role", "").asString())];
        if (index < 1 || index >= vectorData.size()) continue;
        auto xRes = display.get("XRes", 0).asInt();
        auto yRes = display.get("YRes", 0).asInt();
        auto name = display.get("Name", 0).asString();
        auto rotation = display.get("Rotation", 0).asInt();
        std::replace(name.begin(), name.end(), ',', '_');
        vectorData[index] = ((rotation == 90 || rotation == 270) ? std::string("P") : std::string("L")) +
            ((xRes > 1920 || yRes > 1920) ? "4K" : "2K") + name;
    }

    std::string ret;
    for (auto&& data : vectorData) 
    {
        ret.append(data);
        ret.append(",");
    }

    if (!ret.empty())
        ret.resize(ret.size() - 1);

    return ret;
}

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                     _In_opt_ HINSTANCE hPrevInstance,
                     _In_ LPWSTR    lpCmdLine,
                     _In_ int       nCmdShow)
{
    //InitializeCRTDebug();

    MiniDumper minidump("MonacoShell", "", "monacoshell.log", 32);

    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);
    CHAR strVersion[MAX_PATH] = { 0 };
    #ifdef TEST_FOR_MEMORY_LEAKS
    _CrtMemState state;
    _CrtMemCheckpoint(&state);
    #endif

    {
        //
        // Option parser
        //

        int argc = 0;
        auto argv = CommandLineToArgv(&argc, true);
        option::Options opts((option::Descriptor*)usage);
        if (!opts.Parse(argv, argc))
        {
            RedirectIOToConsole();
            printf("%s\n", opts.error_msg());

            return -1;    
        }

        if (GetVersionFromRC(strVersion, MAX_PATH))
        {
            printf("Monaco Shell %s\n\n", strVersion);
        }

        if (opts.count() > 0)
        {
            RedirectIOToConsole();

            if (opts[HELP])
            {
                opts.PrintHelp();
            }

            if (opts[START])
            {
                const char* appname = opts.GetValue(START);
                Command cmd = {0};
                cmd.cmdId = 1;
                strcpy(cmd.data,appname);
                 SendMessage(cmd);
            }

            if (opts[STOP])
            {
                const char* appname = opts.GetValue(STOP);
                Command cmd = {0};
                cmd.cmdId = 0;
                strcpy(cmd.data,appname);
                SendMessage(cmd);
            }

            return -1;
        }
    }

    HANDLE _hRunOnceMutex = OpenMutexA(MUTEX_ALL_ACCESS, 0, "MonacoShell");
    if (_hRunOnceMutex == 0) // No instance exists
    {
        _hRunOnceMutex = CreateMutexA(0, 0, "MonacoShell");
    }
    else
    {
        ReleaseMutex(_hRunOnceMutex);

        return -1; // Already running
    }

    aristocrat::SwitchToExecutablesDirectory(hInstance);

    if (!SetupFileLog())
    {
        Log(LogTarget::Console, "main(): SetupFileLog() failed!\n");
    }

    Log(LogTarget::File, "--- SESSION START ---\n");
    Log(LogTarget::File, "Monaco Shell %s\n", strVersion);

    DisplayChangeNotification dcn;

    //
    // Determine, based on the cabinet type, what configuration files to load
    //

    CabinetInfo cabinetInfo;
    int cabinetType = 0;
    int totalSleepTime = 0;

    do
    {
        cabinetInfo.RemoveCachedData();
        cabinetType = cabinetInfo.GetCabinetType();
        if (cabinetType == 0)
        {
            //
            // If we received '0' as the cabinet type it means we do not yet know.
            // We will give Windows some more time to settle and try again.  If this takes much
            // too long (40 seconds) we will exit.
            //

            if (totalSleepTime >= 40000)
            {
                Log(LogTarget::File, "wWinMain(): Cabinet Detection: Exceeded Maximum Sleep Time of 40000.  Returning cabinetType - '%d'.\n", cabinetType);

                break;
            }
            else
            {
                Log(LogTarget::File, "wWinMain(): Cabinet Detection: cabinetType = '0'.  Sleeping for 1 seconds before trying to get the cabinet type again.\n");

                totalSleepTime += 1000;
                ::Sleep(1000);
            }
        }
    } while (cabinetType == 0);

    Log(LogTarget::File, "wWinMain(): Cabinet Detection: Total Sleep Time - '%d'.\n", totalSleepTime);

    cabinetInfo.SetupDisplays();
    auto cabinetJsonData = cabinetInfo.GetCabinetInfo();
    auto configFile = GetConfigFilePath(cabinetJsonData, &cabinetInfo);
    auto manager = ShellManager::LoadFromConfiguration(configFile.c_str(), GetCabinetConfigString(cabinetJsonData), &cabinetInfo);
    if (manager == nullptr)
        return -1;

    manager->Initialize();

    aristocrat::timer timer;
    int counter = 0;
    double onDisplayChangedElapsed = 0;
    bool onDisplayChanged = false;

    dcn.OnDisplayChanged(
        [&] {
            Log(LogTarget::File, "wWinMain():dcn.OnDisplayChanged(): Executing.\n");

            manager->hide_all_displays();
            onDisplayChanged = true;
            onDisplayChangedElapsed = 0;
        });

    //dcn.RunMessageLoopOnce(); // pump any displaychange events (ignore wm_quit)

#define DISPLAY_CHANGED_ELAPSED_TIMER   3000.0

    while (ShellWindow::RunMessageLoopOnce())
    {
        double elapsedTime = timer.getElapsedTimeInMilliSec();

        onDisplayChangedElapsed += elapsedTime;
        manager->Update(elapsedTime);
        ::Sleep(10);

        int numTouches = GetSystemMetrics(SM_MAXIMUMTOUCHES);
        if (numTouches != global_num_touches)
        {
            Log(LogTarget::File, "wWinMain(): Touch Points Changed: numTouches = '%d'. Packet Mode = '%s'.\n", numTouches, numTouches > 0 ? "Digitizer" : "Mouse");
            global_num_touches = numTouches;
        }

        //
        // Check to see if we got a Display Changed message after waiting a certain
        // amount of time.  This to help with calling SetupDisplays() multiple times
        // if we get more than one of those messages quickly.
        //
        // Will default the timer to DISPLAY_CHANGED_ELAPSED_TIMER milliseconds and
        // evalute logs to see if it needs to be adjusted.
        //

        if (onDisplayChanged && (onDisplayChangedElapsed >= DISPLAY_CHANGED_ELAPSED_TIMER))
        {
            Log(LogTarget::File, "wWinMain(): Received one or more Display Changed messages.  Waited '%.1f' milliseconds.  Re-setting up the displays.\n", DISPLAY_CHANGED_ELAPSED_TIMER);

            onDisplayChanged = false;
            cabinetInfo.RemoveCachedData();
            cabinetInfo.SetupDisplays();
            cabinetJsonData = cabinetInfo.GetCabinetInfo();
            configFile = GetConfigFilePath(cabinetJsonData, &cabinetInfo);
            manager->reload_gui(configFile.c_str(), GetCabinetConfigString(cabinetJsonData));
        }

#ifdef TEST_FOR_MEMORY_LEAKS
        counter++;
        if (counter > 100) break;
#endif
    }

    manager->Release();
    ReferenceMap::Destroy();
    if (_hRunOnceMutex)
    {
        ReleaseMutex(_hRunOnceMutex);
    }

    aristocrat::MultiByte2WideCharacterString(nullptr);
    aristocrat::WideCharacter2MultiByteString(nullptr);

#ifdef TEST_FOR_MEMORY_LEAKS
    _CrtMemDumpAllObjectsSince(&state);
    for (int i = 0; i < 1; ++i)
        ::Sleep(500);
#endif

    Log(LogTarget::File, "wWinMain(): Complete.  Returning '0'.\n");
    Log(LogTarget::File, "--- SESSION END ---\n\n");

    return (int) 0;
}