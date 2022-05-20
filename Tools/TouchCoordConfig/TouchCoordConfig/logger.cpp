#pragma once

#include "stdafx.h"
#include "logger.h"
#include <stdlib.h>
#include <sstream>
#include <iomanip>
#include <stdarg.h>
#include <time.h>
#include <sys/timeb.h>
#include <chrono>
#include <shlwapi.h>
#include <shlobj_core.h>

#pragma warning(disable : 4996)

bool global_logger_enable_file_log        = false;
bool global_logger_enable_console_log     = true;
char global_logger_log_filename[MAX_PATH] = { 0 };

LogEnabledSetting GetConsoleLogEnabledSetting()
{
    LogEnabledSetting retVal = Log_Enabled;

    if (!global_logger_enable_console_log)
    {
        retVal = Log_Disabled;
    }

    return retVal;
}

LogEnabledSetting GetFileLoggingEnabledSetting()
{
    LogEnabledSetting retVal = Log_Enabled;

    if (!global_logger_enable_file_log)
    {
        retVal = Log_Disabled;
    }

    return retVal;
}

void SetLogToConsole(LogEnabledSetting setting)
{
    if (setting == Log_Enabled)
    {
        global_logger_enable_console_log = true;
    }
    else // Log_Disabled
    {
        global_logger_enable_console_log = false;
    }
}

void SetLogToFile(LogEnabledSetting setting)
{
    if (setting == Log_Enabled)
    {
        global_logger_enable_file_log = true;
    }
    else // Log_Disabled
    {
        global_logger_enable_file_log = false;
    }
}

bool SetupFileLog(const char* fileName)
{
    bool bRetValue = true;
    char filePath[MAX_PATH] = { 0 };

    //
    // Make sure we were passed a valid pointer
    //

    if (NULL == fileName)
    {
        Log("SetupFileLog(): ERROR: Invalid fileName passed.\n");
        bRetValue = false;
        goto Error;
    }

    strcpy_s(global_logger_log_filename, MAX_PATH, fileName);

    //
    // Strip out the filename that should have been passed in with the full path on the command line.
    //

    strcpy_s(filePath, MAX_PATH, global_logger_log_filename);
    if (!PathRemoveFileSpecA(filePath))
    {
        Log("SetupFileLog(): ERROR: PathRemoveFileSpecA failed!.  Path - '%s'\n", filePath);
        bRetValue = false;
        goto Error;
    }

    //
    // If the size of the path is 0 we assume only a file name was passed in WITHOUT a path,
    // so we'll create the file log in the current folder and can skip trying to create a folder.
    //

    if (0 != strlen(filePath))
    {
        //
        // Check to see if the specified logs folder exists.  If not, we'll try and create it.
        //

        if (!::PathFileExistsA(filePath))
        {
            Log("SetupFileLog(): PathFileExistsA() did not find folder '%s', creating.\n", filePath);

            int error = SHCreateDirectoryExA(NULL, filePath, NULL);
            if ((error != ERROR_SUCCESS) && (error != ERROR_FILE_EXISTS) && (error != ERROR_ALREADY_EXISTS))
            {
                Log("SetupFileLog(): ERROR: SHCreateDirectoryExA() failed - '%d'.\n", error);
                bRetValue = false;
                goto Error;
            }
        }
    }

    SetLogToFile(Log_Enabled);

Error:
    return bRetValue;
}

void LogToConsole(const char* szValue)
{
    if (!global_logger_enable_console_log)
    {
        return;
    }

    printf(szValue);
}

void HandleMaxLogSize()
{
    int openResult = 0;
    FILE* tf = NULL;
    const int bytesToSave    = 200 * 1024;         // Save the last 200k of the log
    const int maxLogFileSize = 20 * 1024 * 1024;   // Max log file size is 20MB

    //
    // Check the file size.  If it's larger than 'maxLogFileSize' we want to delete it
    // and create a new one so that we don't fill up the drive with log data.
    //

    if (::PathFileExistsA(global_logger_log_filename))
    {
        openResult = fopen_s(&tf, global_logger_log_filename, "rb");
        if (0 == openResult)
        {
            //
            // Get the file size
            //

            fseek(tf, 0, SEEK_END);
            size_t logFileSize = ftell(tf);

            if (logFileSize >= maxLogFileSize)
            {
                printf("HandleMaxLogSize(): '%s' is too large - '%zu', deleting.\n", global_logger_log_filename, logFileSize);

                //
                // To save some of the logging data, we'll read the last 'bytesToSave' from the
                // file so that we can put that at the beginning of the new log file, after it's been deleted.
                //

                BYTE buf[bytesToSave] = { 0 };
                fseek(tf, (long)logFileSize - (bytesToSave), SEEK_SET);
                fread(buf, sizeof(BYTE), bytesToSave, tf);
                fclose(tf);
                tf = NULL;

                //
                // Store the saved bytes back to a new log file
                //

                openResult = fopen_s(&tf, global_logger_log_filename, "wb");
                if (0 == openResult)
                {
                    fwrite(buf, sizeof(BYTE), bytesToSave, tf);
                }
            }

            fflush(tf);
            fclose(tf);
            tf = NULL;
        }
        else
        {
            global_logger_enable_file_log = false;
            printf("HandleMaxLogSize(): ERROR: Unable to open file '%s', file logging disabled.\n", global_logger_log_filename);
            return;
        }
    }
}

void LogToFile(const char* szValue, bool omitTime /*= false*/)
{
    int openResult = 0;
    char szTempTime[50] = { 0 };
    char szTime[50] = { 0 };
    struct tm* lTime;
    FILE* tf = NULL;

    if (!global_logger_enable_file_log)
    {
        return;
    }

    HandleMaxLogSize();

    openResult = fopen_s(&tf, global_logger_log_filename, "a+t");
    if (0 == openResult)
    {
        auto now = std::chrono::system_clock::now();
        auto nowAsTimeT = std::chrono::system_clock::to_time_t(now);
        auto nowMs = std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch()) % 1000;

        lTime = std::localtime(&nowAsTimeT);
        std::strftime(szTempTime, sizeof(szTempTime), "%D %T", lTime);
        sprintf_s(szTime, sizeof(szTime), "[%s.%03lld]", szTempTime, nowMs.count());

        if (omitTime)
        {
            fprintf_s(tf, "%s", szValue);
        }
        else
        {
            fprintf_s(tf, "%s: %s", szTime, szValue);
        }

        fflush(tf);
        fclose(tf);
        tf = NULL;
    }
    else
    {
        global_logger_enable_file_log = false;
        printf("LogToFile(): ERROR: Unable to open file '%s', file logging disabled.\n", global_logger_log_filename);
    }
}

void Log(const char* szValue, ...)
{
    char szBuffer[4096] = {};
    va_list argptr;
    va_start(argptr, szValue);
    if (-1 == (vsprintf(szBuffer, szValue, argptr)))
    {
        //
        // whatever
        //
    }
    va_end(argptr);

    LogToConsole(szBuffer);
    LogToFile(szBuffer);
}

void LogOmitTime(const char* szValue, ...)
{
    char szBuffer[4096] = {};
    va_list argptr;
    va_start(argptr, szValue);
    if (-1 == (vsprintf(szBuffer, szValue, argptr)))
    {
        //
        // whatever
        //
    }
    va_end(argptr);

    LogToConsole(szBuffer);
    LogToFile(szBuffer, true);
}