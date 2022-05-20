#pragma once

#include <platform.h>
#include <winutil.h>
#include <fstream>
#include <iostream>
#include <json/json.h>
#include <stringutil.h>
#include <timer.h>
#include <limits>
#include "logger.h"
#include "consolecapture.h"
#include "processlist.h"
#include <circularbuffer.h>
#include "streammanager.h"

class Application
{
    enum OutputType
    {
        Log,
        Capture
    };

    enum eProcessType
    {
        RunOnce,
        Monitor,
        Interval,
        Manual
    };

    enum eProcessState
    {
        Abort = -4,
        Retry = -3,
        FailedWithReturnCode = -2,
        Failed = -1,
        Initial = 0,
        Running = 1,
        Completed = 2,
        Delaying = 3,
        Stopped = 4,
        Idle = 5
    };

    static eProcessType ProcessType(std::string type)
    {
        if (aristocrat::icasecmp(type, "runonce"))
            return RunOnce;

        if (aristocrat::icasecmp(type, "monitor"))
            return Monitor;

        if (aristocrat::icasecmp(type, "interval"))
            return Interval;

        if (aristocrat::icasecmp(type, "manual"))
            return Manual;

        return Monitor;
    }

    static const int UndefinedSuccessCode = std::numeric_limits<int>::min();

public:
    //
    // Factory
    //

    static Application* from_json(const Json::Value& v)
    {
        Application app;
        app._title              = v.get("title","no title").asCString();
        app._executable         = v.get("executable","").asCString();
        app._workingDirectory   = v.get("workingDirectory","").asCString();
        app._type               = ProcessType(v.get("type","monitor").asCString());
        app._hidden             = v.get("hidden",false).asBool();
        app._waitForCompletion  = v.get("wait",false).asBool();
        app._do_capture         = v.get("capture",false).asBool();
        app._minimizeAtStart    = v.get("minimize",false).asBool();
        app._runAsAdmin         = v.get("admin",false).asBool();
        app._singleton          = v.get("singleton",false).asBool();
        app._arguments          = v.get("arguments","").asCString();
        app._retries            = v.get("retry",0).asInt();
        app._restartDelay       = v.get("restartDelay", 5000).asUInt();
        app._intervalDelay      = v.get("intervalDelay", 5000).asUInt();
        app._successCode        = v.get("successCode", UndefinedSuccessCode).asInt();
        app._logging            = v.get("logging",true).asBool();
        app._stream             = v.get("stream","").asCString();
        app._clearstreamonrun   = v.get("clearStreamOnRun",false).asBool();
        app._enabled            = v.get("enabled",true).asBool();

        ::Log(LogTarget::File, "Application():from_json(): [%s]\n", app._title.c_str());
        ::Log(LogTarget::File, "Application():from_json():     executable = '%s'\n", app._executable.c_str());
        ::Log(LogTarget::File, "Application():from_json():     workingDir = '%s'\n", app._workingDirectory.c_str());
        ::Log(LogTarget::File, "Application():from_json():     arguments  = '%s'\n", app._arguments.c_str());
        ::Log(LogTarget::File, "Application():from_json():     type = '%d' | hidden = '%s' | waitForComplete = '%s' | capture = '%s'\n", app._type, app._hidden ? "true" : "false", app._waitForCompletion ? "true" : "false", app._do_capture ? "true" : "false");
        ::Log(LogTarget::File, "Application():from_json():     minimize = '%s' | admin = '%s' | singleton = '%s' | retries = '%d'\n", app._minimizeAtStart ? "true" : "false", app._runAsAdmin ? "true" : "false", app._singleton ? "true" : "false", app._retries);
        ::Log(LogTarget::File, "Application():from_json():     restartDelay = '%d' | intervalDelay = '%d' | successCode = '%d'\n", app._restartDelay, app._intervalDelay, app._successCode);
        ::Log(LogTarget::File, "Application():from_json():     logging = '%s' | stream = '%s' | clearStream = '%s' | enabled = '%s'\n", app._logging ? "true" : "false", app._stream.c_str(), app._clearstreamonrun ? "true" : "false", app._enabled ? "true" : "false");

        if (app._enabled == false)
        {
            app._state = Idle;
        }

        if (!app._stream.empty())
        {
            StreamManager::Instance()->AddStream(app._stream);
        }

        if (app._type == Manual)
        {
            app._state = Idle;
        }

        return new Application{app};
    }

private:
    Application() : _state(Initial)
    {
    }

    virtual ~Application()
    {
    }

