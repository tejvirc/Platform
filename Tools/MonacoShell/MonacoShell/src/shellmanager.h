#pragma once

#include <vector>
#include <algorithm>
#include <regex>
#include <json/json.h>
#include <fstream>
#include "logger.h"
#include "application.h"
#include "gui.h"
#include "streammanager.h"
#include "referencemap.h"
#include "cabinetinfo.h"

struct Command
{
    static constexpr DWORD DWDATA() { return 0x1000000; }
    static constexpr DWORD STOP()   { return 0x0; }
    static constexpr DWORD START()  { return 0x1; }
    int cmdId;
    char data[508];
};

void SendMessage(const Command &cmd)
{
    HWND hwnd = FindWindow("ShellWindow", NULL);
    if (hwnd == NULL)
    {
        printf("FindWindow failed to find window of Class 'ShellWindow'\n");
    }

    COPYDATASTRUCT cds = { 0 };
    cds.dwData = Command::DWDATA();
    cds.cbData = sizeof(Command);
    cds.lpData = (PVOID)&cmd;
    SendMessage(hwnd, WM_COPYDATA, (WPARAM)hwnd, (LPARAM)(LPVOID)&cds);
    printf("Command Sent.\n");
}

class ShellManager : public IShellContext, public IDataReceiver
{
    MONACO_BEGIN_INTERFACE_MAP(ShellManager)
        MONACO_INTERFACE_ENTRY(IShellContext)
        MONACO_INTERFACE_ENTRY(IDataReceiver)
    MONACO_END_INTERFACE_MAP

    ShellManager(CabinetInfo* pCabinetInfoObj) :
        _pCabinetInfo(pCabinetInfoObj)
    {
        _monInfo = new MonitorInfo(pCabinetInfoObj);
        if (_monInfo == nullptr)
        {
            Log(LogTarget::File, "ShellManager():ShellManager(): Unable to allocate memory for new MonitorInfo().\n");
        }

        _streams = StreamManager::Create();
    }

    virtual ~ShellManager()
    {
        _pCabinetInfo = nullptr;
        Reset();
    }

    void Reset()
    {
        if (_monInfo != nullptr)
        {
            delete _monInfo;
            _monInfo = nullptr;
        }

        if (_gui != nullptr)
        {
            _gui->Release();
            _gui = nullptr;
        }

        std::for_each(_applications.begin(), _applications.end(), [](auto a) { a->Release(); });

        if (_streams != nullptr)
        {
            _streams->Release();
            _streams = nullptr;
        }
    }

public:
    void Release()
    {
        delete this;
    }

    static bool FilterNodes(Json::Value& root, const std::string& dataToMatch) 
    {
        if (root.isArray())
        {
            for (Json::ArrayIndex i = 0; i < root.size();)
            {
                if (FilterNodes(root[i], dataToMatch))
                {
                    if (root.removeIndex(i, nullptr))
                    {
                        continue;
                    }
                }
                i++;
            }
        }
        else if (root.isObject())
        {
            auto members = root.getMemberNames();
            auto filterStr = root.get("filter", "").asString();
            if (!filterStr.empty())
            {
                std::regex filter(filterStr);
                std::cmatch m;
                if (!std::regex_match(dataToMatch.c_str(), m, filter))
                {
                    return true;
                }
            }

            for (auto&& member : members)
            {
                if (FilterNodes(root[member], dataToMatch))
                {
                    root.removeMember(member);
                }
            }
        }

        return false;
    }

