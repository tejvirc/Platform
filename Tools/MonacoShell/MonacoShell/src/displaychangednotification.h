#pragma once

#include <shellapi.h>
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <wchar.h>
#include <math.h>
#include <sstream>
#include <map>
#include <atomic>
#include <dbt.h>
#include <functional>
#include "logger.h"

#ifndef HINST_THISCOMPONENT
EXTERN_C IMAGE_DOS_HEADER __ImageBase;
#define HINST_THISCOMPONENT ((HINSTANCE)&__ImageBase)
#endif

class DisplayChangeNotification
{
private:
    HWND         _hwnd;
    UINT         _windowWidth;
    UINT         _windowHeight;
    std::string  _windowName;

    std::function<void()> _on_display_change;

public:
    DisplayChangeNotification() :
        _hwnd(NULL)
    {
        Log(LogTarget::File, "DisplayChangeNotification(): Created.\n");

        Create("DeviceNotification", 0, 0, 1, 1);
    }

    ~DisplayChangeNotification()
    {
        Log(LogTarget::File, "DisplayChangeNotification(): Destroyed.\n");

        DestroyWindow(_hwnd);
        ::SetPropA(_hwnd, "DisplayChangeNotification", (HANDLE)nullptr);

        _hwnd = NULL;
    }

private:
    const char* Name()
    {
        return _windowName.c_str();
    }

    HWND WindowHandle()
    {
        return _hwnd;
    }

    //
    // Register the window class and call methods for instantiating drawing resources
    //

    HRESULT Create(const char* szTitleName, int x, int y, int clientWidth, int clientHeight)
    {
        HRESULT hr = E_FAIL;

        Log(LogTarget::File, "DisplayChangeNotification():Create(%s, %d, %d, %d, %d)\n", szTitleName, x, y, clientWidth, clientHeight);

        _windowName = szTitleName;
        _windowWidth = clientWidth;
        _windowHeight = clientHeight;

        // Register the window class.
        WNDCLASSEXA wcex = { sizeof(WNDCLASSEXA) };
        wcex.style = CS_HREDRAW | CS_VREDRAW;
        wcex.lpfnWndProc = DisplayChangeNotification::WndProc;
        wcex.cbClsExtra = 0;
        wcex.cbWndExtra = sizeof(LONG_PTR);
        wcex.hInstance = HINST_THISCOMPONENT;
        wcex.hbrBackground = NULL;
        wcex.lpszMenuName = NULL;
        wcex.hCursor = LoadCursor(NULL, IDI_APPLICATION);
        wcex.lpszClassName = "DeviceNotification";

        RegisterClassExA(&wcex);

        DWORD wndStyle = WS_POPUP;/*| WS_MAXIMIZE*/;

        //
        // Create the window.
        //

        _hwnd = CreateWindowExA(
            (WS_EX_NOACTIVATE | WS_EX_COMPOSITED),
            "DeviceNotification",
            _windowName.c_str(),
            wndStyle,
            x,
            y,
            clientWidth,
            clientHeight,
            NULL,
            NULL,
            HINST_THISCOMPONENT,
            this
        );

        hr = _hwnd ? S_OK : E_FAIL;
        if (SUCCEEDED(hr))
        {
            SetWindowPos(_hwnd, HWND_BOTTOM, x, y, clientWidth, clientHeight,
                SWP_NOACTIVATE | SWP_FRAMECHANGED /*| SWP_SHOWWINDOW*/); // remain hidden
            UpdateWindow(_hwnd);
        }

        return hr;
    }

public:
    //
    // Process and dispatch messages
    //

    static void RunMessageLoopOnce()
    {
        MSG msg;
        bool w_quit = false;

        while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE))
        {
            // Translate the message and dispatch it to WindowProc()
            if (msg.message != WM_QUIT)
            {
                TranslateMessage(&msg);
                DispatchMessage(&msg);
            }
        }
    }

    void OnDisplayChanged(std::function<void()> on_display_change)
    {
        Log(LogTarget::File, "DisplayChangeNotification():OnDisplayChanged(std::function<void()> on_display_change)\n");

        _on_display_change = on_display_change;
    }

    virtual void OnDisplayChanged()
    {
        Log(LogTarget::File, "DisplayChangeNotification():OnDisplayChanged()\n");

        if (_on_display_change)
            _on_display_change();
    }

