#pragma once

//
// Windows Header Files
//

#include <VersionHelpers.h>
#include <platform.h>
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <wchar.h>
#include <math.h>
#include <sstream>
#include <map>
#include <atomic>
#include <dbt.h>
#include "MonitorInfo.h"
#include "logger.h"

using namespace Gdiplus;

#ifndef HINST_THISCOMPONENT
EXTERN_C IMAGE_DOS_HEADER __ImageBase;
#define HINST_THISCOMPONENT ((HINSTANCE)&__ImageBase)
#endif

enum MouseButton
{
    Left = 1,
    Right = 2,
    Middle = 3,
    None = 0
};

enum KeyboardModifiers
{
    Control = 1<<1,
    LControl = 1<<2,
    RControl = 1<<3,
    Alt = 1<<4,
    AltGr = 1<<5 | Alt,
    Shift = 1<<6,
    LShift = 1<<7 | Shift,
    RShift = 1<<8 | Shift,
    CapsLock = 1<<9 | Shift
};

struct WindowInfo
{
    LONG style;
    LONG ex_style;
    bool maximized;
    RECT window_rect;
};

class ShellWindow
{
private:
    HWND         _hwnd;
    WindowInfo   _saved_window_info;
    UINT         _windowWidth;
    UINT         _windowHeight;
    std::string  _windowName;
    bool         _fullScreen;
    bool         _treat_touch_as_mouse;
    DWORD        _activeTouchId;
    bool         _bNoActivation         = true;
    int          _monitorIndex          = -1;
    CabinetInfo* _pCabinetInfo;

    //
    // Graphics
    //

    Gdiplus::Bitmap* _pMemBitmap = nullptr;

    //
    // Events
    //

    std::function<void(int vkey)> _on_key;
    std::function<void(UINT cInput, TOUCHINPUT*)> _on_touch;
    std::function<void(int x, int y, MouseButton button)> _on_mousedown;
    std::function<void(int x, int y, MouseButton button, bool pressed)> _on_mouseclick;
    std::function<void(int x, int y, MouseButton button, bool pressed)> _on_mousemove;
    std::function<void(int x, int y, int scroll)> _on_mousescroll;
    std::function<void(int x, int y)> _on_resize;
    std::function<void(Graphics& g)> _on_paint;
    std::function<void(DWORD, const char* data, size_t length)> _on_data;
    std::function<void()> _on_display_change;
    std::function<void(DWORD id)> _on_timer;

    static DWORD constexpr DisplayManagerTimer() { return 10001; };
    static DWORD constexpr DisplayManagerTimerIntervalMS() { return 500; };

    std::atomic<int>* _gdi_ref_count = 0;
    ULONG_PTR         _gdiplusToken = 0;

public:
    ShellWindow(CabinetInfo* pCabinetInfoObj) :
        _hwnd(NULL),
        _windowWidth(0),
        _windowHeight(0),
        _fullScreen(false),
        _treat_touch_as_mouse(true),
        _activeTouchId(0),
        _pCabinetInfo(pCabinetInfoObj)
    {
        static Gdiplus::GdiplusStartupInput gdiplusStartupInput = 0;  // cpp headeronly hack
        static ULONG_PTR                    gdiplusToken = 0;         // cpp headeronly hack
        static std::atomic<int>             gdi_ref_count = 0;        // cpp headeronly hack

        if (gdi_ref_count.fetch_add(1,std::memory_order_relaxed) == 0)
        {
            //
            // Initialize GDI+.
            //

            GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);
        }