    static ShellManager* LoadFromConfiguration(const char* filename, const std::string& cabinetConfigString, CabinetInfo* pCabinetInfoObj)
    {
        Log(LogTarget::File, "ShellManager():LoadFromConfiguration()\n");

        ShellManager*           manager = new ShellManager(pCabinetInfoObj);
        std::string             errs;
        std::ifstream           is(filename, std::ifstream::in);
        Json::Value             root;
        Json::CharReaderBuilder rbuilder;

        rbuilder["collectComments"] = false;
        rbuilder["allowSingleQuotes"] = true;

        if (is)
        {
            std::string errs;

            if (!Json::parseFromStream(rbuilder, is, &root, &errs))
            {
                Log(LogTarget::Console, "ShellManager():LoadFromConfiguration(): ERROR Loading configuration '%s'.\n", filename);
                Log(LogTarget::Console, "    ERROR String: '%s'.", errs.c_str());
                Log(LogTarget::File, "ShellManager():LoadFromConfiguration(): ERROR Loading configuration file '%s'.", filename);
                Log(LogTarget::File, "ShellManager():LoadFromConfiguration(): ERROR String: '%s'.", errs.c_str());

                delete manager;
                manager = nullptr;
            }

            if (manager != nullptr)
            {
                FilterNodes(root, cabinetConfigString);
                manager->load_json(root);
            }
        }
        else 
        {
            Log(LogTarget::File, "ShellManager():LoadFromConfiguration(): Failed opening file: '%s'.", filename);

            //
            // file not found
            //

            delete manager;
            manager = nullptr;
        }

        Log(LogTarget::File, "ShellManager():LoadFromConfiguration(): Complete.\n");

        return manager;
    }

    void hide_all_displays()
    {
        Log(LogTarget::File, "ShellManager():hide_all_displays()\n");

        _gui->Hide();
    }

    bool have_devmodes_changed()
    {
        MonitorInfo mi(nullptr);
        return !mi.Compare(_monInfo);
    }

    //
    // Reload a shellmanager, retaining applications and states of current manager
    //

    bool reload_gui(const char* filename, const std::string& cabinetConfigString)
    {
        auto tmpManager = LoadFromConfiguration(filename, cabinetConfigString, _pCabinetInfo);
        if (tmpManager != nullptr)
        {
            _gui->Release();
            _gui = tmpManager->_gui;
            Initialize();

            if (_monInfo != nullptr)
                delete _monInfo;

            _monInfo = tmpManager->_monInfo;
            tmpManager->_monInfo = nullptr;
            tmpManager->_gui = nullptr;
            tmpManager->Release();

            return true;
        }

        return false;
    }

    void load_json(const Json::Value& root)
    {
        auto config     = root["config"];

        //
        // config node
        //

        _title  = config.get("title","no title").asCString();

        ReferenceMap::from_json(config);

        //
        // load gui
        //

        _gui = GuiManager::from_json(config["ui"], _pCabinetInfo);

        //
        // applications node
        //

        auto apps = config["applications"];

        for (auto a : apps)
        {
            _applications.push_back(Application::from_json(a));
        }
    }

    void Initialize()
    {
        if (_gui != nullptr) 
            _gui->Initialize(this);
    }

    void Update(double elapsedTimeInMs)
    {
        for (auto a : _applications)
        {
            bool cont = a->Update(elapsedTimeInMs);

            if (!cont)
                break;
        }

        if (_gui)
        {
            _gui->Update(elapsedTimeInMs);
        }
    }

    void StartApplication(const char* applicationName) 
    {
        for (auto a : _applications)
        {
            if (aristocrat::icasecmp(a->name(),applicationName))
            {
                Log(LogTarget::File, "ShellManager():StartApplication(): Starting '%s'.\n", a->name());

                a->start();
            }
        }
    }

    void StopApplication(const char* applicationName)
    {
        for (auto a : _applications)
        {
            if (aristocrat::icasecmp(a->name(),applicationName))
            {
                Log(LogTarget::File, "ShellManager():StopApplication(): Stopping '%s'.\n", a->name());

                a->stop();
            }
        }
    }

    void OnData(DWORD id, const char* data, size_t length)
    {
        if (id == Command::DWDATA())
        {
            Command* command = static_cast<Command*>((LPVOID)data);
            if (command->cmdId == Command::START())
            {
                StartApplication(command->data);
            }
            else if (command->cmdId == Command::STOP())
            {
                StopApplication(command->data);
            }
        }

        if (_gui)
        {
            _gui->OnData(id, data, length);
        }
    }

    GuiManager* Gui()
    {
        return _gui;
    }

private:
    GuiManager*                 _gui = nullptr;
    std::vector<Application*>   _applications;
    std::string                 _title;
    StreamManager*              _streams;
    CabinetInfo*                _pCabinetInfo;
    MonitorInfo*                _monInfo = nullptr;
};
