#pragma once
#include <windows.h>
#include <commctrl.h>
#include <shlobj.h>
#include <shlwapi.h>
#include <stdio.h>
#include <fcntl.h>
#include <io.h>
#include <iostream>
#include <fstream>
#include "stringutil.h"

#pragma comment(lib,"shlwapi")

namespace aristocrat
{
#pragma warning(push)
#pragma warning(disable:4505) //unreferenced local function has been removed

    static bool has_extension(const wchar_t* filepath, const wchar_t* ext)
    {
        size_t n = wcslen(filepath);
        size_t p = wcslen(ext);
        while (n-- && p--)
        {
            if (filepath[n] != ext[p]) return false;
        }
        return true;
    }

    static bool get_folder_from_path(wchar_t* folder, const wchar_t* filepath)
    {
        size_t n = wcslen(filepath);
        int i = 0;
        while (n--)
        {
            if (filepath[n] == L'\\') break;
        }

        for (i = 0; i < n; ++i)
        {
            folder[i] = filepath[i];
        }
        folder[i] = 0;
        return true;
    }

    static bool get_filename_from_path(wchar_t* filename, const wchar_t* filepath)
    {
        size_t nlen = wcslen(filepath);
        size_t n = nlen;
        size_t i = 0;
        wchar_t *fn = filename;
        while (n--)
        {
            if (filepath[n] == L'\\') break;
        }

        for (i = n + 1; i < nlen; ++i)
        {
            *fn++ = filepath[i];
        }
        *fn++ = 0;
        return true;
    }

    static bool get_filename_from_path(char* filename, const char* filepath)
    {
        size_t nlen = strlen(filepath);
        size_t n = nlen;
        size_t i = 0;
        char *fn = filename;
        while (n--)
        {
            if (filepath[n] == '\\') break;
        }

        for (i = n + 1; i < nlen; ++i)
        {
            *fn++ = filepath[i];
        }
        *fn++ = 0;
        return true;
    }

    static const char* SwitchToExecutablesDirectory(HINSTANCE hInstance)
    {
        static char moduleFileName[MAX_PATH + 1];
        GetModuleFileNameA(hInstance, moduleFileName, MAX_PATH + 1);
        PathRemoveFileSpecA(moduleFileName);
        SetCurrentDirectoryA(moduleFileName);
        return moduleFileName;
    }

    

    static const char* OpenGameDialog()
    {
        static char game_path[MAX_PATH];
        OPENFILENAMEA ofn;
        ZeroMemory(&ofn, sizeof(ofn));
        ofn.lStructSize = sizeof(ofn);
        ofn.hwndOwner = 0;
        ofn.lpstrFile = game_path;
        ofn.lpstrFile[0] = 0;
        ofn.nMaxFile = sizeof(game_path);
        ofn.lpstrFilter = "Game Dll\0*.DLL\0";
        ofn.nFilterIndex = 1;
        ofn.lpstrFileTitle = NULL;
        ofn.nMaxFileTitle = 0;
        ofn.lpstrInitialDir = NULL;
        ofn.lpstrTitle = "Open GDK Game";
        ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;
        if (GetOpenFileNameA(&ofn))
        {
            return game_path;
        }
        return nullptr;
    }

    static size_t GetFileSize(const char* filename)
    {
        if (!::PathFileExistsA(filename)) return 0;
        FILE* f = fopen(filename, "r");
        fseek(f, 0, SEEK_END);
        size_t fsize = ftell(f);
        fclose(f);
        return fsize;
    }

    // Use LocalFree to delete return value
    static char* ReadFile(const char* filename)
    {
        if (!::PathFileExistsA(filename)) return nullptr;
        FILE* f = fopen(filename,"r");
        if (f == 0) return nullptr;
        fseek(f, 0, SEEK_END);
        size_t size = ftell(f);
        fseek(f, 0, SEEK_SET);
        char* buffer = (char*)LocalAlloc(LMEM_ZEROINIT, sizeof(char*) * (size + 1));
        fread(buffer, 1, size, f);
        fclose(f);

        return buffer;
    }

    static bool ReadBinaryFile(const char* filename, char* buffer, size_t buffer_size)
    {
        if (!::PathFileExistsA(filename)) return false;
        FILE* f = fopen(filename, "rb");
        if (f == 0) return false;
        fseek(f, 0, SEEK_END);
        size_t size = ftell(f);
        if (size > buffer_size) 
        {
            fclose(f);
            return false;
        }
        fseek(f, 0, SEEK_SET);
        fread(buffer, 1, size, f);
        fclose(f);
        return true;
    }