        _gdi_ref_count = &gdi_ref_count;    // cpp headeronly hack
        _gdiplusToken = gdiplusToken;       // cpp headeronly hack
    }

    ~ShellWindow()
    {
        if (_pMemBitmap != nullptr) 
            delete _pMemBitmap;

        _pMemBitmap = nullptr;
        DestroyWindow(_hwnd);
        ::SetProp(_hwnd, "ShellWindow", (HANDLE)nullptr);

        if (_gdi_ref_count->fetch_sub(1, std::memory_order_relaxed) == 1)
        {
            GdiplusShutdown(_gdiplusToken);
            _gdiplusToken = 0;
        }

        _hwnd = NULL;
        _pCabinetInfo = nullptr;
    }

    inline std::string WideCharacter2MultiByteString(const wchar_t* value)
    {
        size_t len = wcslen(value) + 1; // include nullterm
        char* buf = new char[len];
        std::mbstate_t state = std::mbstate_t();
        std::wcsrtombs(buf, &value, len, &state);
        std::string ret = buf;
        delete[] buf;

        return ret;
    }

    const char* Name()
    {
        return _windowName.c_str();
    }

    HWND WindowHandle()
    {
        return _hwnd;
    }

    void DisableTouchFeedback()
    {
        //
        // For slot games, we do not want the generic windows touch feedback graphics to show.
        //

        //
        // Min support for these APIs.
        //

        if (IsWindows8OrGreater())
        {
            typedef void (WINAPI *PSWFS)(HWND, FEEDBACK_TYPE, DWORD, UINT32, const VOID*);

            auto pSetWindowFeedbackSetting = (PSWFS)GetProcAddress(GetModuleHandle(TEXT("user32.dll")), "SetWindowFeedbackSetting");
            if (pSetWindowFeedbackSetting != nullptr)
            {
                BOOL enabled = false;

                pSetWindowFeedbackSetting(_hwnd, FEEDBACK_TOUCH_CONTACTVISUALIZATION, 0, sizeof(BOOL), &enabled);
                pSetWindowFeedbackSetting(_hwnd, FEEDBACK_TOUCH_TAP, 0, sizeof(BOOL), &enabled);
                pSetWindowFeedbackSetting(_hwnd, FEEDBACK_TOUCH_DOUBLETAP, 0, sizeof(BOOL), &enabled);
                pSetWindowFeedbackSetting(_hwnd, FEEDBACK_TOUCH_PRESSANDHOLD, 0, sizeof(BOOL), &enabled);
                pSetWindowFeedbackSetting(_hwnd, FEEDBACK_TOUCH_RIGHTTAP, 0, sizeof(BOOL), &enabled);
                pSetWindowFeedbackSetting(_hwnd, FEEDBACK_GESTURE_PRESSANDTAP, 0, sizeof(BOOL), &enabled);
                pSetWindowFeedbackSetting(_hwnd, FEEDBACK_PEN_PRESSANDHOLD, 0, sizeof(BOOL), &enabled);
            }
        }
    }

    int Width() const
    {
        return _windowWidth;
    }

    int Height() const
    {
        return _windowHeight;
    }

    void Invalidate(RECT* rect = NULL, bool bErase = false)
    {
        InvalidateRect(WindowHandle(), rect, false);
    }

    bool IsFullscreen() { return _fullScreen; }

    //
    // Register the window class and call methods for instantiating drawing resources
    //

    HRESULT CreateMonitorIndex(const char* szTitleName, int MonitorIndex)
    {
        MonitorInfo info(_pCabinetInfo);
        auto di = info.GetDisplayInfoByIndex(MonitorIndex);

        if (di == nullptr)
            return E_FAIL;

        _monitorIndex = MonitorIndex;
        auto width = di->devMode.dmPelsWidth, height = di->devMode.dmPelsHeight;

        Log(LogTarget::File, "ShellWindow():CreateMonitorIndex(): _monitorIndex: '%d'.\n", _monitorIndex);

        if (di->devMode.dmDisplayOrientation == DMDO_90 || di->devMode.dmDisplayOrientation == DMDO_270)
        {
            Log(LogTarget::File, "ShellWindow():CreateMonitorIndex(): Display Orientation - '%d', swapping width & height.\n", di->devMode.dmDisplayOrientation);

            std::swap(width, height);
        }

        Log(LogTarget::File, "ShellWindow():CreateMonitorIndex(): width x height: '%dx%d'.\n", width, height);

        return Create(szTitleName,di->devMode.dmPosition.x,di->devMode.dmPosition.y, width, height);
    }

    HRESULT CreatePrimary(const char* szTitleName)
    {
        MonitorInfo info(_pCabinetInfo);
        auto di = info.GetPrimaryDisplay();

        if (di == nullptr)
            return E_FAIL;

        _monitorIndex = di->monitorIndex;

        Log(LogTarget::File, "ShellWindow():CreatePrimary(): _monitorIndex: '%d'.\n", _monitorIndex);

        return Create(szTitleName,di->devMode.dmPosition.x,di->devMode.dmPosition.y,di->devMode.dmPelsWidth,di->devMode.dmPelsHeight);
    }

    //
    // Register the window class and call methods for instantiating drawing resources
    //

    HRESULT Create(const char* szTitleName, int x, int y, int clientWidth, int clientHeight)
    {
        _windowName = szTitleName;
        _windowWidth = clientWidth;
        _windowHeight = clientHeight;

        Log(LogTarget::File, "ShellWindow():Create(): _windowName: %s, _windowWidth x _windowHeight: '%dx%d'.\n", _windowName.c_str(), _windowWidth, _windowHeight);

        HeapSetInformation(NULL, HeapEnableTerminationOnCorruption, NULL, 0);

        if (!SUCCEEDED(CoInitialize(NULL)))
            return E_FAIL;

        //
        // Register the window class.
        //

        WNDCLASSEX wcex = { sizeof(WNDCLASSEX) };
        wcex.style = CS_HREDRAW | CS_VREDRAW;
        wcex.lpfnWndProc = ShellWindow::WndProc;
        wcex.cbClsExtra = 0;
        wcex.cbWndExtra = sizeof(LONG_PTR);
        wcex.hInstance = HINST_THISCOMPONENT;
        wcex.hbrBackground = NULL;
        wcex.lpszMenuName = NULL;
        wcex.hCursor = LoadCursor(NULL, IDI_APPLICATION);
        wcex.lpszClassName = "ShellWindow";

        RegisterClassEx(&wcex);

        DWORD wndStyle = WS_POPUP;/* | WS_MAXIMIZE*/;

        //
        // Create the window.
        //

        _hwnd = CreateWindowEx(
            ((_bNoActivation ? WS_EX_NOACTIVATE : 0) | WS_EX_COMPOSITED),
            "ShellWindow",
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

        HRESULT hr = _hwnd ? S_OK : E_FAIL;
        if (SUCCEEDED(hr))
        {
            Log(LogTarget::File, "ShellWindow():Create(): SetWindowPos(%p, HWND_BOTTOM, %d, %d, %d, %d, SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW)\n", _hwnd, x, y, clientWidth, clientHeight);

            SetWindowPos(_hwnd, HWND_BOTTOM, x, y, clientWidth, clientHeight, 
                SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
            UpdateWindow(_hwnd);

            if (_monitorIndex == -1) // Resolve which monitor index this window belongs to
            {
                MonitorInfo info(_pCabinetInfo);
                auto di = info.GetDisplayInfoFromHWND(_hwnd);

                if (di)
                {
                    _monitorIndex = di->monitorIndex;

                    Log(LogTarget::File, "ShellWindow():Create(): _monitorIndex: '%d'.\n", _monitorIndex);
                }
            }

            int value = GetSystemMetrics(SM_DIGITIZER);
            if (value & NID_READY)
            {
                //
                // stack ready
                //
            }

            if (value  & NID_MULTI_INPUT)
            {
                RegisterTouchWindow(_hwnd,0);
            }

            DisableTouchFeedback();
        }

        return hr;
    }

    //
    // Process and dispatch messages
    //

    static bool RunMessageLoopOnce()
    {
        MSG msg;
        bool w_quit = false;

        while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE))
        {
            //
            // Translate the message and dispatch it to WindowProc()
            //

            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }

        return !w_quit;
    }

    void Resize(UINT width, UINT height)
    {
        //
        // TODO: resize? not needed..
        //
    }

