//------------------------------------------------
//
// Copyright(c) 2015 Kortek. All rights reserved.
//
//   Hugh Chang, chk@kortek.co.kr
//
//   $Id: HidDevice.cpp 4241 2016-08-29 02:19:42Z chk $
//
//------------------------------------------------

#include "stdafx.h"
#include "HidDevice.h"
#include "Utils.h"
#include "logger.h"
#include <algorithm>

HidDevice::HidDevice(void)
{
    memset(&olRead, 0, sizeof(olRead));

    hHidRead  = INVALID_HANDLE_VALUE;
    hHidWrite = INVALID_HANDLE_VALUE;

    CloseFileHandles();

    olRead.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
}

HidDevice::~HidDevice(void)
{
    CloseFileHandles();

    if (olRead.hEvent != NULL)
    {
        CloseHandle(olRead.hEvent);
        olRead.hEvent = NULL;
    }
}

void HidDevice::CloseFileHandles(void)
{
    if (hHidRead != INVALID_HANDLE_VALUE)
    {
        CloseHandle(hHidRead);
        hHidRead  = INVALID_HANDLE_VALUE;
    }
    
    if (hHidWrite != INVALID_HANDLE_VALUE)
    {
        CloseHandle(hHidWrite);
        hHidWrite = INVALID_HANDLE_VALUE;
    }

    connectedDeviceInstancePath = "";
    capInputReportLen  = 0;
    capOutputReportLen = 0;

    bConnected = FALSE;
}

BOOL HidDevice::ConnectDeviceInternal(const char* deviceInstancePath, bool log)
{
    DisconnectDevice();

    hHidRead = ::CreateFileA(deviceInstancePath, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, 
        OPEN_EXISTING, FILE_FLAG_OVERLAPPED, 0);
    if (hHidRead == INVALID_HANDLE_VALUE) 
    {
        if (log)
        {
            Log("HidDevice::ConnectDeviceInternal(): ERROR - CreateFile(Read) : %s", Utils::errorCodeToString(GetLastError()));
        }
        CloseFileHandles();
        return FALSE;
    }
    
    hHidWrite = ::CreateFileA(deviceInstancePath, GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, 
        OPEN_EXISTING, 0, 0);
    if (hHidWrite == INVALID_HANDLE_VALUE) 
    {
        if (log)
        {
            Log("HidDevice::ConnectDeviceInternal(): ERROR - CreateFile(Write) : %s", Utils::errorCodeToString(GetLastError()));
        }
        CloseFileHandles();
        return FALSE;
    }

    if (log)
    {
        Log("HidDevice::ConnectDeviceInternal(): CreateFile: hHidRead %p hHidWrite %p", hHidRead, hHidWrite);
    }

    PHIDP_PREPARSED_DATA preparsedData;
    if (!HidD_GetPreparsedData(hHidRead, &preparsedData)) {
        if (log)
        {
            Log("HidDevice::ConnectDeviceInternal(): ERROR - CreateFile: HidD_GetPreparsedData() failed");
        }
        CloseFileHandles();
        return FALSE;
    }

    HIDP_CAPS capabilities;
    if (HidP_GetCaps(preparsedData, &capabilities) != HIDP_STATUS_SUCCESS)
    {
        if (log)
        {
            Log("HidDevice::ConnectDeviceInternal(): ERROR - CreateFile: HidP_GetCaps() failed");
        }
        CloseFileHandles();
        return FALSE;
    }

    capInputReportLen  = capabilities.InputReportByteLength;
    capOutputReportLen = capabilities.OutputReportByteLength;
    capFeatureReportLen = capabilities.FeatureReportByteLength;

    if (log)
    {
        Log("HidDevice::ConnectDeviceInternal() - CreateFile: capInputReportLen %d capOutputReportLen %d capFeatureReportLen %d", capInputReportLen, capOutputReportLen, capFeatureReportLen);
    }

    connectedDeviceInstancePath = deviceInstancePath;
    bConnected = TRUE;

    return TRUE;
}

BOOL HidDevice::ConnectDevice(const char* deviceStringPartial, const char* deviceStringFull, bool log)
{
    std::string devicePathFound;

    if (log)
    {
        Log("HidDevice::ConnectDevice(): Looking for [%s]", deviceStringFull);
    }

    GUID hidClassGUID;
    HidD_GetHidGuid(&hidClassGUID);

    HDEVINFO devInfoSet = INVALID_HANDLE_VALUE;
    devInfoSet = SetupDiGetClassDevs(&hidClassGUID, NULL, NULL, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

    for (DWORD memberIndex = 0; ; memberIndex++)
    {
        SP_DEVICE_INTERFACE_DATA deviceInterfaceData;
        deviceInterfaceData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
        BOOL bSuccess = SetupDiEnumDeviceInterfaces(devInfoSet, NULL, &hidClassGUID, memberIndex, &deviceInterfaceData);
        if (bSuccess == FALSE) // GetLastError() == ERROR_NO_MORE_ITEMS
            break;

        DWORD requiredSize;
        SetupDiGetDeviceInterfaceDetailA(devInfoSet, &deviceInterfaceData, NULL, NULL, &requiredSize, NULL);
        PSP_DEVICE_INTERFACE_DETAIL_DATA_A pInterfaceDetailData = (PSP_DEVICE_INTERFACE_DETAIL_DATA_A) malloc(requiredSize);

        pInterfaceDetailData->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_A);
        SetupDiGetDeviceInterfaceDetailA(devInfoSet, &deviceInterfaceData, pInterfaceDetailData, requiredSize, NULL, NULL);

        std::string devicePath = pInterfaceDetailData->DevicePath;
        
        std::transform(devicePath.begin(), devicePath.end(), devicePath.begin(), ::tolower);
        if (
            Utils::FindStringWithAsterisk(devicePath, deviceStringFull)) {
            devicePathFound = devicePath;
            if (log)
            {
                LogOmitTime("    * (%02d) %s", memberIndex, devicePath.c_str());
            }
        }
        else if (Utils::FindStringWithAsterisk(devicePath, deviceStringPartial)) {
            if (log)
            {
                LogOmitTime("    (%02d) %s", memberIndex, devicePath.c_str());
            }
        }

        if (pInterfaceDetailData != NULL)
        {
            free(pInterfaceDetailData);
            pInterfaceDetailData = NULL;
        }
    }

    if (devInfoSet != INVALID_HANDLE_VALUE)
    {
        SetupDiDestroyDeviceInfoList(devInfoSet);
    }

    if (devicePathFound == "")
        return FALSE;

    return ConnectDeviceInternal(devicePathFound.c_str(), log);
}

