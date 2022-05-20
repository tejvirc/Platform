#define _MINIDUMPIMPL_
#define _CRT_NON_CONFORMING_SWPRINTF
#include <tchar.h>
#include <stdio.h> 
#include <windows.h>
#include <assert.h>
#include <string>
#include <crtdbg.h>
#define _IMAGEHLP_
#include "minidump.h"
#include <Shlobj.h>
#pragma warning( push )
#pragma warning( disable:4091 )
#include <dbghelp.h>
#pragma warning( pop ) 
#include <sys/timeb.h>
#include <time.h>
#include <locale.h>
#include <iomanip>
#include "symlookup.h"
#include "logger.h"

typedef LONG NTSTATUS;

//
// Exception code used in _invalid_parameter
//

#ifndef STATUS_INVALID_PARAMETER
#define STATUS_INVALID_PARAMETER    ((NTSTATUS)0xC000000DL)
#endif  /* STATUS_INVALID_PARAMETER */

#ifdef __cplusplus
extern "C" {
#endif  /* __cplusplus */

void * _ReturnAddress(void);
#pragma intrinsic(_ReturnAddress)

#ifdef _X86_
void * _AddressOfReturnAddress(void);
#pragma intrinsic(_AddressOfReturnAddress)
#endif  /* _X86_ */

#ifdef __cplusplus
}
#endif  /* __cplusplus */

// based on dbghelp.h
typedef BOOL (WINAPI *MINIDUMPWRITEDUMP)(HANDLE hProcess, DWORD dwPid, HANDLE hFile, MINIDUMP_TYPE DumpType,
                CONST PMINIDUMP_EXCEPTION_INFORMATION ExceptionParam,
                CONST PMINIDUMP_USER_STREAM_INFORMATION UserStreamParam,
                CONST PMINIDUMP_CALLBACK_INFORMATION CallbackParam
                );

using namespace std;

#undef _stprintf
#ifdef _UNICODE // fixes VS 2008 SWPRINTF CONFORMING macro bug
    #define _stprintf swprintf 
#else
    #define _stprintf sprintf_s
#endif

const TCHAR* MiniDumper::_szAppName;
DWORD64* MiniDumper::_vAddrStack;
std::basic_string<TCHAR> MiniDumper::_szLogFile;
std::basic_string<TCHAR> MiniDumper::_szDumpDirectory;
UINT MiniDumper::_max_callstack;
LONG MiniDumper::_retvalOnSucess;
bool MiniDumper::_reuseOutputFile;

MiniDumper::MiniDumper()
{
}

MiniDumper::MiniDumper(const TCHAR* szAppName, const TCHAR* dumpdir,
    const TCHAR* logfile, unsigned short max_callstack, bool reuseOutputFile)
{
    Initialize(szAppName, dumpdir, logfile, max_callstack, EXCEPTION_EXECUTE_HANDLER, reuseOutputFile);
}

void MiniDumper::Initialize(const TCHAR* szAppName, const TCHAR* dumpdir,
    const TCHAR* logfile, unsigned short max_callstack, LONG retOnSuccess, bool reuseOutputFile)
{
    //
    // if this assert fires then you have two instances of MiniDumper
    // which is not allowed
    //

    assert(_szAppName==NULL);
    _max_callstack = max_callstack;
    static std::basic_string<TCHAR> appName(szAppName ? szAppName : _T("Application"));
    _szAppName = appName.data();
    _reuseOutputFile = reuseOutputFile;

    if (dumpdir)
        _szDumpDirectory = basic_string<TCHAR>(dumpdir);

    if (logfile)
        _szLogFile = basic_string<TCHAR>(logfile);

    if (max_callstack > 0 && max_callstack < 1024)
     {
        static std::vector<DWORD64> store;
        store.resize(sizeof(DWORD64)*max_callstack);
        _vAddrStack = store.data();
    }
    else
    {
        _vAddrStack = nullptr;
    }

    _retvalOnSucess = retOnSuccess;

    ::SetUnhandledExceptionFilter(TopLevelFilter);
    _set_invalid_parameter_handler(InvalidParameterHandler);
}