    void Output(OutputType type, const char* szValue, ...)
    {
        if (type == Log && _logging == false)
            return;

        char szBuffer[4096] = {};
        va_list argptr;
        va_start(argptr, szValue);

        if (-1 == (vsnprintf(szBuffer, 4095, szValue, argptr)))
        {
            //
            // whatever
            //
        }

        va_end(argptr);
        StreamManager::Instance()->Add(_stream.c_str(), szBuffer);
    }

    bool StartProcess()
    {
        ProcessList processList;

        // attach if we are in singleton mode
        if (_singleton && processList.Exist(_executable))
        {
            // kill if we are in runonce
            if (_type == RunOnce)
            {
                ::Log(LogTarget::File, "Application():StartProcess(): Starting '%s'.\n", _title.c_str());
                auto p = processList.GetInfo(_executable);
                HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, p->th32ProcessID);
                if (hProcess)
                {
                    Output(Log, "[%s] Process already running, Terminating it.", _title.c_str());
                    ::Log(LogTarget::File, "Application():StartProcess(): [%s] Process already running, Terminating it.\n", _title.c_str());

                    TerminateProcess(hProcess, -1);
                    ::CloseHandle(hProcess);
                }
            }
            else if (_type == Monitor)
            {
                auto p = processList.GetInfo(_executable);
                HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, p->th32ProcessID);
                if (hProcess)
                {
                    Output(Log, "[%s] Process already running, Attaching.", _title.c_str());
                    ::Log(LogTarget::File, "Application():StartProcess(): [%s] Process already running, Attaching.\n", _title.c_str());

                    _hProcess = hProcess;
                    _state = Running;

                    return true;
                }
            }
        }

        if (_executable.empty())
        {
            Output(Log, "[%s] Executable Path is not set.", _title.c_str());
            ::Log(LogTarget::File, "Application():StartProcess(): [%s] Executable Path is not set.\n", _title.c_str());

            _state = Failed;
        }

        if (!::PathFileExistsA(_executable.c_str()) && !::PathIsDirectoryA(_executable.c_str()))
        {
            Output(Log, "[%s] Executable %s not found.", _title.c_str(), _executable.c_str());
            ::Log(LogTarget::File, "Application():StartProcess(): [%s] Executable %s not found.\n", _title.c_str(), _executable.c_str());

            _state = Failed;
        }

        WORD dwShowWindowStyle = SW_SHOW;
        if (_hidden)
        {
            dwShowWindowStyle = SW_HIDE;
        }

        if (_minimizeAtStart)
        {
            dwShowWindowStyle |= SW_SHOWMINNOACTIVE;
        }

        Output(Log, "[%s] Starting.", _title.c_str());
        ::Log(LogTarget::File, "Application():StartProcess(): Starting '%s'.\n", name());

        if (_runAsAdmin) // Need to use ShellExecute to start in elevated mode
        {
            SHELLEXECUTEINFOA shExInfo = { 0 };
            shExInfo.cbSize       = sizeof(shExInfo);
            shExInfo.fMask        = SEE_MASK_NOCLOSEPROCESS | SEE_MASK_FLAG_NO_UI;
            shExInfo.hwnd         = 0;
            shExInfo.lpVerb       = "runas";                    // Operation to perform
            shExInfo.lpFile       = _executable.c_str();        // Application to start    
            shExInfo.lpParameters = _arguments.c_str();         // Additional parameters
            shExInfo.lpDirectory  = _workingDirectory.c_str();
            shExInfo.nShow        = dwShowWindowStyle;
            shExInfo.hInstApp     = 0;

            if (ShellExecuteExA(&shExInfo) && shExInfo.hProcess != 0)
            {
                _hProcess = shExInfo.hProcess;
                _dwPid = ::GetProcessId(_hProcess);

                Output(Log, "[%s] '%s' started, pid = '%ul', RunAs = '%s'", _title.c_str(), _executable.c_str(), _dwPid, _runAsAdmin ? "Admin" : "User");
                ::Log(LogTarget::File, "Application():StartProcess(): [%s] '%s' started, pid = '%ul', RunAs = '%s'.\n", _title.c_str(), _executable.c_str(), _dwPid, _runAsAdmin ? "Admin" : "User");

                _state = Running;
            }
            else 
            {
                Output(Log, "[%s] '%s' failed to start.", _title.c_str(), _executable.c_str());
                ::Log(LogTarget::File, "Application():StartProcess(): [%s] '%s' failed to start.\n", _title.c_str(), _executable.c_str());

                _state = Failed;

                return false;
            }
        }
        else
        {
            STARTUPINFOA startupInfo = { 0 };

            if (_do_capture)
            {
                if (_clearstreamonrun)
                {
                    StreamManager::Instance()->Clear(_stream);
                }

                _console = std::shared_ptr<utils::ConsoleCapture>(new utils::ConsoleCapture());
                _console->callback_on_new_line([=](const char* text){Output(Capture, text);});
            }

            startupInfo.wShowWindow = dwShowWindowStyle;
            startupInfo.dwFlags     = STARTF_USESHOWWINDOW;
            startupInfo.cb          = sizeof(STARTUPINFO);

            if (_do_capture)
            {
                _console->configure(startupInfo);
            }

            std::stringstream full_args;
            full_args << _executable << " " << _arguments;

            std::string procargs = full_args.str();
            PROCESS_INFORMATION piProcInfo = { 0 };

            ::Log(LogTarget::File, "Application():StartProcess(): Calling CreateProcessA.  Full Args = '%s'.\n", full_args.str().c_str());
            if (::CreateProcessA(NULL,
                (char*)full_args.str().c_str(),
                NULL,
                NULL,
                TRUE, //*was* FALSE, but passed TRUE to capture App's output: handles are inherited; TODO: check
                0,
                NULL,
                _workingDirectory.c_str(),
                &startupInfo, &piProcInfo)) 
            {
                _dwPid = piProcInfo.dwProcessId;
                _hProcess = piProcInfo.hProcess;

                ::CloseHandle(piProcInfo.hThread);

                Output(Log, "[%s] '%s' started, pid = '%ul', RunAs = '%s'", _title.c_str(), _executable.c_str(), _dwPid, _runAsAdmin ? "admin" : "user");
                ::Log(LogTarget::File, "Application():StartProcess(): [%s] '%s' started, pid = '%ul', RunAs = '%s'.\n", _title.c_str(), _executable.c_str(), _dwPid, _runAsAdmin ? "admin" : "user");

                _state = Running;
            } 
            else 
            {
                DWORD lastError = GetLastError();
                Output(Log, "[%s] '%s' failed to start. Error = '%d'", _title.c_str(), _executable.c_str(), lastError);
                ::Log(LogTarget::File, "Application():StartProcess(): [%s] '%s' failed to start. Error = '%d'.\n", _title.c_str(), _executable.c_str(), lastError);

                _state = Failed;

                return false;
            }
        }

        return true;
    }

