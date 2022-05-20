//------------------------------------------------
//
// Copyright(c) 2015 Kortek. All rights reserved.
//
//   Hugh Chang, chk@kortek.co.kr
//
//   $Id: DataFlash.cpp 4241 2016-08-29 02:19:42Z chk $
//
//------------------------------------------------

#include "stdafx.h"
#include "DataFlash.h"
#include "CommunicationProtocol.h"
#include "Utils.h"

bool CDataFlash::WritePage(HidDevice& hidDevice, unsigned int addr, unsigned int len, unsigned char *buf, int previouslyWrittenLen)
{
    struct HidReportVendor cmd(VENDOR_CMD_DATA_FLASH_WRITE), resultData(0);

    cmd.startAddress[0] = 0xff & addr;
    cmd.startAddress[1] = (0xff00 & addr) >> 8;
    cmd.len = 0xff;
    cmd.response = 0xff & len;
    cmd.dummy = (0xff00 & len) >> 8;

    int remainingLen = len;
    int bufIndex = 0;
    int partialLen;
        
    while (remainingLen > 0)
    {
        partialLen = min(sizeof(cmd.buf), remainingLen);
        memcpy_s(cmd.buf, sizeof(cmd.buf), &buf[bufIndex], partialLen);
        bufIndex += partialLen;
        remainingLen -= partialLen;

        if (hidDevice.WriteDevice((BYTE *)&cmd, sizeof(cmd)) == -1)
            return false;

        Sleep(USB_CONTINUOUS_DATA_SEND_DELAY_MSEC);

        cmd.cmd = VENDOR_CMD_DATA_FLASH_WRITE_CONTINUE;
    }

    if (hidDevice.ReadDevice((BYTE *)&resultData, sizeof(resultData)) == -1)
        return false;
    else if (resultData.response == VENDOR_CMD_RESPONSE_NOK)
        return false;
    else
        return true;
}

bool CDataFlash::Read(HidDevice& hidDevice, unsigned int addr, unsigned int len, unsigned char *buf)
{
    struct HidReportVendor cmd(VENDOR_CMD_DATA_FLASH_READ), resultData(0);

    int remainingLen = len;
    int bufIndex = 0;
    int partialLen;

    while (remainingLen > 0)
    {
        partialLen = min(sizeof(cmd.buf), remainingLen);

        cmd.startAddress[0] = 0xff & addr;
        cmd.startAddress[1] = (0xff00 & addr) >> 8;
        cmd.len = 0xff;
        cmd.response = 0xff & partialLen;
        cmd.dummy = (0xff00 & partialLen) >> 8;

        if (hidDevice.SendRecv((BYTE *)&cmd, sizeof(cmd), (BYTE *)&resultData, sizeof(resultData)) == false)
            return false;
        else if (resultData.response == VENDOR_CMD_RESPONSE_NOK)
            return false;

        memcpy_s(&buf[bufIndex], partialLen, resultData.buf, partialLen);

        Sleep(USB_CONTINUOUS_DATA_SEND_DELAY_MSEC);

        bufIndex += partialLen;
        remainingLen -= partialLen;
        addr += partialLen;
    }

    return true;
}

bool CDataFlash::WritePages(HidDevice& hidDevice, unsigned int addr, unsigned int len, unsigned char *buf, unsigned int pageSize)
{
    unsigned int totalLen = len;

    while (len > 0)
    {
        unsigned char bufPage[DATA_FLASH_PAGE_SIZE_48K];
        if (len < pageSize)
        {
            ZeroMemory(bufPage, pageSize);
            CopyMemory(bufPage, buf, len);
        }
        else
            CopyMemory(bufPage, buf, pageSize);

        if (WritePage(hidDevice, addr, pageSize, bufPage, totalLen - len) == false)
            return false;

        if (len < pageSize)
            break;

        addr += pageSize;
        len  -= pageSize;
        buf  += pageSize;
    }

    return true;
}

bool CDataFlash::ReadStructs(HidDevice& hidDevice, struct DataFlashBasicInfo *dataFlashBasicInfo, struct DataFlashTouchFunc *dataFlashTouchFunc, struct DataFlashLog *dataFlashLog, struct TaskInfo *taskInfo)
{
    if (dataFlashBasicInfo != NULL)
    {
        if (Read(hidDevice, DATAFLASH_BASIC_INFO_OFFSET, sizeof(struct DataFlashBasicInfo), (unsigned char *) dataFlashBasicInfo) == false)
            return false;
    }

    if (dataFlashTouchFunc != NULL)
    {
        if (Read(hidDevice, DATAFLASH_TOUCH_FUNC_OFFSET, sizeof(struct DataFlashTouchFunc), (unsigned char *) dataFlashTouchFunc) == false)
            return false;
    }

    if (dataFlashLog != NULL)
    {
        if (Read(hidDevice, DATAFLASH_LOG_OFFSET, sizeof(struct DataFlashLog), (unsigned char *) dataFlashLog) == false)
            return false;
    }

    if (taskInfo != NULL)
    {
        if (Read(hidDevice, TASKINFO_OFFSET, sizeof(struct TaskInfo), (unsigned char *) taskInfo) == false)
            return true;
    }

    return true;
}

bool CDataFlash::WriteStructs(HidDevice& hidDevice, struct DataFlashBasicInfo *dataFlashBasicInfo, struct DataFlashTouchFunc *dataFlashTouchFunc, struct DataFlashLog *dataFlashLog)
{
    unsigned char buf[DATA_FLASH_PAGE_SIZE];

    if (dataFlashBasicInfo != NULL)
    {
        FillMemory(buf, sizeof(buf), 0xff);
        CopyMemory(buf, dataFlashBasicInfo, sizeof(struct DataFlashBasicInfo));
        if (WritePages(hidDevice, DATAFLASH_BASIC_INFO_OFFSET, sizeof(buf), buf) == false)
            return false;
    }

    if (dataFlashTouchFunc != NULL)
    {
        FillMemory(buf, sizeof(buf), 0xff);
        CopyMemory(buf, dataFlashTouchFunc, sizeof(struct DataFlashTouchFunc));
        if (WritePages(hidDevice, DATAFLASH_TOUCH_FUNC_OFFSET, sizeof(buf), buf) == false)
            return false;
    }

    if (dataFlashLog != NULL)
    {
        FillMemory(buf, sizeof(buf), 0xff);
        CopyMemory(buf, dataFlashLog, sizeof(struct DataFlashLog));
        if (WritePages(hidDevice, DATAFLASH_LOG_OFFSET, sizeof(buf), buf) == false)
            return false;
    }

    return true;
}

bool CDataFlash::ReadStructsOld(HidDevice& hidDevice, struct DataFlashOld *dataFlashOld)
{
    if (dataFlashOld != NULL)
    {
        if (Read(hidDevice, DATAFLASH_BASIC_INFO_OFFSET, sizeof(struct DataFlashOld), (unsigned char *) dataFlashOld) == false)
            return false;
    }

    return true;
}

bool CDataFlash::WriteStructsOld(HidDevice& hidDevice, struct DataFlashOld *dataFlashOld)
{
    unsigned char buf[DATA_FLASH_PAGE_SIZE];

    if (dataFlashOld != NULL)
    {
        FillMemory(buf, sizeof(buf), 0xff);
        CopyMemory(buf, dataFlashOld, sizeof(struct DataFlashOld));
        if (WritePages(hidDevice, DATAFLASH_BASIC_INFO_OFFSET, sizeof(buf), buf) == false)
            return false;
    }

    return true;
}