public:
    //
    // callback setters
    //

    void OnResize(std::function<void(int w, int h)> onresize) 
    {
        _on_resize = onresize;
    }

    void OnKey(std::function<void(int vkey)> onkey)
    {
        _on_key = onkey;
    }

    void OnMouseClick(std::function<void(int x, int y, MouseButton button, bool pressed)> onMouseClick) 
    {
        _on_mouseclick = onMouseClick;
    }

    void OnMouseDown(std::function<void(int x, int y, MouseButton button)> onMouseDown)
    {
        _on_mousedown = onMouseDown;
    }

    void OnMouseMove(std::function<void(int x, int y, MouseButton button, bool pressed)> onMouseMove) 
    {
        _on_mousemove = onMouseMove;
    }

    void OnTouch(std::function<void(UINT cInput, TOUCHINPUT*)> onTouch) 
    {
        _on_touch = onTouch;
    }

    void OnTouch(std::function<void(int x, int y, MouseButton button, bool pressed)> onMouseMove) 
    {
        _on_mousemove = onMouseMove;
    }

    void OnData(std::function<void(DWORD, const char* data, size_t length)> on_data)
    {
        _on_data = on_data;
    }

    void OnDisplayChanged(std::function<void()> on_display_change)
    {
        Log(LogTarget::File, "ShellWindow():OnDisplayChanged(std::function<void()> on_display_change)\n");

        _on_display_change = on_display_change;
    }

    void OnTimer(std::function<void(DWORD)> on_timer)
    {
        _on_timer = on_timer;
    }

    void OnPaint(std::function<void(Graphics& g)> onpaint) 
    {
        _on_paint = onpaint;
        InvalidateRect(WindowHandle(), NULL, FALSE); // ensure a OnPaint is invoked when new callback is set
    }

    //
    // virtual methods (events)
    //

    virtual void OnKeyPressed(int /*ascii*/, int vkey, int /*modifiers*/)
    {
        if (_on_key) _on_key(vkey);
    }

    virtual void OnTouch(UINT cInput, TOUCHINPUT* ti)
    {
        if (_on_touch) _on_touch(cInput,ti);
    }

    virtual void UpdateDisplayToMonitorSize()
    {
        //
        // Will maximize window according to its placement monitor
        //

        Log(LogTarget::File, "ShellWindow():UpdateDisplayToMonitorSize()\n");

        RECT window_rect;
        bool is_resolved = false;

        if (_monitorIndex != -1)
        {
            MonitorInfo info(_pCabinetInfo);
            MonitorInfo::DisplayInfo* di = info.GetDisplayInfoByIndex(_monitorIndex);

            Log(LogTarget::File, "ShellWindow():UpdateDisplayToMonitorSize(): _monitorIndex: '%d'.\n", _monitorIndex);

            if (di)
            {
                int width = di->devMode.dmPelsWidth, height = di->devMode.dmPelsHeight;

                if (di->devMode.dmDisplayOrientation == DMDO_90 || di->devMode.dmDisplayOrientation == DMDO_270)
                {
                    std::swap(width, height);
                }

                window_rect.left = di->devMode.dmPosition.x;
                window_rect.top = di->devMode.dmPosition.y;
                window_rect.right = di->devMode.dmPosition.x + width;
                window_rect.bottom = di->devMode.dmPosition.y + height;
                is_resolved = true;
            }
        }

        if (!is_resolved) // fallback to currently placed monitor
        {
            ShowWindow(_hwnd, SW_HIDE);
        }
        else
        {
            Log(LogTarget::File, "ShellWindow():UpdateDisplayToMonitorSize(): SetWindowPos(%p, HWND_BOTTOM, %d, %d, %d, %d, SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW)\n", _hwnd, window_rect.left, window_rect.top, window_rect.right - window_rect.left, window_rect.bottom - window_rect.top);

            SetWindowPos(_hwnd, HWND_BOTTOM, window_rect.left, window_rect.top,
                window_rect.right - window_rect.left, window_rect.bottom - window_rect.top,
                SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
        }

        UpdateWindow(_hwnd);
        InvalidateRect(_hwnd, NULL, FALSE);

        Log(LogTarget::File, "ShellWindow():UpdateDisplayToMonitorSize(): Complete.\n");
    }

    virtual void OnPaint(PAINTSTRUCT* ps)
    {
        Graphics graphics(ps->hdc);

        if (_pMemBitmap == nullptr)
        {
            _pMemBitmap  = new Bitmap(Width(), Height());
        }

        if (_pMemBitmap != nullptr)
        {
            Graphics* pMemGraphics = nullptr;
            pMemGraphics = Graphics::FromImage(_pMemBitmap);
            pMemGraphics->Clear(Color::Black);

            if (_on_paint)
               _on_paint(*pMemGraphics); // render to back buffer

            delete pMemGraphics;
            graphics.DrawImage(_pMemBitmap, 0, 0); // render to front buffer
        }
    }

    virtual void OnMouseClick(int x, int y, MouseButton button, bool pressed)
    {
        if (_on_mouseclick)
            _on_mouseclick(x,y,button,pressed);
    }

    virtual void OnMouseDown(int x, int y, MouseButton button)
    {
        if (_on_mousedown)
            _on_mousedown(x, y, button);
    }

    virtual void OnMouseMove(int x, int y, MouseButton button, bool pressed)
    {
        if (_on_mousemove)
            _on_mousemove(x, y, button, pressed);
    }

    virtual void OnMouseScroll(int x,int y,int scroll)
    {
        if (_on_mousescroll)
            _on_mousescroll(x, y, scroll);
    }

    virtual void OnData(DWORD id, const char* data, size_t length)
    {
        if (_on_data)
            _on_data(id,data,length);
    }

    void UpdateDisplaySettings()
    {
        SetTimer(_hwnd, ShellWindow::DisplayManagerTimer(), ShellWindow::DisplayManagerTimerIntervalMS(), NULL);
    }

    virtual void OnDisplayChanged()
    {
        Log(LogTarget::File, "ShellWindow():OnDisplayChanged()\n");

        if (_on_display_change)
            _on_display_change();
    }

    void OnTimer(DWORD dwTimerId)
    {
        if (_on_timer) 
            _on_timer(dwTimerId);
    }

    //
    // Resize the render target.
    //

    virtual void OnResize(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;

        Log(LogTarget::File, "ShellWindow():OnResize(): _windowWidth x _windowHeight: '%dx%d'.\n", _windowWidth, _windowHeight);

        if (_pMemBitmap != nullptr)  // Invalidate memory bitmap (re-created at next paint)
        {
            delete _pMemBitmap;
            _pMemBitmap = nullptr;
        }

        if (_on_resize)
            _on_resize(width, height);

        InvalidateRect(WindowHandle(),NULL,false);
    }

    void Hide() 
    {
        ::ShowWindow(WindowHandle(), SW_HIDE);
    }

    void Show() 
    {
        ::ShowWindow(WindowHandle(), SW_SHOW);
    }

    int Width()
    {
        return _windowWidth;
    }

    int Height()
    {
        return _windowHeight;
    }
 
 private:
    void OnTouchInternal(UINT cInput, TOUCHINPUT* pInputs)
    {
        if (_treat_touch_as_mouse)
        {
            POINT p = {pInputs[0].x / 100,pInputs[0].y / 100};

            ScreenToClient(_hwnd,&p);

            if ((pInputs[0].dwFlags & TOUCHEVENTF_DOWN) == TOUCHEVENTF_DOWN)
            {
                _activeTouchId = pInputs[0].dwID;
                OnMouseClick(p.x, p.y, MouseButton::Left, true);
            }

            if ((pInputs[0].dwFlags & TOUCHEVENTF_UP) == TOUCHEVENTF_UP && pInputs[0].dwID == _activeTouchId)
            {
                _activeTouchId = 0;
                OnMouseClick(p.x, p.y, MouseButton::Left, false);
            }
        }
        else
        {
            OnTouch(cInput, pInputs);
        }
    }

    static bool IsTouchEvent()
    {
        auto extraInfo = GetMessageExtraInfo();

        return (extraInfo & 0x80) == 0x80;
    }

    static bool IsMultiTouchSupported()
    {
        int value = GetSystemMetrics(SM_DIGITIZER);

        return (value & NID_MULTI_INPUT) ? true : false;
    }

    //
    // The windows proc.
    //

    static LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
    {
        ShellWindow *pSw = static_cast<ShellWindow *>(::GetProp(hWnd, "ShellWindow"));

        if (message == WM_CREATE)
        {
            Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_CREATE (0x%X)\n", message);

            LPCREATESTRUCT pcs = (LPCREATESTRUCT)lParam;
            pSw = (ShellWindow*)pcs->lpCreateParams;
            ::SetProp(hWnd, "ShellWindow", (HANDLE)pSw);
        }
        else
        {
            if (pSw != NULL)
            {
                switch (message)
                {
                case WM_TIMER:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_TIMER (0x%X)\n", message);

                        DWORD timerId = (DWORD) wParam;
                        if (timerId == ShellWindow::DisplayManagerTimer())
                        {
                            KillTimer(hWnd,timerId);
                            pSw->UpdateDisplayToMonitorSize();
                        }
                        else
                        {
                            pSw->OnTimer(timerId);
                        }
                    } break;
                case WM_MOVE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_MOVE (0x%X)\n", message);
                    } break;
                case WM_ACTIVATE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_ACTIVATE (0x%X)\n", message);
                    } break;
                case WM_SETFOCUS:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_SETFOCUS (0x%X)\n", message);
                    } break;
                case WM_NCPAINT:
                    {
                        //Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_NCPAINT (0x%X)\n", message);
                    } break;
                case WM_NCACTIVATE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_NCACTIVATE (0x%X)\n", message);
                    } break;

                case WM_NCDESTROY:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_NCDESTROY (0x%X)\n", message);
                    } break;
                case WM_NCHITTEST:
                    {
                        //Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_NCHITTEST (0x%X)\n", message);
                    } break;
                case WM_SETCURSOR:
                    {
                        //Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_SETCURSOR (0x%X)\n", message);
                    } break;
                case WM_SYNCPAINT:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_SYNCPAINT (0x%X)\n", message);
                    } break;
                case WM_MOUSEACTIVATE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_MOUSEACTIVATE (0x%X)\n", message);
                    } break;
                case WM_ACTIVATEAPP:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_ACTIVATEAPP (0x%X)\n", message);
                    } break;
                case WM_IME_SETCONTEXT:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_IME_SETCONTEXT (0x%X)\n", message);
                    } break;
                case WM_IME_NOTIFY:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_IME_NOTIFY (0x%X)\n", message);
                    } break;
                case WM_GETOBJECT:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_GETOBJECT (0x%X)\n", message);
                    } break;
                case WM_POINTERACTIVATE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_POINTERACTIVATE (0x%X)\n", message);
                    } break;
                case WM_POINTERUPDATE:
                    {
                        // Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_POINTERUPDATE (0x%X)\n", message);
                    } break;
                case WM_POINTERDOWN:
                    {
                        // Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_POINTERDOWN (0x%X)\n", message);
                    } break;
                case WM_WININICHANGE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_WININICHANGE (0x%X)\n", message);
                    } break;
                case WM_DEVICECHANGE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_DEVICECHANGE (0x%X)\n", message);

                        switch (wParam)
                        {
                        case DBT_CONFIGCHANGECANCELED:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_CONFIGCHANGECANCELED (0x%X)\n", wParam);
                            } break;
                        case DBT_CONFIGCHANGED:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_CONFIGCHANGED (0x%X)\n", wParam);
                            } break;
                        case DBT_CUSTOMEVENT:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_CUSTOMEVENT (0x%X)\n", wParam);
                            } break;
                        case DBT_DEVICEARRIVAL:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_DEVICEARRIVAL (0x%X)\n", wParam);

                                DEV_BROADCAST_HDR* pDBH = (DEV_BROADCAST_HDR*)lParam;

                                //
                                // This event happenes on 1st time boots when windows is installing drivers for devices, but not on
                                // the 2nd, or subsequent boots.
                                //

                                switch (pDBH->dbch_devicetype)
                                {
                                case DBT_DEVTYP_DEVICEINTERFACE:
                                    {
                                        Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_DEVICEINTERFACE (%d)\n", pDBH->dbch_devicetype);

                                        DEV_BROADCAST_DEVICEINTERFACE* pDBDI = (DEV_BROADCAST_DEVICEINTERFACE*)lParam;

                                        Log(LogTarget::File, "                    Device Name = %s\n", pDBDI->dbcc_name);
                                    } break;
                                case DBT_DEVTYP_HANDLE:
                                    {
                                        Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_HANDLE (%d)\n", pDBH->dbch_devicetype);
                                    } break;
                                case DBT_DEVTYP_OEM:
                                    {
                                        Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_OEM (%d)\n", pDBH->dbch_devicetype);
                                    } break;
                                case DBT_DEVTYP_PORT:
                                    {
                                        Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_PORT (%d)\n", pDBH->dbch_devicetype);

                                        //
                                        // This event will happen if Windows detects new COM ports that it is not expecting
                                        //

                                        DEV_BROADCAST_PORT* pDBP = (DEV_BROADCAST_PORT*)lParam;

                                        Log(LogTarget::File, "                      Port Name = %s\n", pDBP->dbcp_name);
                                    } break;
                                case DBT_DEVTYP_VOLUME:
                                    {
                                        Log(LogTarget::File, "                    Device Type = DBT_DEVTYP_VOLUME (%d)\n", pDBH->dbch_devicetype);
                                    } break;
                                default:
                                    {
                                        Log(LogTarget::File, "                    Device Type = UNEXPECTED (%d)\n", pDBH->dbch_devicetype);
                                    } break;
                                }
                            } break;
                        case DBT_DEVICEQUERYREMOVE:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_DEVICEQUERYREMOVE (0x%X)\n", wParam);
                            } break;
                        case DBT_DEVICEQUERYREMOVEFAILED:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_DEVICEQUERYREMOVEFAILED (0x%X)\n", wParam);
                            } break;
                        case DBT_DEVICEREMOVECOMPLETE:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_DEVICEREMOVECOMPLETE (0x%X)\n", wParam);
                            } break;
                        case DBT_DEVICEREMOVEPENDING:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_DEVICEREMOVEPENDING (0x%X)\n", wParam);
                            } break;
                        case DBT_DEVICETYPESPECIFIC:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_DEVICETYPESPECIFIC (0x%X)\n", wParam);
                            } break;
                        case DBT_DEVNODES_CHANGED:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_DEVNODES_CHANGED (0x%X)\n", wParam);

                                //
                                // This event happenes when new devices are installed, the the VBD or a new display.
                                //
                            } break;
                        case DBT_QUERYCHANGECONFIG:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_QUERYCHANGECONFIG (0x%X)\n", wParam);
                            } break;
                        case DBT_USERDEFINED:
                            {
                                Log(LogTarget::File, "                         wParam = DBT_USERDEFINED (0x%X)\n", wParam);
                            } break;
                        default:
                            {
                                Log(LogTarget::File, "                         wParam = UNEXPECTED (0x%X)\n", wParam);
                            } break;
                        }
                    } break;
                case WM_COPYDATA:
                    {
                        //Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_COPYDATA (0x%X)\n", message);

                        PCOPYDATASTRUCT pCDS = (PCOPYDATASTRUCT) lParam;
                        pSw->OnData((DWORD)pCDS->dwData, (const char*) pCDS->lpData, pCDS->cbData);
                    } break;
                case WM_SIZE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_SIZE (0x%X)\n", message);

                        UINT width = LOWORD(lParam);
                        UINT height = HIWORD(lParam);
                        pSw->OnResize(width, height);
                    } break;
                case WM_WINDOWPOSCHANGED:
                    {
                        PWINDOWPOS pWinPos = (PWINDOWPOS)lParam;

                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_WINDOWPOSCHANGED (0x%X) - [hWnd %p] - XxY: %dx%d - WidthxHeight: %dx%d\n", message, pWinPos->hwnd, pWinPos->x, pWinPos->y, pWinPos->cx, pWinPos->cy);
                    } break;
                case WM_DISPLAYCHANGE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_DISPLAYCHANGE (0x%X) - [hWnd %p] - [Resolution %dx%d]\n", message, hWnd, LOWORD(lParam), HIWORD(lParam));

                        pSw->OnDisplayChanged();
                    } break;
                case WM_PAINT:
                    {
                        //Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_PAINT (0x%X)\n", message);

                        PAINTSTRUCT ps;
                        BeginPaint(hWnd, &ps);
                        pSw->OnPaint(&ps);
                        EndPaint(hWnd, &ps);
                        ValidateRect(hWnd, NULL);
                    } break;
                case WM_WINDOWPOSCHANGING:
                    {
                        PWINDOWPOS pWinPos = (PWINDOWPOS)lParam;

                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_WINDOWPOSCHANGING (0x%X) - [hWnd %p] - XxY: %dx%d - WidthxHeight: %dx%d\n", message, pWinPos->hwnd, pWinPos->x, pWinPos->y, pWinPos->cx, pWinPos->cy);

                        //
                        // Force Any position change to index 0,0
                        //

                        //LPWINDOWPOS(lParam)->x = 0;
                        //LPWINDOWPOS(lParam)->y = 0;
                        InvalidateRect(hWnd, NULL, FALSE);
                    } break;
                case WM_ERASEBKGND:
                    {
                        //Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_ERASEBKGND (0x%X)\n", message);

                        return 1;
                    } break;
                case WM_KEYDOWN:
                    //if (lParam == 1) // ignore repeated keys
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_KEYDOWN (0x%X)\n", message);

                        WORD ch = 0;
                        BYTE kbs[256];
                        if (::GetKeyboardState(kbs))
                        {
                            ::MapVirtualKey((UINT)wParam, MAPVK_VSC_TO_VK);
                            if (kbs[VK_CONTROL] & 0x80)
                            {
                                kbs[VK_CONTROL] &= 0x7f;
                                if (::ToAscii((UINT)wParam, ::MapVirtualKey((UINT)wParam, MAPVK_VSC_TO_VK), kbs, &ch, 0) == 0)
                                {
                                    ch = 0;
                                }
                                kbs[VK_CONTROL] |= 0x80;
                            }
                            else if (::ToAscii((UINT)wParam, ::MapVirtualKey((UINT)wParam, MAPVK_VSC_TO_VK), kbs, &ch, 0) == 0)
                            {
                                ch = 0;
                            }

                            int modifiers = 0;

                            if (kbs[VK_LSHIFT] & 0x80)
                            {
                                modifiers |= KeyboardModifiers::LShift;
                            }

                            if (kbs[VK_RSHIFT] & 0x80)
                            {
                                modifiers |= KeyboardModifiers::RShift;
                            }

                            if (kbs[VK_LCONTROL] & 0x80)
                            {
                                modifiers |= KeyboardModifiers::LControl;
                            }

                            if (kbs[VK_RCONTROL] & 0x80)
                            {
                                modifiers |= KeyboardModifiers::RControl;
                            }

                            if (kbs[VK_LMENU] & 0x80)
                            {
                                modifiers |= KeyboardModifiers::Alt;
                            }

                            if (kbs[VK_RMENU] & 0x80)
                            {
                                modifiers |= KeyboardModifiers::AltGr;
                            }

                            if (kbs[VK_CAPITAL] & 0x01)
                            {
                                modifiers |= KeyboardModifiers::CapsLock;
                            }

                            pSw->OnKeyPressed((char)ch,(int)wParam,modifiers);
                        }
                    } break;
                case WM_LBUTTONDOWN:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_LBUTTONDOWN (0x%X)\n", message);

                        if (!IsTouchEvent() || !IsMultiTouchSupported())
                        {
                            POINTS p = MAKEPOINTS(lParam);

                            pSw->OnMouseClick(p.x, p.y, Left, true);
                            pSw->OnMouseDown(p.x, p.y, Left);
                        }
                    } break;
                case WM_LBUTTONUP:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_LBUTTONUP (0x%X)\n", message);

                        if (!IsTouchEvent() || !IsMultiTouchSupported())
                        {
                            POINTS p = MAKEPOINTS(lParam);

                             pSw->OnMouseClick(p.x, p.y, Left, false);
                        }
                    } break;
                case WM_RBUTTONDOWN:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_RBUTTONDOWN (0x%X)\n", message);

                        if (!IsTouchEvent() || !IsMultiTouchSupported())
                        {
                            POINTS p = MAKEPOINTS(lParam);

                            pSw->OnMouseClick(p.x, p.y, Right, true);
                            pSw->OnMouseDown(p.x, p.y, Right);
                        }
                    } break;
                case WM_RBUTTONUP:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_RBUTTONUP (0x%X)\n", message);

                        if (!IsTouchEvent() || !IsMultiTouchSupported())
                        {
                            POINTS p = MAKEPOINTS(lParam);

                            pSw->OnMouseClick(p.x, p.y, Right, false);
                        }
                    } break;
                case WM_MOUSEWHEEL:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_MOUSEWHEEL (0x%X)\n", message);

                        POINTS p = MAKEPOINTS(lParam);
                        POINT pl = {p.x,p.y};
                        ScreenToClient(hWnd,&pl);
                        // DWORD fwKeys = GET_KEYSTATE_WPARAM(wParam);
                        int zDelta = GET_WHEEL_DELTA_WPARAM(wParam);
                        pSw->OnMouseScroll(pl.x,pl.y,zDelta/WHEEL_DELTA);
                    } break;
                case WM_MBUTTONDOWN:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_MBUTTONDOWN (0x%X)\n", message);

                        if (!IsTouchEvent() || !IsMultiTouchSupported())
                        {
                            POINTS p = MAKEPOINTS(lParam);

                            pSw->OnMouseClick(p.x, p.y, Middle, true);
                            pSw->OnMouseDown(p.x, p.y, Middle);
                        }
                    } break;
                case WM_MBUTTONUP:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_MBUTTONUP (0x%X)\n", message);

                        if (!IsTouchEvent() || !IsMultiTouchSupported())
                        {
                            POINTS p = MAKEPOINTS(lParam);

                            pSw->OnMouseClick(p.x, p.y, Middle, false);
                        }
                    } break;
                case WM_MOUSEMOVE:
                    {
                        //Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_MOUSEMOVE (0x%X)\n", message);

                        POINTS p = MAKEPOINTS(lParam);
                        MouseButton mb = None;

                        if ((wParam & MK_LBUTTON) == MK_LBUTTON)
                            mb = Left;
                        else if ((wParam & MK_RBUTTON) == MK_RBUTTON)
                            mb = Right;
                        else if ((wParam & MK_MBUTTON) == MK_MBUTTON)
                            mb = Middle;

                        pSw->OnMouseMove(p.x, p.y, mb,mb != None);
                    } break;
                case WM_TOUCH:
                    {
                        // Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_TOUCH (0x%X)\n", message);

                        UINT cInputs = LOWORD(wParam);
                        PTOUCHINPUT pInputs = new TOUCHINPUT[cInputs];
                        if (pInputs)
                        {
                            if (GetTouchInputInfo((HTOUCHINPUT)lParam, cInputs, pInputs, sizeof(TOUCHINPUT)))
                            {
                                pSw->OnTouchInternal(cInputs, pInputs);
                                CloseTouchInputHandle((HTOUCHINPUT)lParam);
                            }
                            else
                            {
                                //
                                // handle the error here
                                //
                            }
                        }

                        delete [] pInputs;

                    } break;
                case WM_NCCALCSIZE:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_NCCALCSIZE (0x%X)\n", message);
                    } break;
                case WM_DWMCOLORIZATIONCOLORCHANGED:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_DWMCOLORIZATIONCOLORCHANGED (0x%X)\n", message);
                    } break;
                case WM_QUERYENDSESSION:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_QUERYENDSESSION (0x%X)\n", message);
                    } break;
                case WM_ENDSESSION:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_ENDSESSION (0x%X)\n", message);
                    } break;
                case WM_KILLFOCUS:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_KILLFOCUS (0x%X)\n", message);
                    } break;
                case WM_QUIT:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_QUIT (0x%X)\n", message);
                    } break;
                case WM_DESTROY:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = WM_DESTROY (0x%X)\n", message);

                        PostQuitMessage(0);
                        ::Sleep(10); // Let the W_QUIT get pushed before exit this proc
                    } break;
                default:
                    {
                        Log(LogTarget::File, "ShellWindow():WndProc(): message = UNKNOWN (0x%X)\n", message);
                    } break;
                }
            }
        }

        return DefWindowProc(hWnd, message, wParam, lParam);;
    }
};