LONG MiniDumper::TopLevelFilter(struct _EXCEPTION_POINTERS *pExceptionInfo)
{
    LONG retval = EXCEPTION_CONTINUE_SEARCH;
    // HWND hParent = NULL; // find a better value for your app

    //
    //TODO: add crashhandler stackwalker to get the debug stack (add to log4cxx)
    //

    //
    // firstly see if dbghelp.dll is around and has the function we need
    // look next to the EXE first, as the one in System32 might be old 
    // (e.g. Windows 2000)
    //

    HMODULE hDll = NULL;
    TCHAR szDbgHelpPath[_MAX_PATH] = { 0 };

    if (GetModuleFileName(NULL, szDbgHelpPath, _MAX_PATH))
    {
        TCHAR *pSlash = _tcsrchr(szDbgHelpPath, '\\');
        if (pSlash)
        {
            _tcscpy(pSlash+1, _T("DBGHELP.DLL"));
            hDll = ::LoadLibrary(szDbgHelpPath);
        }
    }

    if (hDll==NULL)
    {
        //
        // load any version we can
        //

        hDll = ::LoadLibrary(_T("DBGHELP.DLL"));
    }

    LPCTSTR szResult = NULL;

    if (hDll)
    {
        MINIDUMPWRITEDUMP pDump = (MINIDUMPWRITEDUMP)::GetProcAddress(hDll, "MiniDumpWriteDump");
        if (pDump)
        {
            TCHAR *szDumpPath = 0; // Just for use with the "old" functions...
            TCHAR szScratch[_MAX_PATH * 4] = { 0 };
            std::basic_string<TCHAR> sPathAndFileName;

            //
            // Work out a good place for the dump file - we will first 
            // try to use the directory from where the application
            // was called = the "working directory".
            //

            if (_szDumpDirectory.empty())
            {
                long numCharsNecessary = GetCurrentDirectory(0, NULL);
                bool directoryPathOk = (numCharsNecessary > 0);
                if (!directoryPathOk) // Something is wrong with the directory string...
                {
                    //
                    // Set a default directory for the file.
                    //

                    _tcscpy(szDumpPath, _T(".\\"));
                }
                else // The directory string is Ok...
                {
                    //
                    // Allocate space for the directory string plus
                    // the terminating null character
                    //

                    szDumpPath = (TCHAR*)malloc(sizeof(TCHAR)*(numCharsNecessary+1));

                    //
                    // Store the current directory string (not including
                    // the filename)
                    //

                    GetCurrentDirectory(numCharsNecessary+1, szDumpPath);
                }

                //
                // At this point, szDumpPath contains a valid directory string.
                //

                sPathAndFileName = szDumpPath; // -just to make things easier...!
                free(szDumpPath);
            }
            else
            {
                sPathAndFileName = _szDumpDirectory;
            }

            //
            // If the last character in the directory string is not
            // a backslash, then add one
            //

            if (sPathAndFileName[sPathAndFileName.length()-1] != _T('\\'))
                sPathAndFileName += _T("\\");

            //
            // Append filename and extension:
            //

            sPathAndFileName += _szAppName;

            if (!_reuseOutputFile)
            {
                sPathAndFileName += _T("-");
                TCHAR datebuf[255] = { 0 };
                GetDateFormat(LOCALE_SYSTEM_DEFAULT, 0, NULL, _T("yyyy'.'MM'.'dd"), datebuf, 255);
                sPathAndFileName += datebuf;

                sPathAndFileName += _T(".");
                TCHAR timebuf[255] = { 0 };
                GetTimeFormat(LOCALE_SYSTEM_DEFAULT, 0, NULL, _T("HH'.'mm'.'ss"), timebuf, 255);
                sPathAndFileName += timebuf;
            }
 
            sPathAndFileName += _T(".dmp");

            Log("MiniDump", " ");
            Log("MiniDump", "Crash: Writing minidump");
            Log("MiniDump", " ");

            ::Log(LogTarget::File, "\nMiniDumper():TopLevelFilter(): *******************************\n");
            ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): *******************************\n");
            ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): ** Crash - Writing minidump. **\n");
            ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): *******************************\n");
            ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): *******************************\n\n");

            {
                //
                // Create the file
                //

                HANDLE hFile = ::CreateFile(sPathAndFileName.c_str(),
                    GENERIC_WRITE, FILE_SHARE_WRITE, 
                    NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

                if (hFile != INVALID_HANDLE_VALUE)
                {
                    _MINIDUMP_EXCEPTION_INFORMATION ExInfo;

                    ExInfo.ThreadId = ::GetCurrentThreadId();
                    ExInfo.ExceptionPointers = pExceptionInfo;
                    ExInfo.ClientPointers = NULL;
                    uint32_t stack_count = 0;

                    if (!_szLogFile.empty())
                    {
                        Log("MiniDump", "Loading lookup symbol tables");
                        ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): Loading lookup symbol tables.\n");

                        aristocrat::SymbolLookup* pLookup = aristocrat::SymbolLookup::Create();
                        if (pLookup == nullptr)
                        {
                            Log("MiniDump", "Failed loading lookup symbol tables");
                            ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): Failed loading lookup symbol tables.\n");
                        }

                        if (pLookup != nullptr)
                        {
                            stack_count = pLookup->StackWalkInternal(pExceptionInfo->ContextRecord,
                                _vAddrStack, _max_callstack);
                            LogFormat("MiniDump", "Callstack (%d lines):", stack_count);
                            ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): Callstack '%d' lines.\n", stack_count);

                            for (uint32_t i = 0; i < stack_count; ++i)
                            {
                                const char* str = pLookup->Lookup(_vAddrStack[i]);
                                
                                if (strlen(str) > 0)
                                {
                                    Log("MiniDump", str);
                                    ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): %s\n", str);
                                }
                                else
                                {
                                    Log("MiniDump","~~--[missing symbol information]--~~");
                                    ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): ~~--[Missing symbol information]--~~\n");
                                }
                            }
                        }
                        pLookup->Release();
                    }

                    //
                    // write the dump
                    //

                    BOOL bOK = pDump(GetCurrentProcess(), GetCurrentProcessId(), hFile, MiniDumpNormal, 
                        &ExInfo, NULL, NULL);

                    if (bOK)
                    {
                        _stprintf(szScratch, 1024, _T("Saved dump file to '%s'"), sPathAndFileName.c_str());
                        szResult = szScratch;
                        retval = _retvalOnSucess;

                        ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): szResult = Saved dump file to '%s'.\n", szResult);
                    }
                    else
                    {
                        _stprintf(szScratch, 1024, _T("Failed to save dump file to '%s' (ERROR %d)"), 
                            sPathAndFileName.c_str(), GetLastError());
                        szResult = szScratch;

                        ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): szResult = Failed to save dump file to '%s' (ERROR '%d).\n", szResult, GetLastError());
                    }

                    ::CloseHandle(hFile);
                }
                else
                {
                    _stprintf(szScratch, 1024, _T("Failed to create dump file '%s' (ERROR %d)"), 
                        sPathAndFileName.c_str(), GetLastError());
                    szResult = szScratch;

                    ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): szResult = Failed to create dump file '%s' (ERROR '%d).\n", szResult, GetLastError());
                }
            }
        }
        else
        {
            szResult = _T("DBGHELP.DLL too old");
            ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): szResult = 'DBGHELP.DLL too old'.\n");
        }
    }
    else
    {
        szResult = _T("DBGHELP.DLL not found");
        ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): szResult = 'DBGHELP.DLL not found'.\n");
    }

    // if (szResult)
    //     ::MessageBox(NULL, szResult, _szAppName, MB_OK);

    ::Log(LogTarget::File, "MiniDumper():TopLevelFilter(): Returning %ld\n", retval);

    return retval;
}