public:
    bool WaitForCompletion()
    {
        if (_type == RunOnce)
            return _waitForCompletion && _state != Completed;

        return false;
    }

    bool Update(double elapsedTimeInMs)
    {
        if (_console != nullptr) 
            while(_console->ProcessCapture());

        switch(_state)
        {
            case Initial:
                {
                    if (!StartProcess())
                        return !WaitForCompletion();
                } break;
            case Failed:
                {
                    if (_retries <= _retryAttempts)
                        return _state;

                    ++_retryAttempts;
                    _state = Retry; 

                    Output(Log, "[%s] Retry Attempt %d.", _retryAttempts);
                    ::Log(LogTarget::File, "Application():Update(): Retry Attempt '%d'.\n", _retryAttempts);

                    if (!StartProcess())
                        return !WaitForCompletion();
                } break;
            case Running:
                if (!IsProcessActive())
                {
                    if (_type == RunOnce)
                    {
                        if ((_successCode != UndefinedSuccessCode) && (_successCode != _exitCode) && (_retryAttempts < _retries))
                        {
                            if (_retryAttempts == 0)
                            {
                                Output(Log, "[%s] Process Failed.  Will restart in %dms.  Exitcode '%d'.", _title.c_str(), _restartDelay, _exitCode);
                                ::Log(LogTarget::File, "Application():Update(): [%s] Process Failed.  Will restart in %dms.  Exitcode '%d'.\n", _title.c_str(), _restartDelay, _exitCode);
                            }
                            else
                            {
                                Output(Log, "[%s] Process Failed.  Will restart in %dms.  Exitcode '%d'.  Retry Attempt (%d/%d)", _title.c_str(), _restartDelay, _exitCode, _retryAttempts, _retries);
                                ::Log(LogTarget::File, "Application():Update(): [%s] Process Failed.  Will restart in %dms.  Exitcode '%d'.  Retry Attempt (%d/%d)\n", _title.c_str(), _restartDelay, _exitCode, _retryAttempts, _retries);
                            }

                            ++_retryAttempts;
                            _state = Delaying;
                            _startDelay = _restartDelay;
                        }
                        else
                        {
                            if ((_successCode != UndefinedSuccessCode) && (_retryAttempts != 0))
                            {
                                Output(Log, "[%s] Process Completed with Exitcode '%d'.  Retry Attempt (%d/%d)", _title.c_str(), _exitCode, _retryAttempts, _retries);
                                ::Log(LogTarget::File, "Application():Update(): [%s] Process Completed.  Exitcode '%d'.  Retry Attempt (%d/%d)\n", _title.c_str(), _exitCode, _retryAttempts, _retries);
                            }
                            else
                            {
                                Output(Log, "[%s] Process Completed with Exitcode %d.", _title.c_str(), _exitCode);
                                ::Log(LogTarget::File, "Application():Update(): [%s] Process Completed.  Exitcode '%d'.\n", _title.c_str(), _exitCode);
                            }

                            _state = Completed;
                        }

                        ::Log(LogTarget::File, "Application():Update(): _successCode = '%d' | _exitCode = '%d' | _retries = '%d' | _retryAttempts = '%d' | _restartDelay = %dms.\n", _successCode, _exitCode, _retries, _retryAttempts, _restartDelay);
                    }
                    else if (_type == Monitor)
                    {
                        Output(Log, "[%s] Process exited with code %d. Will restart in %d milliseconds.", _title.c_str(), _exitCode, _restartDelay);
                        ::Log(LogTarget::File, "Application():Update(): '%s' process exited with code '%d'.  Will restart in %d milliseconds.\n", _title.c_str(), _exitCode, _restartDelay);

                        _state = Delaying;
                        _startDelay = _restartDelay;
                    }
                    else if (_type == Interval)
                    {
                        Output(Log, "[%s] Process exited with code %d. Will restart in %d milliseconds.", _title.c_str(), _exitCode, _intervalDelay);
                        ::Log(LogTarget::File, "Application():Update(): '%s' process exited with code '%d'. Will restart in %d milliseconds.\n", _title.c_str(), _exitCode, _intervalDelay);

                        _state = Delaying;
                        _startDelay = _intervalDelay;
                    }
                    else if (_type == Manual)
                    {
                        Output(Log, "[%s] Process exited with code '%d'", _title.c_str(), _exitCode);
                        ::Log(LogTarget::File, "Application():Update(): '%s' process exited with code '%d'.\n", _title.c_str(), _exitCode);

                        _state = Idle;
                    }
                } break;
            case Delaying:
                {
                    _timeSinceExit += elapsedTimeInMs;
                    if (_timeSinceExit >= _startDelay)
                    {
                        _timeSinceExit = 0;
                        _state = Initial;
                    }
                } break;
            case Idle:
                {
                } break;
        }

        return !WaitForCompletion();
    }

    const char* name() { return _title.c_str(); }

    void start()
    {
        if (_state == Idle || _state == Delaying || _state == Failed || _state == Completed)
            _state = Initial;

        _timeSinceExit = 0;
        _startDelay = 0;
    }

    void stop()
    {
        if (_hProcess != 0)
        {
            ::TerminateProcess(_hProcess,-1);

            Output(Log, "[%s] Process Forced Stopped.", _title.c_str());
            ::Log(LogTarget::File, "Application():stop(): [%s] process force stopped.\n", _title.c_str());
        }
        _state = Idle;
    }

    bool IsProcessActive()
    {
        DWORD nCode;
        if (WaitForSingleObject(_hProcess, 0) == WAIT_TIMEOUT)
        {
            return true;
        }

        if (GetExitCodeProcess(_hProcess, &nCode))
        {
            _exitCode = nCode;
        }

        CloseHandle(_hProcess);
        _hProcess = NULL;

        return false;
    }

    void Release()
    {
        delete this;
    }

private:
    //
    // Members
    //

    std::shared_ptr<utils::ConsoleCapture> _console;
    eProcessState   _state              = Initial;
    HANDLE          _hProcess           = NULL;
    DWORD           _dwPid              = 0;
    int             _retryAttempts      = 0;
    DWORD           _exitCode           = 0;
    double          _timeSinceExit      = 0;
    bool            _do_capture         = false;
    unsigned int    _startDelay         = 0;

    //
    // Configuration
    //

    std::string     _title;
    std::string     _executable;
    std::string     _arguments;
    std::string     _workingDirectory;
    std::string     _stream;
    eProcessType    _type               = RunOnce;
    bool            _enabled            = true;
    bool            _clearstreamonrun   = false;
    bool            _logging            = true;
    bool            _hidden             = false;
    bool            _waitForCompletion  = false;
    bool            _minimizeAtStart    = false;
    bool            _runAsAdmin         = false;
    bool            _singleton          = false;
    int             _retries            = 0;
    int             _successCode        = 0;
    unsigned int    _restartDelay       = 0;
    unsigned int    _intervalDelay      = 0;
};