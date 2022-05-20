#pragma once
#include <Windows.h>
#include <stdlib.h>
#include <crtdbg.h>
#include <cassert>

#include <sstream>
#include <iomanip>
#include <debugapi.h>

namespace aristocrat
{
    static inline void WaitForDebugger()
    {

        while (!::IsDebuggerPresent())
            ::Sleep(100);
        ::DebugBreak();
    }

    static inline void Trace(const char* szValue, ...)
    {
        char szBuffer[4096] = {};
        va_list argptr;
        va_start(argptr, szValue);
        if (-1 == (vsprintf(szBuffer, szValue, argptr)))
        {
            assert(false);
        }
        va_end(argptr);


        
    #if defined(WIN32)
        OutputDebugStringA(szBuffer);
    #else
        fprintf(stdout, szBuffer);
    #endif // WIN32
    }


    static inline void Debug(const char* szValue, ...)
    {
        char szBuffer[4096] = {};
        va_list argptr;
        va_start(argptr, szValue);
        if (-1 == (vsprintf(szBuffer, szValue, argptr)))
        {
            assert(false);
        }
        va_end(argptr);


#if defined(WIN32)
        OutputDebugStringA(szBuffer);
#else
        fprintf(stdout, szBuffer);
#endif // WIN32
    }
}

#ifdef _DEBUG
    #define TRACE(a,...) aristocrat::Trace(a,##__VA_ARGS__);
    #define DEBUG(a,...) aristocrat::Debug(a,##__VA_ARGS__);
#else
#define TRACE(a,...) ((void)0)
#define DEBUG(a,...) ((void)0)
#define FORCETRACE(a,...) aristocrat::Trace(a,##__VA_ARGS__);
#define FORCEDEBUG(a,...) aristocrat::Debug(a,##__VA_ARGS__);
#endif