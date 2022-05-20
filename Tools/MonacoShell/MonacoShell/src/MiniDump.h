#pragma once

/**********************************************************
Important Node: Only include minidump.h in main application source file.

Usage:
    Create an instance of MiniDump:
        MiniDumper minidump("application"); // the name of the application which also will be the name of the dump file

Notes:
    You may catch any application specific exception, and exclude accessviolations and other runtime exception.
    Don't use catch(...).  The minidumper catches any uncaught exceptions, and creates a minidump where the executable
    exists.  The dump will be named 'application.dmp' located next to the executable.
*/

struct _EXCEPTION_POINTERS;

class MiniDumper
{
private:
    static DWORD64*                 _vAddrStack;
    static UINT                     _max_callstack;
    static const TCHAR*             _szAppName;
    static std::basic_string<TCHAR> _szLogFile;
    static std::basic_string<TCHAR> _szDumpDirectory;
    static LONG                     _retvalOnSucess;
    static bool                     _reuseOutputFile;

    static LONG WINAPI TopLevelFilter(struct _EXCEPTION_POINTERS *pExceptionInfo);
    static void __cdecl InvalidParameterHandler(const wchar_t *, const wchar_t *, const wchar_t *, unsigned int, uintptr_t);

    void Initialize(const TCHAR* szAppName, const TCHAR* dumpdir, 
        const TCHAR* logfile, unsigned short max_callstack, LONG retOnSuccess, bool reuseOutputFile);

    static void WriteLog(const char* msg, const size_t length);
    static void Log(const char* source, const char* message);
    static void LogFormat(const char* source, const char* fmt, ...);

public:
    MiniDumper(const TCHAR* szAppName, const TCHAR* dumpdir = NULL, 
        const TCHAR* logfile = NULL, unsigned short max_callstack = 0, bool reuseOutputFile = true);

    MiniDumper();
};