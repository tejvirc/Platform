//------------------------------------------------
//
// Copyright(c) 2015 Kortek. All rights reserved.
//
//   Hugh Chang, chk@kortek.co.kr
//
//   $Id: HidDevice.h 4241 2016-08-29 02:19:42Z chk $
//
//------------------------------------------------

#pragma once

extern "C" {

    // We need Windows Driver Kit 7.1.0.7600 (Do not use 8.0 to support Windows XP)
    //   Project Properties
    //     VC++ Directories
    //       Include Directories : $(IncludePath);C:\WinDDK\7600.16385.1\inc\api
    //       Library Directories : $(LibraryPath);C:\WinDDK\7600.16385.1\lib\win7\i386
    //     Linker - Input - Additional Dependencies
    //       setupapi.lib;hid.lib;hidparse.lib;%(AdditionalDependencies)
    //     General - Platform Toolset
    //       Visual Studio 2012 - Windows XP (v110_xp)

    #include <SETUPAPI.H>
    #include <hidsdi.h>
    #include <hidpi.h>
}

#include <string>

class HidDevice
{
private:
    std::string connectedDeviceInstancePath;

    OVERLAPPED olRead;
    HANDLE hHidRead;
    HANDLE hHidWrite;

    BOOL   bConnected;

    BOOL ConnectDeviceInternal(const char* deviceInstancePath, bool log);
    void ClearReadDevice(const char* msg, bool log);

public:
    USHORT capInputReportLen;
    USHORT capOutputReportLen;
    USHORT capFeatureReportLen;

    HidDevice(void);
    ~HidDevice(void);

    void CloseFileHandles(void);

    BOOL ConnectDevice(const char* deviceStringPartial, const char* deviceStringFull, bool log = false);
    BOOL ConnectDevice(const char* deviceInstancePath, bool log = false);
    void DisconnectDevice(void);
    BOOL ReconnectDevice(bool log = false);
    BOOL IsConnected(void);
    DWORD WriteDevice(BYTE *buf, int len, bool log = false);
    DWORD ReadDevice(BYTE *buf, int len, int timeout_in_msec = -1, bool log = false);

    bool SendRecv(BYTE *sendBuf, int sendLen, BYTE *recvBuf, int recvLen, int timeout_in_msec = -1, bool log = false);

    std::string GetDeviceInstancePath()
    {
        return connectedDeviceInstancePath;
    }

    bool SetFeature(BYTE *buf, int len, bool log = false);
    bool GetFeature(BYTE *buf, int len, bool log = false);
};