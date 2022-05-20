#pragma once

#include <platform.h>
#include <stringutil.h>
#include <vector>


class ProcessList
{
public:
    ProcessList()
    {
        EnumerateProcesses();
    }

    bool Exist(std::string pathToFile)
    {
       return GetInfo(pathToFile) != nullptr;
    }

    PROCESSENTRY32* GetInfo(std::string pathToFile)
    {
        std::string strExecName = pathToFile;
        strExecName.erase(0, pathToFile.rfind('\\', pathToFile.length()) + 1);
        for (int i = 0; i < _processList.size();++i)
        {
            PROCESSENTRY32* pe = &_processList[i];
            if (aristocrat::icasecmp(pe->szExeFile, strExecName))
                return pe;
        }
        std::string s;
        
        return nullptr;
    }

    void EnumerateProcesses()
    {
        _processList.clear();
        HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

        PROCESSENTRY32 pe;

        if (Process32First(hSnapshot, &pe)) 
        {
            _processList.reserve(200);
            do 
            {
                _processList.push_back(pe);
            } while (Process32Next(hSnapshot, &pe));
        }
        CloseHandle(hSnapshot);    
    }

    std::vector<PROCESSENTRY32> _processList;
};
