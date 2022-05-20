//------------------------------------------------
//
// Copyright(c) 2015 Kortek. All rights reserved.
//
//   Hugh Chang, chk@kortek.co.kr
//
//   $Id: CommunicationProtocol.h 2394 2015-09-30 07:35:38Z chk $
//
//------------------------------------------------

#pragma once

const unsigned int USB_CONTINUOUS_DATA_SEND_DELAY_MSEC  = 3;

const BYTE  REPORTID_VENDOR                             = 0x76;
const BYTE  VENDOR_MAGIC                                = 0x77;
const BYTE  VENDOR_CMD_I2C_READ                         = 0x78;
const BYTE  VENDOR_CMD_GET_FIRMWARE_VERSION             = 0x7f;
const BYTE  VENDOR_CMD_DATA_FLASH_READ                  = 0x80;
const BYTE  VENDOR_CMD_DATA_FLASH_WRITE                 = 0x81;
const BYTE  VENDOR_CMD_DATA_FLASH_WRITE_CONTINUE        = 0x82;
const BYTE  VENDOR_CMD_SET_LDROM                        = 0x83;
const BYTE  VENDOR_CMD_GET_BOARD_JUMPER_VALUE           = 0x84;
const BYTE  VENDOR_CMD_RESET_APROM                      = 0x85;
const BYTE  VENDOR_CMD_RESPONSE_OK                      = 0x01;
const BYTE  VENDOR_CMD_RESPONSE_NOK                     = 0xff;

struct HidReportVendor {
    BYTE    reportID;
    BYTE    vendorMagic; 
    BYTE    cmd;
    BYTE    startAddress[2];
    BYTE    len;
    BYTE    response;
    BYTE    dummy;
    BYTE    buf[56];

    HidReportVendor(BYTE cmdToBeSet) 
    {
        reportID    = REPORTID_VENDOR;
        vendorMagic = VENDOR_MAGIC;
        cmd         = cmdToBeSet;
        ZeroMemory(startAddress, sizeof(startAddress));
        len         = 0;
        response    = 0;
        dummy       = 0;
        ZeroMemory(buf, sizeof(buf));
    }
};

const unsigned int  HITOUCH_HEADER_NUC122                       = 0x12212276;
const unsigned int  HITOUCH_MAGIC_START_COMMAND                 = 0xDEEB1234;
const unsigned int  HITOUCH_MAGIC_END_COMMAND                   = 0xDEEB9999;
const unsigned int  HITOUCH_MAGIC_START_COMMAND_RESULT          = 0xDEEC1357;
const unsigned int  HITOUCH_MAGIC_END_COMMAND_RESULT            = 0xDEEC2468;
const unsigned int  HITOUCH_MAGIC_START_DATA                    = 0xDEEDABCD;
const unsigned int  HITOUCH_MAGIC_END_DATA                      = 0xDEED0F0F;
const unsigned int  HITOUCH_CMD_FW_DOWNLOAD_START               = 0x000000D0;
const unsigned int  HITOUCH_CMD_FW_DOWNLOAD_END                 = 0x000000D1;
const unsigned int  HITOUCH_CMD_DUKE_CONTROL_B                  = 0x00000008;
const unsigned int  HITOUCH_CMD_DOWN_FIRMWARE                   = 0x00000007;
const unsigned int  HITOUCH_CMD_DOWN_SLAVE_FIRMWARE             = 0x0000008e;
const unsigned int  HITOUCH_CMD_RESET_ISTXXX                    = 0x000000fe;
const unsigned int  HITOUCH_CMD_RESULT_OK                       = 0x00;
const unsigned int  HITOUCH_CMD_RESULT_FAIL                     = 0x01;
const unsigned char HITOUCH_DCB_VTBLASTER_ENTRY_IST700          = 0x90;
const unsigned char HITOUCH_DCB_VTBLASTER_ENTRY_IST770_SLAVE    = 0x91;
const unsigned char HITOUCH_DCB_RESETB_IST770_SLAVE             = 0x92;

