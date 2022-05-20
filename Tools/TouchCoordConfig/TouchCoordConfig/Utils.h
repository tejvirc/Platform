//------------------------------------------------
//
// Copyright(c) 2015 Kortek. All rights reserved.
//
//   Hugh Chang, chk@kortek.co.kr
//
//   $Id: Utils.h 4241 2016-08-29 02:19:42Z chk $
//
//------------------------------------------------

#pragma once

#define defaultIniFile _T("./TouchFlip.ini")

#include <string>

class Utils
{
private:

public:

    Utils(void);
    ~Utils(void);

    static char*  errorCodeToString(DWORD errorCode);

    static bool FindStringWithAsterisk(std::string, const std::string&);
    static std::string GetDeviceInstancePid(const std::string& devicePath);

};