private:
    //
    // The windows proc.
    //

    static LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
    {
        DisplayChangeNotification* pSw = static_cast<DisplayChangeNotification*>(::GetPropA(hWnd, "DisplayChangeNotification"));

        if (message == WM_CREATE)
        {
            LPCREATESTRUCT pcs = (LPCREATESTRUCT)lParam;

            pSw = (DisplayChangeNotification*)pcs->lpCreateParams;
            ::SetPropA(hWnd, "DisplayChangeNotification", (HANDLE)pSw);
        }
        else
        {
            if (pSw != NULL)
            {
                switch (message)
                {
                //case WM_DEVICECHANGE:
                //{
                //    Log(LogTarget::File, "DisplayChangeNotification():WndProc(): message = WM_DEVICECHANGE (0x%X)\n", message);

                //    switch (wParam)
                //    {
                //    case DBT_CONFIGCHANGECANCELED:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_CONFIGCHANGECANCELED (0x%X)\n", wParam);
                //        } break;
                //    case DBT_CONFIGCHANGED:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_CONFIGCHANGED (0x%X)\n", wParam);
                //        } break;
                //    case DBT_CUSTOMEVENT:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_CUSTOMEVENT (0x%X)\n", wParam);
                //        } break;
                //    case DBT_DEVICEARRIVAL:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_DEVICEARRIVAL (0x%X)\n", wParam);
                //            DEV_BROADCAST_HDR* pDBH = (DEV_BROADCAST_HDR*)lParam;

                //            //
                //            // This event happenes on 1st time boots when windows is installing drivers for devices, but not on
                //            // the 2nd, or subsequent boots.
                //            //

                //            switch (pDBH->dbch_devicetype)
                //            {
                //            case DBT_DEVTYP_DEVICEINTERFACE:
                //                {
                //                    Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_DEVICEINTERFACE (%d)\n", pDBH->dbch_devicetype);
                //                    DEV_BROADCAST_DEVICEINTERFACE* pDBDI = (DEV_BROADCAST_DEVICEINTERFACE*)lParam;
                //                    Log(LogTarget::File, "                    Device Name = %s\n", pDBDI->dbcc_name);
                //                } break;
                //            case DBT_DEVTYP_HANDLE:
                //                {
                //                    Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_HANDLE (%d)\n", pDBH->dbch_devicetype);
                //                } break;
                //            case DBT_DEVTYP_OEM:
                //                {
                //                    Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_OEM (%d)\n", pDBH->dbch_devicetype);
                //                } break;
                //            case DBT_DEVTYP_PORT:
                //                {
                //                    Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_PORT (%d)\n", pDBH->dbch_devicetype);

                //                    //
                //                    // This event will happen if Windows detects new COM ports that it is not expecting
                //                    //

                //                    DEV_BROADCAST_PORT* pDBP = (DEV_BROADCAST_PORT*)lParam;
                //                    Log(LogTarget::File, "                      Port Name = %s\n", pDBP->dbcp_name);
                //                } break;
                //            case DBT_DEVTYP_VOLUME:
                //                {
                //                    Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_VOLUME (%d)\n", pDBH->dbch_devicetype);
                //                } break;
                //            default:
                //                {
                //                    Log(LogTarget::File, "                    Device Type = UNEXPECTED (%d)\n", pDBH->dbch_devicetype);
                //                } break;
                //            }
                //        } break;
                //    case DBT_DEVICEQUERYREMOVE:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_DEVICEQUERYREMOVE (0x%X)\n", wParam);
                //        } break;
                //    case DBT_DEVICEQUERYREMOVEFAILED:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_DEVICEQUERYREMOVEFAILED (0x%X)\n", wParam);
                //        } break;
                //    case DBT_DEVICEREMOVECOMPLETE:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_DEVICEREMOVECOMPLETE (0x%X)\n", wParam);
                //        } break;
                //    case DBT_DEVICEREMOVEPENDING:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_DEVICEREMOVEPENDING (0x%X)\n", wParam);
                //        } break;
                //    case DBT_DEVICETYPESPECIFIC:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_DEVICETYPESPECIFIC (0x%X)\n", wParam);
                //        } break;
                //    case DBT_DEVNODES_CHANGED:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_DEVNODES_CHANGED (0x%X)\n", wParam);

                //            //
                //            // This event happenes when new devices are installed, the the VBD or a new display.
                //            //

                //        } break;
                //    case DBT_QUERYCHANGECONFIG:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_QUERYCHANGECONFIG (0x%X)\n", wParam);
                //        } break;
                //    case DBT_USERDEFINED:
                //        {
                //            Log(LogTarget::File, "                         wParam = DBT_USERDEFINED (0x%X)\n", wParam);
                //        } break;
                //    default:
                //        {
                //            Log(LogTarget::File, "                         wParam = UNEXPECTED (0x%X)\n", wParam);
                //        } break;
                //    }
                //} break;
                case WM_DISPLAYCHANGE:
                    {
                        Log(LogTarget::File, "DisplayChangeNotification():WndProc(): message = WM_DISPLAYCHANGE (0x%X) - [hWnd %p] - [Resolution %dx%d]\n", message, hWnd, LOWORD(lParam), HIWORD(lParam));

                        pSw->OnDisplayChanged();
                    } break;
                case WM_DESTROY:
                    {
                        Log(LogTarget::File, "DisplayChangeNotification():WndProc(): message = WM_DESTROY (0x%X)\n", message);

                        PostQuitMessage(0);
                        ::Sleep(10); // Let the W_QUIT get pushed before exit this proc
                    } break;
                default:
                    {
                        // Log(LogTarget::File, "DisplayChangeNotification():WndProc(): message = UNKNOWN (0x%X)\n", message);
                    } break;
                }
            }
        }

        return DefWindowProc(hWnd, message, wParam, lParam);;
    }
};