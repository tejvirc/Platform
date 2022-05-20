#pragma once

#include <stdlib.h>
#include <sstream>
#include <iomanip>
#include <stdarg.h>
#include <time.h>
#include <sys/timeb.h>
#include <debug.h>
#include <circularbuffer.h>
#include <chrono>
#include <shlobj_core.h>

enum LogTarget
{
    Debug,
    Console,
    File
};

static char global_logger_log_filename[] = "D:\\Aristocrat-VLT\\Platform\\logs\\Log_MonacoShell.log";

static inline void Log(LogTarget target, const char* szValue, ...);

inline bool SetupFileLog()
{
    bool bRetValue = true;
    char filePath[MAX_PATH] = { 0 };

    //
    // Strip out the filename so we are left with just the folder path.
    //

    strcpy_s(filePath, MAX_PATH, global_logger_log_filename);
    if (!PathRemoveFileSpecA(filePath))
    {
        Log(LogTarget::Console, "SetupFileLog(): ERROR: PathRemoveFileSpecA failed!.  Path - '%s'\n", global_logger_log_filename);
        bRetValue = false;
        goto Error;
    }

    //
    // If the size of the path is 0 we assume only a file name was passed in WITHOUT a path
    // so we'll create the file log in the current folder and can skip trying to create a folder.
    //

    if (0 != strlen(filePath))
    {
        //
        // Check to see if the specified logs folder exists.  If not, we'll try and create it.
        //

        if (!::PathFileExistsA(filePath))
        {
            Log(LogTarget::Console, "SetupFileLog(): PathFileExistsA() did not find folder '%s', creating.\n", filePath);

            int error = SHCreateDirectoryExA(NULL, filePath, NULL);
            if ((error != ERROR_SUCCESS) && (error != ERROR_FILE_EXISTS) && (error != ERROR_ALREADY_EXISTS))
            {
                Log(LogTarget::Console, "SetupFileLog(): ERROR: SHCreateDirectoryExA() failed - '%d'.\n", error);
                bRetValue = false;
                goto Error;
            }
        }
    }

Error:
    return bRetValue;
}

static inline void HandleMaxLogSize()
{
    int openResult = 0;
    FILE* tf = NULL;
    const int bytesToSave    = 200 * 1024;         // Save the last 200k of the log
    const int maxLogFileSize = 20 * 1024 * 1024;   // Max log file size is 20 MB

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
            size_t fsize = ftell(tf);

            if (fsize >= maxLogFileSize)
            {
                Log(LogTarget::Console, "HandleMaxLogSize(): '%s' is too large - '%zu', deleting.\n", global_logger_log_filename, fsize);

                //
                // To save some of the logging data, we'll read the last 'bytesToSave' from the
                // file so we can put that at the beginning of the new log file, after it's been deleted.
                //

                BYTE buf[bytesToSave] = { 0 };
                fseek(tf, (long)fsize - (bytesToSave), SEEK_SET);
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
            Log(LogTarget::Console, "HandleMaxLogSize(): ERROR: Unable to open file '%s'.\n", global_logger_log_filename);
            return;
        }
    }
}

static inline void LogRaw(LogTarget target, const char* szValue)
{
    if (target == LogTarget::Debug)
    {
    //
    // Output to Debug Window
    //

#if defined(WIN32)
        OutputDebugStringA(szValue);
        OutputDebugStringA("\n");
#else
        fprintf(stdout, szValue);
#endif // WIN32
    }
    else if (target == LogTarget::Console)
    {
        //
        // Output to Console
        //

#ifdef _DEBUG
        LogRaw(LogTarget::Debug, szValue);
#endif
    }
    else if (target == LogTarget::File)
    {
        //
        // Output to file
        //

        char szTempTime[50] = { 0 };
        char szTime[50] = { 0 };
        struct tm* lTime;
        FILE* tf = NULL;

        HandleMaxLogSize();

        if (0 == fopen_s(&tf, global_logger_log_filename, "a+t"))
        {
            auto now = std::chrono::system_clock::now();
            auto nowAsTimeT = std::chrono::system_clock::to_time_t(now);
            auto nowMs = std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch()) % 1000;

            lTime = std::localtime(&nowAsTimeT);
            std::strftime(szTempTime, sizeof(szTempTime), "%D %T", lTime);
            sprintf_s(szTime, sizeof(szTime), "[%s.%03lld]", szTempTime, nowMs.count());

            fprintf_s(tf, "%s: %s", szTime, szValue);
            fflush(tf);
            fclose(tf);
            tf = NULL;
        }
    }
}

static inline void Log(LogTarget target, const char* szValue, ...)
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

    LogRaw(target, szBuffer);
}

static inline void LogTime(LogTarget target, const char* szValue, ...)
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

    const int c_nTimeLength = 30;
    std::stringstream ss;
    timeb rawtime;
    ftime(&rawtime);

    time_t ltime;
    time(&ltime);
    struct tm* today = localtime(&ltime);
    char szTimeBuf[c_nTimeLength + 1];
    strftime(szTimeBuf, c_nTimeLength, "%Y-%m-%d %H:%M:%S", today);
    ss << szTimeBuf << "." << std::setfill('0') << std::setw(3) << rawtime.millitm <<  " ";
    ss << szBuffer;

    LogRaw(target, szBuffer);
}