const unsigned int  HITOUCH_ID_CODE_ADDR                        = 0x0238;
const unsigned int  HITOUCH_ID_CODE_DATA                        = 0x0bb11477;
const unsigned int  HITOUCH_ONCE_WRITE_BYTE                     = 52;
const unsigned int  HITOUCH_VERSION_ADDR_MASTER                 = 0x023C;
const unsigned int  HITOUCH_VERSION_ADDR_SLAVE_LEFT             = 0x0230;
const unsigned int  HITOUCH_VERSION_ADDR_SLAVE_RIGHT            = 0x0234;
const unsigned int  HITOUCH_UDEFINE_ADDR_MASTER                 = 0x0090;
const unsigned int  HITOUCH_UDEFINE_ADDR_SLAVE_LEFT             = 0x0098;
const unsigned int  HITOUCH_UDEFINE_ADDR_SLAVE_RIGHT            = 0x00A0;
const unsigned int  HITOUCH_VRCRC16_ADDR_MASTER                 = 0x00A8;
const unsigned int  HITOUCH_VRCRC16_ADDR_SLAVE_LEFT             = 0x00AA;
const unsigned int  HITOUCH_VRCRC16_ADDR_SLAVE_RIGHT            = 0x00AC;
const unsigned int  HITOUCH_CHIP_REVISION_ADDR                  = 0x022e;
const unsigned int  HITOUCH_SLAVE_CHECKSUM_ADDR                 = 0x0038;

struct HidReportHiTouchInspectorCommand {
    unsigned int    headerNUC122;       // HITOUCH_HEADER_NUC122
    unsigned int    StartMagicWord;     // HITOUCH_MAGIC_START_COMMAND
    unsigned int    code;               // HITOUCH_CMD_...
    unsigned short  DataLenToBeSent;
    unsigned short  DataLenToBeRead;
    unsigned char   AddInfo[12];
    unsigned char   buf[32];
    unsigned int    EndMagicWord;       // HITOUCH_MAGIC_END_COMMAND

    HidReportHiTouchInspectorCommand() 
    {
        headerNUC122 = HITOUCH_HEADER_NUC122;
        StartMagicWord = HITOUCH_MAGIC_START_COMMAND;
        code = 0;
        DataLenToBeSent = 0;
        DataLenToBeRead = 0;
        ZeroMemory(AddInfo, sizeof(AddInfo));
        ZeroMemory(buf, sizeof(buf));
        EndMagicWord = HITOUCH_MAGIC_END_COMMAND;
    }
};

struct HidReportHiTouchInspectorCommandResult {
    unsigned int    headerNUC122;       // HITOUCH_HEADER_NUC122
    unsigned int    StartMagicWord;     // HITOUCH_MAGIC_START_COMMAND_RESULT
    unsigned int    code;
    unsigned int    ResultCode;         // HITOUCH_CMD_RESULT_OK, HITOUCH_CMD_RESULT_FAIL
    unsigned char   AddInfo[12];
    unsigned int    EndMagicWord;       // HITOUCH_MAGIC_END_COMMAND_RESULT
    unsigned char   dummy[32];
};

struct HidReportHiTouchInspectorData {
    unsigned int    headerNUC122;       // HITOUCH_HEADER_NUC122
    unsigned int    StartMagicWord;     // HITOUCH_MAGIC_START_DATA
    unsigned char   buf[HITOUCH_ONCE_WRITE_BYTE];
    unsigned int    EndMagicWord;       // HITOUCH_MAGIC_END_DATA
    
    HidReportHiTouchInspectorData() 
    {
        headerNUC122 = HITOUCH_HEADER_NUC122;
        StartMagicWord = HITOUCH_MAGIC_START_DATA;
        ZeroMemory(buf, sizeof(buf));
        EndMagicWord = HITOUCH_MAGIC_END_DATA;
    }
};