//------------------------------------------------
//
// Copyright(c) 2015 Kortek. All rights reserved.
//
//   Hugh Chang, chk@kortek.co.kr
//
//   $Id: Utils.cpp 4241 2016-08-29 02:19:42Z chk $
//
//------------------------------------------------

#include "stdafx.h"
#include "Utils.h"
#include <string>
#include <algorithm>
#include <numeric>

Utils::Utils(void) { }
Utils::~Utils(void) { }

char* Utils::errorCodeToString(DWORD errorCode)
{
    const DWORD msgBufMax = 1024;
    static char msgBuf[msgBufMax];
    FormatMessageA(FORMAT_MESSAGE_FROM_SYSTEM, NULL, errorCode, MAKELANGID(LANG_ENGLISH,SUBLANG_DEFAULT), msgBuf, msgBufMax, NULL);
    return msgBuf;
}

bool Utils::FindStringWithAsterisk(std::string deviceId, const std::string& searchDeviceId)
{
    const char* source = deviceId.c_str();
    const char* search = searchDeviceId.c_str();
    while(*source != 0)
    {
        if (*source == *search || *search == '*') {
            ++search;
            if (*search == 0) // search pattern finished, found match
                return true;
        }
        else { search = searchDeviceId.c_str(); } // start again
        ++source;
    }
    return false;
}

std::string Utils::GetDeviceInstancePid(const std::string& devicePath)
{
    std::string devicePathUpper = devicePath;
    std::transform(devicePathUpper.begin(), devicePathUpper.end(), devicePathUpper.begin(), ::toupper);

    size_t searchDeviceIndex = devicePathUpper.find("PID_");

    if (searchDeviceIndex == std::string::npos)
    {
        return "";
    }

    return devicePath.substr(searchDeviceIndex + 4, 4);
}