BOOL HidDevice::ConnectDevice(const char* deviceInstancePath, bool log)
{
    return ConnectDeviceInternal(deviceInstancePath, log);
}

void HidDevice::DisconnectDevice(void)
{
    CloseFileHandles();
}

BOOL HidDevice::ReconnectDevice(bool log)
{
    std::string deviceInstancePath = connectedDeviceInstancePath;
    DisconnectDevice();
    return ConnectDevice(deviceInstancePath.c_str(), log);
}

BOOL HidDevice::IsConnected(void)
{
    return bConnected;
}

DWORD HidDevice::WriteDevice(BYTE *buf, int len, bool log)
{
    DWORD numBytesWritten = 0;

    if (WriteFile(hHidWrite, buf, len, &numBytesWritten, NULL))
    {
        if (log)
        {
            Log("HidDevice::WriteDevice(): WriteFile %d", numBytesWritten);
        }
        return numBytesWritten;
    }
    else
    {
        if (log)
        {
            Log("HidDevice::WriteDevice(): ERROR - %s", Utils::errorCodeToString(GetLastError()));
        }
        return -1;
    }
}

void HidDevice::ClearReadDevice(const char* msg, bool log)
{
    if (CancelIo(hHidRead) == FALSE)
    {
        Log("HidDevice::ClearReadDevice(): ERROR - CancelIo : %s", Utils::errorCodeToString(GetLastError()));
    }

    if (log)
    {
        Log("HidDevice::ClearReadDevice(): %s", msg);
    }

    ZeroMemory(&olRead, FIELD_OFFSET(OVERLAPPED, hEvent));
    ResetEvent(olRead.hEvent);
}

DWORD HidDevice::ReadDevice(BYTE *buf, int len, int timeout_in_msec, bool log)
{
    DWORD numBytesRead = 0;

    if (ReadFile(hHidRead, buf, len, &numBytesRead, &olRead))
    {
        if (log)
        {
            Log("HidDevice::ReadDevice(): %s, %d", "ReadFile", numBytesRead);
        }
        ZeroMemory(&olRead, FIELD_OFFSET(OVERLAPPED, hEvent));
        ResetEvent(olRead.hEvent);
        return numBytesRead;
    }
    else
    {
        if (GetLastError() == ERROR_IO_PENDING)
        {
            int ret = WaitForSingleObject(olRead.hEvent, timeout_in_msec); 
            if (ret == WAIT_OBJECT_0)
            {  
                if (GetOverlappedResult(hHidRead, &olRead, &numBytesRead, TRUE))
                {
                    if (log)
                    {
                        Log("HidDevice::ReadDevice(): %s, %d", "GetOverlappedResult", numBytesRead);
                    }
                    ZeroMemory(&olRead, FIELD_OFFSET(OVERLAPPED, hEvent));
                    ResetEvent(olRead.hEvent);
                    return numBytesRead;
                }
                else
                {
                    ClearReadDevice(Utils::errorCodeToString(GetLastError()), log);
                    return -1;
                }
            }
            else if (ret == WAIT_TIMEOUT)
            {
                ClearReadDevice("WAIT_TIMEOUT", log);
                return 0;
            }
            else
            {
                ClearReadDevice(Utils::errorCodeToString(GetLastError()), log);
                return -1;
            }
        }
        else
        {
            ClearReadDevice(Utils::errorCodeToString(GetLastError()), log);
            return -1;
        }
    }
}

bool HidDevice::SendRecv(BYTE *sendBuf, int sendLen, BYTE *recvBuf, int recvLen, int timeout_in_msec, bool log)
{
    if (WriteDevice(sendBuf, sendLen, log) == -1)
        return false;

    if (ReadDevice(recvBuf, recvLen, timeout_in_msec, log) == -1)
        return false;

    return true;
}

bool HidDevice::SetFeature(BYTE *buf, int len, bool log)
{
    if (HidD_SetFeature(hHidWrite, buf, len))
    {
        if (log)
        {
            Log("HidDevice::SetFeature(): %d", len);
        }
        return true;
    }
    else
    {
        if (log)
        {
            Log("HidDevice::SetFeature(): ERROR - %s", Utils::errorCodeToString(GetLastError()));
        }
        return false;
    }
}

bool HidDevice::GetFeature(BYTE *buf, int len, bool log)
{
    if (HidD_GetFeature(hHidRead, buf, len))
    {
        if (log)
        {
            Log("HidDevice::GetFeature(): %d", len);
        }
        return true;
    }
    else
    {
        if (log)
        {
            Log("HidDevice::GetFeature(): ERROR - %s", Utils::errorCodeToString(GetLastError()));
        }
        return false;
    }
}