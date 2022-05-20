#pragma once

enum LogEnabledSetting
{
    Log_Enabled  = 0,
    Log_Disabled = 1
};

LogEnabledSetting GetConsoleLogEnabledSetting();
LogEnabledSetting GetFileLoggingEnabledSetting();
void SetLogToConsole(LogEnabledSetting setting);
void SetLogToFile(LogEnabledSetting setting);
void HandleMaxLogSize();
void Log(const char* szValue, ...);
void LogOmitTime(const char* szValue, ...);
void LogToConsole(const char* szValue);
void LogToFile(const char* szValue, bool omitTime = false);
bool SetupFileLog(const char* fileName);