void MiniDumper::LogFormat(const char* source, const char* fmt, ...)
{
    const int LOG_BUFFER_SIZE = 4096;
    char log_buffer[LOG_BUFFER_SIZE] = { 0 };

    va_list argptr;
    va_start(argptr, fmt);
    
    if (-1 != (vsnprintf(log_buffer, LOG_BUFFER_SIZE, fmt, argptr)))
    {
        Log(source, log_buffer);
    }

    va_end(argptr);
}

void MiniDumper::Log(const char* source, const char* message)
{
    SYSTEMTIME lt;
    GetLocalTime(&lt);
    char time_buffer[256] = { 0 };
    std::snprintf(time_buffer, sizeof(time_buffer), "%d-%d-%d %d:%d:%d.%3.3d",
        lt.wYear, lt.wMonth, lt.wDay, lt.wHour, lt.wMinute, lt.wSecond, lt.wMilliseconds);

    std::stringstream ss;
    ss << "[" << time_buffer << "] " 
       << source << ": " << message << std::endl;

    auto str(ss.str());
    WriteLog(str.c_str(), str.length());
}

void MiniDumper::WriteLog(const char* msg, const size_t length)
{
    if (_szDumpDirectory.empty() || _szLogFile.empty())
    {
        ::OutputDebugStringA(msg);
    }
    else
    {
        ::SHCreateDirectoryEx(NULL, _szDumpDirectory.c_str(), NULL);

        char fullpath[MAX_PATH] = { 0 };

        ::PathCombineA(fullpath, _szDumpDirectory.c_str(), _szLogFile.c_str());

        FILE* f = fopen(fullpath, "a+");
        fwrite(msg, 1, length, f);
        fclose(f);
    }
}