    static bool WriteFile(const char* filename, const char* data, size_t size)
    {
        FILE* f = fopen(filename, "wb");
        if (f == 0) return false;
        
        fwrite(data, 1, size, f);
        fflush(f);
        fclose(f);
        return GetFileSize(filename) == size;
    }

    static LPSTR* CommandLineToArgv(int* argc, bool ignoreFirst, const char* cmdline)
    {
        static char args[4096]; 
        char* argp = &args[0];
        std::wstring cmdlinew = aristocrat::MultiByte2WideCharacterString(cmdline);
        auto argvw = CommandLineToArgvW(cmdlinew.c_str(), argc);

        LPSTR* argv;
        argv = (LPSTR*)LocalAlloc(LMEM_ZEROINIT, sizeof(char*) * (*argc + 1));
        int di = 0;
        for (int i = ignoreFirst ? 1 : 0; i < *argc; ++i)
        {
            std::string str = aristocrat::WideCharacter2MultiByteString(argvw[i]);
            strcpy(argp, str.c_str());
            argv[di++] = argp;
            argp += str.size();
            *argp++ = 0;
        }

        argv[*argc] = 0;
        LocalFree(argvw);
        *argc = ignoreFirst ? *argc - 1 : *argc;

        return argv;
    }

    static LPSTR* CommandLineToArgv(int* argc, bool ignoreFirst)
    {
        static char args[4096];
        char* argp = &args[0];
        
        auto argvw = CommandLineToArgvW(GetCommandLineW(), argc);



        LPSTR* argv;
        argv = (LPSTR*)LocalAlloc(LMEM_ZEROINIT, sizeof(char*) * (*argc + 1));
        int di = 0;
        for (int i = ignoreFirst ? 1 : 0; i < *argc; ++i)
        {
            std::string str = aristocrat::WideCharacter2MultiByteString(argvw[i]);
            strcpy(argp, str.c_str());
            argv[di++] = argp;
            argp += str.size();
            *argp++ = 0;
        }

        argv[*argc] = 0;
        LocalFree(argvw);
        *argc = ignoreFirst ? *argc - 1 : *argc;

        return argv;
    }

    void RedirectIOToConsole()
    {
        CONSOLE_SCREEN_BUFFER_INFO coninfo;
        if (GetStdHandle(STD_OUTPUT_HANDLE) != 0)
            return;
        bool wasAttached = false;
        // allocate a console for this app
        if (!AttachConsole(ATTACH_PARENT_PROCESS))
        {
            AllocConsole();
             // set the screen buffer to be big enough to let us scroll text
            GetConsoleScreenBufferInfo(GetStdHandle(STD_OUTPUT_HANDLE), &coninfo);
            coninfo.dwSize.Y = 80;
            SetConsoleScreenBufferSize(GetStdHandle(STD_OUTPUT_HANDLE), coninfo.dwSize);
        } else wasAttached = true;

        GetConsoleScreenBufferInfo(GetStdHandle(STD_OUTPUT_HANDLE), &coninfo);

         HANDLE ConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);
        int SystemOutput = _open_osfhandle(intptr_t(ConsoleOutput), _O_TEXT);
        FILE *COutputHandle = _fdopen(SystemOutput, "w");

        // Get STDERR handle
        HANDLE ConsoleError = GetStdHandle(STD_ERROR_HANDLE);
        int SystemError = _open_osfhandle(intptr_t(ConsoleError), _O_TEXT);
        FILE *CErrorHandle = _fdopen(SystemError, "w");

        // Get STDIN handle
        HANDLE ConsoleInput = GetStdHandle(STD_INPUT_HANDLE);
        int SystemInput = _open_osfhandle(intptr_t(ConsoleInput), _O_TEXT);
        FILE *CInputHandle = _fdopen(SystemInput, "r");

        // make cout, wcout, cin, wcin, wcerr, cerr, wclog and clog
        // point to console as well
        std::ios::sync_with_stdio(true);
        
        freopen_s(&CInputHandle, "CONIN$", "r", stdin);
        freopen_s(&COutputHandle, "CONOUT$", "w", stdout);
        freopen_s(&CErrorHandle, "CONOUT$", "w", stderr);
        std::wcout.clear();
        std::cout.clear();
        std::wcerr.clear();
        std::cerr.clear();
        std::wcin.clear();
        std::cin.clear();
        
        if (wasAttached)
        {
            printf("\r");
            for (auto i = 0; i < coninfo.dwSize.X; i++)
                printf(" ");
            printf("\n");
        }
    }
#pragma warning(pop)
}