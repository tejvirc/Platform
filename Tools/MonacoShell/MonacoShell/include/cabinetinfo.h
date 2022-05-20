#pragma once

#include "platform.h"
#include "logger.h"
#include <string>
#include "ProcessHelper.h"


class CabinetInfo
{
public:
    CabinetInfo()
    {
        Log(LogTarget::File, "CabinetInfo(): Constructing.\n");
    }

    ~CabinetInfo()
    {
    }

    void RemoveCachedData()
    {
        Log(LogTarget::File, "CabinetInfo():RemoveCachedData()\n");

        _cabinetInfoString.clear();
        _cabinetType = 0;
    }

    //
    // Returns an integer that represents the type of cabinet we are running on.
    // If this method returns '-1' then there was an error with executing the external
    // application.
    //
    // Return Values for the various cabinets:
    //   public enum CabinetType from CabinetDisplayConfigurator application
    // 
    //   0  = Unknown                           (Unknown)
    //   1  = Helix Single 23'                  (HelixSingle23)
    //   2  = Helix Dual 23'                    (HelixDual23)
    //   3  = Helix Single 27'                  (HelixSingle27)
    //   4  = Helix Plus                        (HelixPlus)
    //   5  = Helix X                           (HelixX)
    //   6  = Helix XT                          (HelixXT)
    //   7  = Flame                             (Flame)
    //   8  = EdgeX                             (EdgeX)
    //   9  = MarsX                             (MarsX)
    //   10 = Bartop                            (Bartop)
    //   11 = Helix Single 23' with Bartop VBD  (HelixSingle23WithBartopVBD)
    //   12 = Helix Dual 23' with Bartop VBD    (HelixDual23WithBartopVBD)
    //   13 = Helix Single 27' with Bartop VBD  (HelixSingle27WithBartopVBD)
    //   14 = Helix Plus with Bartop VBD        (HelixPlusWithBartopVBD)
    //   15 = MarsX with Bartop VBD             (MarsXWithBartopVBD)
    //

    int GetCabinetType()
    {
        Log(LogTarget::File, "CabinetInfo():GetCabinetType()\n");

        std::string displayConfiguratorApp = _cabinetInfoApp;

        //
        // First check our cached value and use that if we have it
        //

        if (_cabinetType != 0)
        {
            Log(LogTarget::File, "CabinetInfo():GetCabinetType(): [Cached]\n");
        }
        else
        {
            Log(LogTarget::File, "CabinetInfo():GetCabinetType(): [NOT Cached]\n");

            displayConfiguratorApp.append(" -s -c");
            _cabinetType = ExecuteConfigApp(displayConfiguratorApp.c_str(), _cabinetInfoAppPath);
        }

        Log(LogTarget::File, "CabinetInfo():GetCabinetType(): Complete. Returning Cabinet Type '%d'.\n", _cabinetType);

        return _cabinetType;
    }

    std::string GetCabinetInfoString()
    {
        Log(LogTarget::File, "CabinetInfo():GetCabinetInfoString()\n");

        std::string displayConfiguratorApp = _cabinetInfoApp;

        if (!_cabinetInfoString.empty())
        {
            Log(LogTarget::File, "CabinetInfo():GetCabinetInfoString(): [Cached].\n");
        }
        else
        {
            Log(LogTarget::File, "CabinetInfo():GetCabinetInfoString(): [NOT Cached].\n");

            displayConfiguratorApp.append(" -p");
            ProcessHelper::ExcecuteProcess(displayConfiguratorApp, &_cabinetInfoString);
            _cabinetInfoString.erase(std::remove(_cabinetInfoString.begin(), _cabinetInfoString.end(), '\r'), _cabinetInfoString.end());
        }

        Log(LogTarget::File, "CabinetInfo():GetCabinetInfoString(): Complete.  Returning Cabinet json:\n%s", _cabinetInfoString.c_str());

        return _cabinetInfoString;
    }

    Json::Value GetCabinetInfo()
    {
        Log(LogTarget::File, "CabinetInfo():GetCabinetInfo()\n");

        std::string json = GetCabinetInfoString();
        Json::CharReaderBuilder builder;
        Json::Value value;
        std::string errs;
        auto reader = builder.newCharReader();
        if (!reader->parse(json.data(), json.data() + json.size(), &value, &errs))
        {
            Log(LogTarget::File, "CabinetInfo():GetCabinetInfo(): ERROR parsing display info json '%s' '%s'.\n", json.c_str(), errs.c_str());
        }

        Log(LogTarget::File, "CabinetInfo():GetCabinetInfo(): Complete.\n");

        return value;
    }

    void SetupDisplays()
    {
        Log(LogTarget::File, "CabinetInfo():SetupDisplays()\n");

        std::string displayConfiguratorApp = _cabinetInfoApp;

        displayConfiguratorApp.append(" -s");
        ExecuteConfigApp(displayConfiguratorApp.c_str(), _cabinetInfoAppPath);

        Log(LogTarget::File, "CabinetInfo():SetupDisplays(): Complete.\n");
    }

private:
    const char* _cabinetInfoAppPath = "C:\\Aristocrat-VLT-Tools\\CabinetDisplayConfigurator";
    const char* _cabinetInfoApp     = "C:\\Aristocrat-VLT-Tools\\CabinetDisplayConfigurator\\CabinetDisplayConfigurator.exe";
    int         _cabinetType        = 0;
    std::string _cabinetInfoString;

    int ExecuteConfigApp(const char* application, const char* appPath)
    {
        DWORD exitCode = 0;
        STARTUPINFOA startupInfo = { 0 };
        PROCESS_INFORMATION piProcInfo = { 0 };

        Log(LogTarget::File, "CabinetInfo():ExecuteConfigApp(%s, %s)\n", application, appPath);

        if (::CreateProcessA(NULL,
            (char*)application,
            NULL,
            NULL,
            FALSE,
            CREATE_NO_WINDOW,
            NULL,
            appPath,
            &startupInfo,
            &piProcInfo))
        {
            ::WaitForSingleObject(piProcInfo.hProcess, INFINITE);
            ::GetExitCodeProcess(piProcInfo.hProcess, &exitCode);

            CloseHandle(piProcInfo.hProcess);
            CloseHandle(piProcInfo.hThread);
        }
        else
        {
            Log(LogTarget::File, "CabinetInfo():ExecuteConfigApp(): ERROR - CreateProcessA failed.  Last Error Code: '%d'.\n", GetLastError());

            exitCode = -1;
        }

        Log(LogTarget::File, "CabinetInfo():ExecuteConfigApp(): Complete.  Returning '%d'.\n", exitCode);

        return exitCode;
    }
};