void MiniDumper::InvalidParameterHandler(const wchar_t *pszExpression, const wchar_t *pszFunction, 
    const wchar_t *pszFile, unsigned int nLine, uintptr_t pReserved)
{
    //
    // Fake an exception for dump file.
    //

    EXCEPTION_RECORD ExceptionRecord;
    CONTEXT ContextRecord;
    EXCEPTION_POINTERS ExceptionPointers;

    (pszExpression);
    (pszFunction);
    (pszFile);
    (nLine);
    (pReserved);

#ifdef _X86_
    __asm {
        mov dword ptr [ContextRecord.Eax], eax
        mov dword ptr [ContextRecord.Ecx], ecx
        mov dword ptr [ContextRecord.Edx], edx
        mov dword ptr [ContextRecord.Ebx], ebx
        mov dword ptr [ContextRecord.Esi], esi
        mov dword ptr [ContextRecord.Edi], edi
        mov word ptr [ContextRecord.SegSs], ss
        mov word ptr [ContextRecord.SegCs], cs
        mov word ptr [ContextRecord.SegDs], ds
        mov word ptr [ContextRecord.SegEs], es
        mov word ptr [ContextRecord.SegFs], fs
        mov word ptr [ContextRecord.SegGs], gs
        pushfd
        pop [ContextRecord.EFlags]
    }

    ContextRecord.ContextFlags = CONTEXT_CONTROL;
#pragma warning(push)
#pragma warning(disable:4311)
    ContextRecord.Eip = (ULONG)_ReturnAddress();
    ContextRecord.Esp = (ULONG)_AddressOfReturnAddress();
#pragma warning(pop)
    ContextRecord.Ebp = *((ULONG *)_AddressOfReturnAddress()-1);

#elif defined (_IA64_) || defined (_AMD64_)
    //
    // Need to fill up the Context in IA64 and AMD64.
    //

    RtlCaptureContext(&ContextRecord);
#else // defined (_IA64_) || defined (_AMD64_)
    ZeroMemory(&ContextRecord, sizeof(ContextRecord));
#endif // defined (_IA64_) || defined (_AMD64_)

    ZeroMemory(&ExceptionRecord, sizeof(ExceptionRecord));

    ExceptionRecord.ExceptionCode = STATUS_INVALID_PARAMETER;
    ExceptionRecord.ExceptionAddress = _ReturnAddress();

    ExceptionPointers.ExceptionRecord = &ExceptionRecord;
    ExceptionPointers.ContextRecord = &ContextRecord;

    //
    // Create dump file by calling TopLevelFilter...
    //

    TopLevelFilter(&ExceptionPointers);

    //
    // Break into debugger if debug build.
    //

    _CrtDbgBreak();

    //
    // Terminate ourself.
    //

    TerminateProcess(GetCurrentProcess(), STATUS_INVALID_PARAMETER);
}