//------------------------------------------------
//
// Copyright(c) 2015 Kortek. All rights reserved.
//
//   Hugh Chang, chk@kortek.co.kr
//
//   $Id: DataFlash.h 9393 2019-10-08 20:10:44Z chk $
//
//------------------------------------------------

#pragma once

#include "HidDevice.h"
#include "logger.h"

struct DataFlashBasicInfo
{
    unsigned char   board_name[32];
    unsigned char   board_version[16];
    unsigned char   master_version[16];
    unsigned char   slave_version[16];
    int             oem;
    unsigned char   inch[16];
    int             panel_type;
    unsigned char   date[16];

    unsigned char   gau8VendorStringDescriptor[32];
    unsigned char   gau8ProductStringDescriptor[32];
    unsigned char   gau8StringInterface0[32];
    unsigned char   gau8StringInterface1[48];
    unsigned char   gau8StringInterface2[32];
    unsigned char   gau8StringSerial[48];

    int             delayUSBInitTime;
    int             idTouchChip;
    int             timeoutKeepAlive;

    int             usbPid;
};

struct DataFlashTouchFunc
{
    unsigned int    reset_time_out;
    int             xflip;
    int             yflip;
    int             flagXScaling;
    int             xScaling;
    int             xShift;
    int             flagYScaling;
    int             yScaling;
    int             yShift;
    int             flagEdgeExpansion;
    int             xExpandRate;
    int             yExpandRate;
    int             xChannelsRx;
    int             yChannelsTx;
    int             xEdgeChannelsTenTimes;
    int             yEdgeChannelsTenTimes;
    int             uartEmulate;
    int             maxTouchCount;
    int             initialDeviceMode;
    int             flagCurveFitting;
    int             curveFittingNumOfPoints;
    int             enableRemoteWakeup;
    int             xySwitch;
    int             touchAreaLeftTopX;
    int             touchAreaLeftTopY;
    int             touchAreaRightBottomX;
    int             touchAreaRightBottomY;

    void print()
    {
        Log("\n");
        Log("Kortek Device Information:\n\n");
        Log("  Variable                | Value\n");
        Log("  ------------------------+-------------\n");
        Log("  reset_time_out          | %d\n", reset_time_out);
        Log("  xflip                   | %d\n", xflip);
        Log("  yflip                   | %d\n", yflip);
        Log("  flagXScaling            | %d\n", flagXScaling);
        Log("  xScaling                | %d\n", xScaling);
        Log("  xShift                  | %d\n", xShift);
        Log("  flagYScaling            | %d\n", flagYScaling);
        Log("  yScaling                | %d\n", yScaling);
        Log("  yShift                  | %d\n", yShift);
        Log("  flagEdgeExpansion       | %d\n", flagEdgeExpansion);
        Log("  xExpandRate             | %d\n", xExpandRate);
        Log("  yExpandRate             | %d\n", yExpandRate);
        Log("  xChannelsRx             | %d\n", xChannelsRx);
        Log("  yChannelsTx             | %d\n", yChannelsTx);
        Log("  xEdgeChannelsTenTimes   | %d\n", xEdgeChannelsTenTimes);
        Log("  yEdgeChannelsTenTimes   | %d\n", yEdgeChannelsTenTimes);
        Log("  uartEmulate             | %d\n", uartEmulate);
        Log("  maxTouchCount           | %d\n", maxTouchCount);
        Log("  initialDeviceMode       | %d\n", initialDeviceMode);
        Log("  flagCurveFitting        | %d\n", flagCurveFitting);
        Log("  curveFittingNumOfPoints | %d\n", curveFittingNumOfPoints);
        Log("  enableRemoteWakeup      | %d\n", enableRemoteWakeup);
        Log("  xySwitch                | %d\n", xySwitch);
        Log("  touchAreaLeftTopX       | %d\n", touchAreaLeftTopX);
        Log("  touchAreaLeftTopY       | %d\n", touchAreaLeftTopY);
        Log("  touchAreaRightBottomX   | %d\n", touchAreaRightBottomX);
        Log("  touchAreaRightBottomY   | %d\n", touchAreaRightBottomY);
        Log("\n");
    }
};

