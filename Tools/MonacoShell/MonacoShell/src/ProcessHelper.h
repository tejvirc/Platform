#pragma once
#include <windows.h> 
#include <tchar.h>
#include <stdio.h> 
#include <strsafe.h>
#include <string>
#include <thread>
#include <future>
#define BUFSIZE 4096 

class ProcessHelper
{
public:
    static int ExcecuteProcess(const std::string& commandLine, std::string* output)
    {
        HANDLE hChildStd_OUT_Rd = NULL;
        HANDLE hChildStd_OUT_Wr = NULL;

        SECURITY_ATTRIBUTES saAttr;


        // Set the bInheritHandle flag so pipe handles are inherited. 

        saAttr.nLength = sizeof(SECURITY_ATTRIBUTES);
        saAttr.bInheritHandle = TRUE;
        saAttr.lpSecurityDescriptor = NULL;

        // Create a pipe for the child process's STDOUT. 

        if (!CreatePipe(&hChildStd_OUT_Rd, &hChildStd_OUT_Wr, &saAttr, 0))
            return -1;

        // Ensure the read handle to the pipe for STDOUT is not inherited.

        if (!SetHandleInformation(hChildStd_OUT_Rd, HANDLE_FLAG_INHERIT, 0))
        {
            CloseHandle(hChildStd_OUT_Rd);
            CloseHandle(hChildStd_OUT_Wr);
            return -1;
        }

        // Create the child process. 

        CreateChildProcess(commandLine, output, hChildStd_OUT_Rd, hChildStd_OUT_Wr);

        return 0;
    }
private:
    static  int CreateChildProcess(const std::string& commandLine, std::string* output, HANDLE hChildStd_OUT_Rd, HANDLE hChildStd_OUT_Wr)
        // Create a child process that uses the previously created pipes for STDIN and STDOUT.
    {
        PROCESS_INFORMATION piProcInfo;
        STARTUPINFO siStartInfo;
        BOOL bSuccess = FALSE;
        DWORD exitCode = -1;

        // Set up members of the PROCESS_INFORMATION structure. 

        ZeroMemory(&piProcInfo, sizeof(PROCESS_INFORMATION));

        // Set up members of the STARTUPINFO structure. 
        // This structure specifies the STDIN and STDOUT handles for redirection.

        ZeroMemory(&siStartInfo, sizeof(STARTUPINFO));
        siStartInfo.cb = sizeof(STARTUPINFO);
        siStartInfo.hStdError = hChildStd_OUT_Wr;
        siStartInfo.hStdOutput = hChildStd_OUT_Wr;
        siStartInfo.hStdInput = NULL;
        siStartInfo.dwFlags |= STARTF_USESTDHANDLES;

        // Create the child process. 

        bSuccess = CreateProcessA(NULL,
            (char*)commandLine.c_str(), // command line 
            NULL,                       // process security attributes 
            NULL,                       // primary thread security attributes 
            TRUE,                       // handles are inherited 
            CREATE_NO_WINDOW,           // creation flags 
            NULL,                       // use parent's environment 
            NULL,                       // use parent's current directory 
            &siStartInfo,               // STARTUPINFO pointer 
            &piProcInfo);               // receives PROCESS_INFORMATION 

         // If an error occurs, exit the application. 
        if (!bSuccess)
            return exitCode;
        auto asyncRead = std::async(std::launch::async, &ProcessHelper::ReadFromPipe, output, hChildStd_OUT_Rd);
        ::WaitForSingleObject(piProcInfo.hProcess, INFINITE);
        ::GetExitCodeProcess(piProcInfo.hProcess, &exitCode);

        // Close handles to the child process and its primary thread.
        // Some applications might keep these handles to monitor the status
        // of the child process, for example. 

        CloseHandle(piProcInfo.hProcess);
        CloseHandle(piProcInfo.hThread);
        CloseHandle(hChildStd_OUT_Wr);
        asyncRead.wait();
        return exitCode;
    }

    static  void ReadFromPipe(std::string* output, HANDLE hChildStd_OUT_Rd)

        // Read output from the child process's pipe for STDOUT
        // and write to the parent process's pipe for STDOUT. 
        // Stop when there is no more data. 
    {
        DWORD dwRead;
        CHAR chBuf[BUFSIZE];
        BOOL bSuccess = FALSE;
        for (;;)
        {
            bSuccess = ReadFile(hChildStd_OUT_Rd, chBuf, BUFSIZE, &dwRead, NULL);
            if (!bSuccess || dwRead == 0)
            {
                break;
            }
            if (output)
            {
                chBuf[std::min(dwRead, (DWORD)(BUFSIZ-1))] = 0;
                output->append(chBuf);
            }
        }
        CloseHandle(hChildStd_OUT_Rd);
    }
};