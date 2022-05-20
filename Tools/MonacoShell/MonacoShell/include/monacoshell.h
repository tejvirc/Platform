#pragma once

#include <platform.h>

//
// Graphics Interfaces
//

class IShellContext : public virtual aristocrat::Interface
{
public:
     MONACO_INTERFACE_ID(IShellContext, 0xabcf12f0, 0xf487, 0x426e, 0xbd, 0x35, 0x1d, 0xfc, 0x31, 0x11, 0xfc, 0x9c);
    virtual ~IShellContext() {}
    virtual void StartApplication(const char* applicationName) = 0;
    virtual void StopApplication(const char* applicationName) = 0;
};

//
// Graphics Interfaces
//

class IShellContextService : public virtual aristocrat::Interface
{
public:
     MONACO_INTERFACE_ID(IShellContextService, 0xabcf12f0, 0xf487, 0x426e, 0xbd, 0x35, 0x1d, 0xfc, 0x31, 0x11, 0xfc, 0x9c);
    virtual ~IShellContextService() {}
    virtual void Initialize(IShellContext* shellContext) = 0;
};

class IDataReceiver : public virtual aristocrat::Interface
{
public:
     MONACO_INTERFACE_ID(IDataReceiver, 0xeb0f12f1, 0xf487, 0x426e, 0xbd, 0x35, 0x1d, 0xfc, 0x31, 0x11, 0xfc, 0x9c);

    virtual ~IDataReceiver() {}
    virtual void OnData(DWORD id, const char* data, size_t length) = 0;
};

class IWindow : public virtual aristocrat::Interface
{
public:
     MONACO_INTERFACE_ID(IWindow, 0xcdabf1f1, 0xf487, 0x426e, 0xbd, 0x35, 0x1d, 0xfc, 0x31, 0x11, 0xfc, 0x9c);

    virtual ~IWindow() {}
    virtual void Invalidate() = 0;
    virtual int Width() = 0;
    virtual int Height() = 0;
};