struct DataFlashLog 
{
    unsigned int countResetUSB;
    unsigned int statusUSB;
    unsigned int countResetTouch;
    unsigned int  extraValue[3];
    unsigned int  extraDelta[7];
};

struct DataFlashOld
{
    unsigned int   reset_time_out;
    unsigned char  board_name[32];
    unsigned char  board_version[16]; 
    unsigned char  master_version[16];
    unsigned char  slave_version[16];
    int            oem;
    unsigned char  inch[16];
    int            panel_type;
    unsigned char  date[16];
    int            xflip;
    int            yflip;

    // Edge Expansion
    int            flagEdgeExpansion;
    int            xExpandRate;
    int            yExpandRate;
    int            xChannelsRx;
    int            yChannelsTx;
    int            xEdgeChannelsTenTimes;
    int            yEdgeChannelsTenTimes;

    int            uartEmulate;
    int            maxTouchCount;
    int            initialDeviceMode;
    int            flagCurveFitting;
    int            curveFittingNumOfPoints;
    int            flagRemoteWakeup;

    void print()
    {
        Log("\n");
        Log("Kortek Device Information:\n\n");
        Log("  Variable       | Value\n");
        Log("  ---------------+-------------\n");
        Log("  board name     | %s\n", board_name);
        Log("  board version  | %s\n", board_version);
        Log("  master version | %s\n", master_version);
        Log("  slave version  | %s\n", slave_version);
        Log("  oem            | %d\n", oem);
        Log("  panel type     | %d\n", panel_type);
        Log("  date           | %c\n", date[0]);
        Log("  xflip          | %d\n", xflip);
        Log("  yflip          | %d\n", yflip);
        Log("\n");
    }
};

#define TASK_INFO_UART          0
#define TASK_INFO_USB_DEVICE    1
#define TASK_INFO_USB_HOST      2
#define TASK_INFO_SPLIT_TOUCH   3
#define TASK_INFO_IDLE          4
#define TASK_INFO_MAX           5

struct TaskInfo {
    struct {
        void*               taskHandle;
        void*               queueSet;
        void*               queueRx;
        void*               queueTx;
        int                 stackHighWaterMark;
        unsigned int        executionCount;
        unsigned int        interruptCount;
    } ti[TASK_INFO_MAX];
    int                     freeHeapSize;
    int                     usbSwitchMode;
};

class CDataFlash
{
public:
    static const int DATAFLASH_BASIC_INFO_OFFSET    = 0;
    static const int DATAFLASH_TOUCH_FUNC_OFFSET    = 1024;
    static const int DATAFLASH_LOG_OFFSET           = 1024*3;
    static const int TASKINFO_OFFSET                = 0xff00;

private:
    static bool WritePage(HidDevice& hidDevice, unsigned int addr, unsigned int len, unsigned char *buf, int previouslyWrittenLen);

public:
    static const unsigned int DATA_FLASH_PAGE_SIZE      = 512;
    static const unsigned int DATA_FLASH_PAGE_SIZE_48K  = 1024*48;

    static bool Read(HidDevice& hidDevice, unsigned int addr, unsigned int len, unsigned char *buf);
    static bool WritePages(HidDevice& hidDevice, unsigned int addr, unsigned int len, unsigned char *buf, unsigned int pageSize = DATA_FLASH_PAGE_SIZE);

    static bool ReadStructs(HidDevice& hidDevice, struct DataFlashBasicInfo *dataFlashBasicInfo, struct DataFlashTouchFunc *dataFlashTouchFunc, struct DataFlashLog *dataFlashLog, struct TaskInfo *taskInfo = NULL);
    static bool WriteStructs(HidDevice& hidDevice, struct DataFlashBasicInfo *dataFlashBasicInfo, struct DataFlashTouchFunc *dataFlashTouchFunc, struct DataFlashLog *dataFlashLog);

    static bool ReadStructsOld(HidDevice& hidDevice, struct DataFlashOld *dataFlashOld);
    static bool WriteStructsOld(HidDevice& hidDevice, struct DataFlashOld *dataFlashOld);
};