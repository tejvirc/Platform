#pragma once

#include <platform.h>
#include <vector>
#include <algorithm>
#include <json/json.h>
#include <fstream>
#include <map>
#include <algorithm>
#include <ctime>
#include "logger.h"
#include "shellwindow.h"
#include "streammanager.h"
#include "animatedvalue.h"
#include "factory.h"
#include "referencemap.h"
#include "monacoshell.h"
#include "cabinetinfo.h"

int inline strcolor(const char* color)
{
    int a=255,r = 0,g = 0,b = 0;
    sscanf(color, "%02x%02x%02x%02x", &r, &g, &b,&a);

    return (a<<24)|(r<<16)|(g<<8)|b;
}

int inline strcolor(const std::string& color)
{
    return strcolor(color.c_str());
}

int inline multiplyAlpha(int color, double alpha)
{
    int a = (color & 0xff000000) >> 24;
    a = (int)(a * alpha);

    return (color & 0x00ffffff) | (a<<24);
};

int inline cmultiply(int color, double mul)
{
    int r = (color & 0x00ff0000) >> 16;
    int g = (color & 0x0000ff00) >> 8;
    int b = (color & 0x000000ff);
    int a = (color & 0xff000000);

    r= std::clamp((int)(r*mul),0,0xff) << 16;
    g= std::clamp((int)(g*mul),0,0xff)<< 8;
    b= std::clamp((int)(b*mul),0,0xff);

    return (a|r|g|b);
};


Gdiplus::StringAlignment inline stralign(const char* align)
{
    if (aristocrat::icasecmp(align, "left"))
        return Gdiplus::StringAlignmentNear;
    if (aristocrat::icasecmp(align, "center"))
        return Gdiplus::StringAlignmentCenter;
    if (aristocrat::icasecmp(align, "right"))
        return Gdiplus::StringAlignmentFar;
    if (aristocrat::icasecmp(align, "top"))
        return Gdiplus::StringAlignmentNear;
    if (aristocrat::icasecmp(align, "center"))
        return Gdiplus::StringAlignmentCenter;
    if (aristocrat::icasecmp(align, "bottom"))
        return Gdiplus::StringAlignmentFar;

    return Gdiplus::StringAlignmentNear;
}

Gdiplus::FontStyle inline strfontstyle(const char* align)
{
    if (aristocrat::icasecmp(align, "regular"))
        return Gdiplus::FontStyleRegular;
    if (aristocrat::icasecmp(align, "default"))
        return Gdiplus::FontStyleRegular;
    if (aristocrat::icasecmp(align, "bold"))
        return Gdiplus::FontStyleBold;
    if (aristocrat::icasecmp(align, "bolditalic"))
        return Gdiplus::FontStyleBoldItalic;
    if (aristocrat::icasecmp(align, "italic"))
        return Gdiplus::FontStyleItalic;
    if (aristocrat::icasecmp(align, "underline"))
        return Gdiplus::FontStyleUnderline;
    if (aristocrat::icasecmp(align, "strikeout"))
        return Gdiplus::FontStyleStrikeout;

    return Gdiplus::FontStyleRegular;
}

Gdiplus::FontStyle inline strfontstyle(const std::string& style)
{
    return strfontstyle(style.c_str());
}

Gdiplus::TextRenderingHint inline strtextrenderhint(const char* align)
{
    if (aristocrat::icasecmp(align, "antialias"))
        return Gdiplus::TextRenderingHintAntiAlias;
    if (aristocrat::icasecmp(align, "antialiasgf"))
        return Gdiplus::TextRenderingHintAntiAliasGridFit;
    if (aristocrat::icasecmp(align, "default"))
        return Gdiplus::TextRenderingHintSystemDefault;
    if (aristocrat::icasecmp(align, "single"))
        return Gdiplus::TextRenderingHintSingleBitPerPixel;
    if (aristocrat::icasecmp(align, "singlegf"))
        return Gdiplus::TextRenderingHintSingleBitPerPixelGridFit;
    if (aristocrat::icasecmp(align, "cleartype"))
        return Gdiplus::TextRenderingHintClearTypeGridFit;

    return Gdiplus::TextRenderingHintSystemDefault;
}

Gdiplus::TextRenderingHint inline strtextrenderhint(const std::string& style)
{
    return strtextrenderhint(style.c_str());
}

Gdiplus::SmoothingMode inline strsmoothingmode(const char* align)
{
    if (aristocrat::icasecmp(align, "default"))
        return Gdiplus::SmoothingModeDefault;
    if (aristocrat::icasecmp(align, "highspeed"))
        return Gdiplus::SmoothingModeHighSpeed;
    if (aristocrat::icasecmp(align, "highquality"))
        return Gdiplus::SmoothingModeHighQuality;
    if (aristocrat::icasecmp(align, "none"))
        return Gdiplus::SmoothingModeNone;
    if (aristocrat::icasecmp(align, "antialias"))
        return Gdiplus::SmoothingModeAntiAlias;
    if (aristocrat::icasecmp(align, "aa"))
        return Gdiplus::SmoothingModeAntiAlias;
 #if (GDIPVER >= 0x0110)
    if (aristocrat::icasecmp(align, "aa8x4"))
        return Gdiplus::SmoothingModeAntiAlias8x4;
    if (aristocrat::icasecmp(align, "aa8x8"))
        return Gdiplus::SmoothingModeAntiAlias8x8;
#endif

    return Gdiplus::SmoothingModeDefault;
}

Gdiplus::SmoothingMode inline strsmoothingmode(const std::string& style)
{
    return strsmoothingmode(style.c_str());
}

Gdiplus::StringAlignment inline stralign(const std::string& align)
{
    return stralign(align.c_str());
}


class IWidget : public virtual aristocrat::Interface
{
public:
     MONACO_INTERFACE_ID(IWidget, 0xeb0f12f0, 0xf487, 0x426e, 0xbd, 0x35, 0x1d, 0xfc, 0x31, 0x11, 0xfc, 0x9c);

    virtual ~IWidget() 
    {
    }

    virtual void OnPaint(Graphics& g) = 0;

    virtual void Initialize(IWindow* window, IShellContext*) 
    {
        if (x <= 1.01)
        {
            x *= window->Width();
        }

        if (y <= 1.01)
        {
            y *= window->Height();
        }

        if (width <= 1.01)
        {
            width *= window->Width();
        }

        if (height <= 1.01)
        {
            height *= window->Height();
        }
    }

    virtual void Update(double elapsedTimeMs)
    {
    }

    virtual bool HitTest(int x, int y, MouseButton mb, bool pressed)
    {
        return false;
    }

    void Release() 
    { 
        delete this; 
    }

    virtual void load_json(const Json::Value& v)
    {
        //
        // rect
        //

        auto rect = v["rect"];
        x = rect.get("x", 0).asDouble();
        y = rect.get("y", 0).asDouble();
        width = rect.get("width", 0).asDouble();
        height = rect.get("height", 0).asDouble();
    }

    virtual void Translate(double tx, double ty)
    {
        x += tx;
        y += ty;
    }

protected:
    double x = 0;
    double y = 0;
    double width = 0;
    double height = 0;
};


class LabelWidget : public IWidget
{
    MONACO_BEGIN_INTERFACE_MAP(LabelWidget)
        MONACO_INTERFACE_ENTRY(IWidget)
    MONACO_END_INTERFACE_MAP

public:
    constexpr static const char* name() { return "label"; }      // json factory name

protected:
    //
    // json properties
    //

    int                        fontcolor     = 0;
    int                        bkcolor       = 0;
    int                        bordercolor   = 0;
    int                        borderwidth   = 0;
    int                        margin        = 0;
    int                        fontSize      = 0;
    std::wstring               fontName;
    Gdiplus::StringAlignment   valign        = Gdiplus::StringAlignmentNear;
    Gdiplus::StringAlignment   halign        = Gdiplus::StringAlignmentNear;
    Gdiplus::FontStyle         fontstyle     = Gdiplus::FontStyleRegular;
    Gdiplus::TextRenderingHint texthint      = Gdiplus::TextRenderingHintSystemDefault;
    Gdiplus::SmoothingMode     smoothingmode = Gdiplus::SmoothingModeDefault;
    std::wstring               text;
    IWindow*                   _window       = nullptr;

    //
    // Graphics
    //

    Font* font = nullptr;

    virtual ~LabelWidget()
    {
        if (font)
            delete font;
    }

public:
    void OnPaint(Gdiplus::Graphics& g)
    {
        Gdiplus::RectF       borderRect((float)x, (float)y, (float)width, (float)height);
        Gdiplus::RectF       textRect((float)x+margin, (float)y+margin, (float)width-margin, (float)height-margin);
        Gdiplus::SolidBrush  textBrush(fontcolor);
        Gdiplus::SolidBrush  textbkBrush(bkcolor);
        Gdiplus::Pen         pen(bordercolor, (float) borderwidth);
        Gdiplus::StringFormat format = Gdiplus::StringFormat::GenericTypographic();

        format.SetAlignment(halign);
        format.SetLineAlignment(valign);

        g.SetTextRenderingHint(texthint);
        g.SetSmoothingMode(smoothingmode);
        g.FillRectangle(&textbkBrush, borderRect);
        g.DrawString(text.c_str(), (INT) text.length(), font, textRect, &format, &textBrush);

        if (borderwidth > 0)
            g.DrawRectangle(&pen, borderRect);

        g.SetTextRenderingHint(Gdiplus::TextRenderingHintSystemDefault);
        g.SetSmoothingMode(Gdiplus::SmoothingModeDefault);
    }

    void Initialize(IWindow* window, IShellContext*) override
    {
        _window = window;
        font = new Gdiplus::Font(fontName.c_str(), (float)fontSize, fontstyle);

        if (x <= 1.01) 
        {
            x *= window->Width();
        }

        if (y <= 1.01)
        {
            y *= window->Height();
        }

        if (width <= 1.01)
        {
            width *= window->Width();
        }

        if (height <= 1.01)
        {
            height *= window->Height();
        }
    }

    static LabelWidget* from_json(const Json::Value& v)
    {
        LabelWidget* widget = new LabelWidget();

        widget->load_json(v);

        return widget;
    }

protected:
    void load_json(const Json::Value& v) override
    {
        IWidget::load_json(v);

        //
        // rect
        //

        auto rect     = v["rect"];
        x             = rect.get("x",0).asDouble();
        y             = rect.get("y",0).asDouble();
        width         = rect.get("width",0).asDouble();
        height        = rect.get("height",0).asDouble();

        //
        // value
        //

        fontName      = aristocrat::MultiByte2WideCharacterString(ReferenceMap::Get(v,"font","arial").asCString());
        fontSize      = ReferenceMap::Get(v, "fontsize", 9).asInt();
        fontstyle     = strfontstyle(v.get("fontstyle","default").asString());
        texthint      = strtextrenderhint(v.get("texthint","default").asString());
        smoothingmode = strsmoothingmode(v.get("smoothingmode","default").asString());
        valign        = stralign(v.get("valign","top").asString());
        halign        = stralign(v.get("halign","left").asString());
        text          = aristocrat::MultiByte2WideCharacterString(v.get("text","").asCString());
        fontcolor     = strcolor(ReferenceMap::Get(v,"fontcolor","ffffff").asString());
        bkcolor       = strcolor(ReferenceMap::Get(v,"backgroundcolor","00000000").asString());
        bordercolor   = strcolor(ReferenceMap::Get(v,"bordercolor","00000000").asString());
        borderwidth   = ReferenceMap::Get(v,"borderwidth",0).asInt();
        margin        = ReferenceMap::Get(v,"margin",0).asInt();
    }
};


class RectWidget : public IWidget
{
    MONACO_BEGIN_INTERFACE_MAP(RectWidget)
        MONACO_INTERFACE_ENTRY(IWidget)
    MONACO_END_INTERFACE_MAP

public:
    constexpr static const char* name() { return "rectangle"; } // json factory name

protected:
    //
    // json properties
    //

    int bkcolor     = 0;
    int bordercolor = 0;
    int borderwidth = 0;
    int radius      = 0;
    int gradient    = 0;
    Gdiplus::SmoothingMode smoothingmode = Gdiplus::SmoothingModeAntiAlias;

    virtual ~RectWidget()
    {
    }

public:
    void DrawRoundRectangle(Gdiplus::Graphics* g, Pen *p, Gdiplus::Rect& rect, UINT8 radius)
    {
        if (g==NULL)
            return;

        Gdiplus::GraphicsPath path;

        if (radius < rect.Width/4)
            path.AddLine(rect.X + radius, rect.Y, rect.X + rect.Width - (radius * 2), rect.Y);

        path.AddArc(rect.X + rect.Width - (radius * 2), rect.Y, radius * 2, radius * 2, 270, 90);

        if (radius < rect.Height/4)
            path.AddLine(rect.X + rect.Width, rect.Y + radius, rect.X + rect.Width, rect.Y + rect.Height - (radius * 2));

        path.AddArc(rect.X + rect.Width - (radius * 2), rect.Y + rect.Height - (radius * 2), radius * 2, radius * 2, 0, 90);

        if (radius < rect.Width/4)
            path.AddLine(rect.X + rect.Width - (radius * 2), rect.Y + rect.Height, rect.X + radius, rect.Y + rect.Height);

        path.AddArc(rect.X, rect.Y + rect.Height - (radius * 2), radius * 2, radius * 2, 90, 90);

        if (radius < rect.Height/4)
            path.AddLine(rect.X, rect.Y + rect.Height - (radius * 2), rect.X, rect.Y + radius);

        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.CloseFigure(); 
        g->DrawPath(p, &path);
    }

    void FillRoundRectangle(Gdiplus::Graphics* g, Brush *p, Gdiplus::Rect& rect, UINT8 radius)
    {
        GraphicsPath path;

        if (g == NULL)
            return;

        if (radius < rect.Width/2)
            path.AddLine(rect.X + radius, rect.Y, rect.X + rect.Width - (radius * 2), rect.Y);

        path.AddArc(rect.X + rect.Width - (radius * 2), rect.Y, radius * 2, radius * 2, 270, 90);

        if (radius < rect.Height/2)
            path.AddLine(rect.X + rect.Width, rect.Y + radius, rect.X + rect.Width, rect.Y + rect.Height - (radius * 2));

        path.AddArc(rect.X + rect.Width - (radius * 2), rect.Y + rect.Height - (radius * 2), radius * 2, radius * 2, 0, 90);

        if (radius < rect.Width/2)
            path.AddLine(rect.X + rect.Width - (radius * 2), rect.Y + rect.Height, rect.X + radius, rect.Y + rect.Height);

        path.AddArc(rect.X, rect.Y + rect.Height - (radius * 2), radius * 2, radius * 2, 90, 90);

        if (radius < rect.Height/2)
            path.AddLine(rect.X, rect.Y + rect.Height - (radius * 2), rect.X, rect.Y + radius);

        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);

        path.CloseFigure();
        g->FillPath(p, &path);
    }

    void OnPaint(Graphics& g)
    {
        g.SetPageUnit(UnitPixel);
        Rect borderRect(static_cast<int>(x), static_cast<int>(y), static_cast<int>(width), static_cast<int>(height));
        SolidBrush textbkBrush(bkcolor);
        float gr = gradient / 100.0f;
        uint32_t lcol = cmultiply(bkcolor,1+gr);
        uint32_t dcol = cmultiply(bkcolor,1-gr);
        LinearGradientBrush bkbrush(PointF((float)x, (float)y),
            PointF((float)(x+width), (float)(y+height)),
            Color(lcol),   // lighter
            Color(dcol));  // darker

        Pen pen(bordercolor, (float) borderwidth);

        //
        // Button Fill/border
        //

        g.SetSmoothingMode(smoothingmode);
        FillRoundRectangle(&g,&bkbrush,borderRect,radius);
        DrawRoundRectangle(&g,&pen,borderRect,radius);
        g.SetSmoothingMode(SmoothingMode::SmoothingModeDefault);
    }

    static RectWidget* from_json(const Json::Value& v)
    {
        RectWidget* widget = new RectWidget();
        widget->load_json(v);

        return widget;
    }

protected:
    void load_json(const Json::Value& v) override
    {
        IWidget::load_json(v);

        //
        // value
        //

        bkcolor       = strcolor(ReferenceMap::Get(v,"backgroundcolor","00000000").asString());
        bordercolor   = strcolor(ReferenceMap::Get(v,"bordercolor","00000000").asString());
        borderwidth   = ReferenceMap::Get(v,"borderwidth",0).asInt();
        radius        = ReferenceMap::Get(v,"radius",0).asInt();
        gradient      = ReferenceMap::Get(v,"gradient",0).asInt(); 
        smoothingmode = strsmoothingmode(v.get("smoothingmode","default").asString());
    }
};


class PanelWidget : public IWidget
{
    MONACO_BEGIN_INTERFACE_MAP(PanelWidget)
        MONACO_INTERFACE_ENTRY(IWidget)
    MONACO_END_INTERFACE_MAP

    std::vector<IWidget*> _widgets;

public:
    constexpr static const char* name() { return "panel"; }      // json factory name

protected:
    virtual ~PanelWidget()
    {
        for (auto w : _widgets)
            w->Release();

        _widgets.clear();
    }

public:
    void OnPaint(Graphics& g) override
    {
        for (auto&& w : _widgets) 
        {
            w->OnPaint(g);
        }
    }

    virtual void Initialize(IWindow* window, IShellContext* ctx)
    {
        IWidget::Initialize(window, ctx);

        for (auto&& w : _widgets)
        {
            w->Initialize(window, ctx);
            w->Translate(x, y);
        }
    }

    virtual void Update(double elapsedTimeMs) 
    {
        IWidget::Update(elapsedTimeMs);

        for (auto&& w : _widgets)
        {
            w->Update(elapsedTimeMs);
        }
    }

    virtual bool HitTest(int x, int y, MouseButton mb, bool pressed) 
    {
        IWidget::HitTest(x,y, mb, pressed);

        for (auto&& w : _widgets)
        {
            if (w->HitTest(x, y, mb, pressed)) return true;
        }

        return false;
    }

    static PanelWidget* from_json(const Json::Value& v)
    {
        PanelWidget* widget = new PanelWidget();
        widget->load_json(v);

        return widget;
    }

    virtual void Translate(double tx, double ty) override
    {
        IWidget::Translate(tx, ty);

        for (auto&& w : _widgets)
        {
            w->Translate(tx, ty);
        }
    }

    void load_json(const Json::Value& v) override
    {
        IWidget::load_json(v);

        //
        // rect
        //

        auto widgets = v.get("widgets", Json::Value(Json::ValueType::arrayValue)); 
        auto&& factory = NameClassFactory<IWidget, const Json::Value&>::Instance();

        for (auto it = widgets.begin(); it != widgets.end(); ++it)
        {
            std::string type = (*it)["type"].asCString();
            auto widget = factory.create(type.c_str(), *it);
            if (widget != nullptr)
            {
                _widgets.push_back(widget);
            }
        }
    }
};


class ButtonWidget : public RectWidget
{
    MONACO_BEGIN_INTERFACE_MAP(ButtonWidget)
        MONACO_INTERFACE_ENTRY(RectWidget)
        MONACO_INTERFACE_ENTRY(IWidget)
    MONACO_END_INTERFACE_MAP

public:
    constexpr static const char* name() { return "button"; }      // json factory name

protected:
    int                        fontcolor      = 0;
    int                        margin         = 0;
    int                        fontSize       = 0;
    std::wstring               fontName;
    Gdiplus::StringAlignment   valign         = StringAlignmentNear;
    Gdiplus::StringAlignment   halign         = StringAlignmentNear;
    Gdiplus::TextRenderingHint texthint       = Gdiplus::TextRenderingHintSystemDefault;
    std::wstring               text;
    Gdiplus::FontStyle         fontstyle      = FontStyleRegular;

    //
    // Graphics
    //

    Gdiplus::Font*             font           = nullptr;
    std::string                application;
    IShellContext*             _shellContext;

    virtual ~ButtonWidget()
    {
        if (font)
            delete font;

        _shellContext = nullptr;
    }

public:
    void OnPaint(Graphics& g)
    {
        RectWidget::OnPaint(g);
        g.SetTextRenderingHint(texthint);
        Gdiplus::Rect        borderRect(static_cast<int>(x), static_cast<int>(y), static_cast<int>(width), static_cast<int>(height));
        Gdiplus::RectF       textRect((float)x+margin, (float)y+margin, (float)width-margin, (float)height-margin);
        Gdiplus::SolidBrush  textBrush(fontcolor);
        Gdiplus::SolidBrush  textbkBrush(bkcolor);

        //
        // Text
        //

        Gdiplus::StringFormat format = Gdiplus::StringFormat::GenericTypographic();
        format.SetAlignment(halign);
        format.SetLineAlignment(valign);

        g.DrawString(text.c_str(), (INT) text.length(), font, textRect, &format, &textBrush);
        g.SetTextRenderingHint(Gdiplus::TextRenderingHintSystemDefault);
    }

    void Initialize(IWindow* window, IShellContext* ctx) override
    {
        RectWidget::Initialize(window, ctx);
        _shellContext = ctx;
        font        = new Gdiplus::Font(fontName.c_str(), (float)fontSize,fontstyle);
    }

    bool HitTest(int x, int y, MouseButton mb, bool pressed) override
    {
        int px = x - static_cast<int>(this->x);
        int py = y - static_cast<int>(this->y);

        if (px >0 && px < width && py > 0 && py < height) 
            return OnClick(px,py,mb,pressed);

        return IWidget::HitTest(x,y,mb,pressed);
    }

    bool OnClick(int x, int y, MouseButton mb, bool pressed)
    {
        if (pressed && _shellContext != nullptr)
        { 
            _shellContext->StartApplication(application.c_str());
        }

        return true;
    }

    static ButtonWidget* from_json(const Json::Value& v)
    {
        ButtonWidget* widget = new ButtonWidget();

        widget->load_json(v);

        return widget;
    }

protected:
    void load_json(const Json::Value& v) override
    {
        RectWidget::load_json(v);

        //
        // value
        //

        fontName    = aristocrat::MultiByte2WideCharacterString(ReferenceMap::Get(v,"font","arial").asCString());
        fontSize    = ReferenceMap::Get(v, "fontsize", 20).asInt();
        valign      = stralign(v.get("valign","center").asString());
        halign      = stralign(v.get("halign","center").asString());
        text        = aristocrat::MultiByte2WideCharacterString(v.get("text","").asCString());
        fontcolor   = strcolor(ReferenceMap::Get(v,"fontcolor","ffffff").asString());
        fontstyle   = strfontstyle(v.get("fontstyle","default").asString());
        texthint    = strtextrenderhint(v.get("texthint","default").asString());
        margin      = ReferenceMap::Get(v,"margin",0).asInt();
        application = ReferenceMap::Get(v,"application","").asString();
    }
};


class DateTimeWidget : public LabelWidget
{
 /* Format options
    %a Abbreviated weekday name
    %A Full weekday name
    %b Abbreviated month name
    %B Full month name
    %c Date and time representation appropriate for locale
    %d Day of month as decimal number (01 – 31)
    %H Hour in 24-hour format (00 – 23)
    %I Hour in 12-hour format (01 – 12)
    %j Day of year as decimal number (001 – 366)
    %m Month as decimal number (01 – 12)
    %M Minute as decimal number (00 – 59)
    %p Current locale's A.M./P.M. indicator for 12-hour clock
    %S Second as decimal number (00 – 59)
    %U Week of year as decimal number, with Sunday as first day of week (00 – 53)
    %w Weekday as decimal number (0 – 6; Sunday is 0)
    %W Week of year as decimal number, with Monday as first day of week (00 – 53)
    %x Date representation for current locale
    %X Time representation for current locale
    %y Year without century, as decimal number (00 – 99)
    %Y Year with century, as decimal number
    %z, %Z Either the time-zone name or time zone abbreviation, depending on registry settings; no characters if time zone is unknown
    %% Percent sign
*/

    std::string textBuffer;
    std::string format;
    double timer = 0;

    virtual ~DateTimeWidget()
    {
    }

public:
    constexpr static const char* name() { return "datetime"; }      // json factory name
    std::string cached_string;

    void OnPaint(Graphics& g) override
    {
        text = BuildText(g);
        LabelWidget::OnPaint(g);
    }

    std::wstring BuildText(Graphics& g)
    {
        return aristocrat::MultiByte2WideCharacterString(cached_string.c_str());
    }

    void Initialize(IWindow* window, IShellContext* ctx) override
    {
        LabelWidget::Initialize(window,ctx);
    }

    void Update(double elapsedTimeMs)
    {
        timer+=elapsedTimeMs;

        if (timer > 500)
        {
            while(timer > 500)
                timer-=500;
        }
        else
        {
            return;
        }

        char time[255];
        tm now;
        time_t ltime;
        ::time(&ltime);
        _localtime64_s(&now, &ltime);
        strftime(time,255,format.c_str(), &now);

        if (cached_string.compare(time) == 0)
            return;

        cached_string = time;
        _window->Invalidate();
    }

    static DateTimeWidget* from_json(const Json::Value& v)
    {
        DateTimeWidget* tsw = new DateTimeWidget();

        tsw->load_json(v);

        return tsw;
    }

protected:
    void load_json(const Json::Value& v) override
    {
        LabelWidget::load_json(v);
        format = v.get("format","%I:%M %p").asCString();
    }
};


class TextStreamWidget : public LabelWidget
{
    std::string textBuffer;
    std::string streamName;

    virtual ~TextStreamWidget()
    {
    }

public:
    constexpr static const char* name() { return "textstream"; }      // json factory name

    void OnPaint(Graphics& g) override
    {
        text = BuildText(g);
        LabelWidget::OnPaint(g);
    }

    std::wstring BuildText(Graphics& g)
    {
        RectF boundingBox;

        g.MeasureString(L"Abc\r\nAbc", 8, font, PointF(0, 0), StringFormat::GenericTypographic(), &boundingBox);
        size_t numlines = (size_t)(height / (boundingBox.Height/2));
        textBuffer.clear();
        auto textIterator = StreamManager::Instance()->begin(streamName.c_str());
        size_t feedlines = StreamManager::Instance()->size(streamName.c_str());

        for (; textIterator != StreamManager::Instance()->end(streamName.c_str()); textIterator++)
        {
            if (feedlines > numlines)
            {
                --feedlines;

                continue;
            }

            textBuffer.append((*textIterator).c_str());

            if (!textBuffer.empty() && textBuffer.back() != '\n')
                textBuffer += "\n";
        }

        return aristocrat::MultiByte2WideCharacterString(textBuffer.c_str());
    }

    void Initialize(IWindow* window, IShellContext* ctx) override
    {
        LabelWidget::Initialize(window,ctx);
    }

    static TextStreamWidget* from_json(const Json::Value& v)
    {
        TextStreamWidget* tsw = new TextStreamWidget();

        tsw->load_json(v);

        return tsw;
    }

protected:
    void load_json(const Json::Value& v) override
    {
        LabelWidget::load_json(v);
        streamName = v.get("stream","").asCString();
    }
};


class ProgressBarWidget : public IWidget, public IDataReceiver
{
public:
    MONACO_BEGIN_INTERFACE_MAP(ProgressBarWidget)
        MONACO_INTERFACE_ENTRY(IWidget)
        MONACO_INTERFACE_ENTRY(IDataReceiver)
    MONACO_END_INTERFACE_MAP

    //
    // json properties
    //

    int fontcolor             = 0;
    int fillcolor             = 0;
    int bkcolor               = 0;
    int bordercolor           = 0;
    int borderwidth           = 0;
    int fontSize              = 0;
    int dataid                = 0;
    int progress              = 0;
    bool autohide             = false;
    std::wstring fontName;
    std::string prefix_text;
    std::string postfix_text;

    //
    // Graphics
    //

    FontStyle fontstyle = FontStyleRegular;
    Gdiplus::TextRenderingHint texthint     = Gdiplus::TextRenderingHintSystemDefault;
    Gdiplus::SmoothingMode smoothingmode    = Gdiplus::SmoothingModeDefault;
    Font* font                              = nullptr;
    bool visible                            = true;
    aristocrat::AnimatedValue<double> _opacity;
    IWindow* _window                        = nullptr;

    virtual ~ProgressBarWidget()
    {
        if (font)
            delete font;
    }

public:
    constexpr static const char* name() { return "progressbar"; }      // json factory name

    void OnPaint(Graphics& g)
    {
        if (_opacity.GetValue() == 0)
            return;

        g.SetSmoothingMode(smoothingmode);
        g.SetTextRenderingHint(texthint);
        double alpha = _opacity.GetValue();

        int afontcolor  = multiplyAlpha(fontcolor,alpha);
        int afillcolor  = multiplyAlpha(fillcolor ,alpha);
        int abkcolor    = multiplyAlpha(bkcolor ,alpha);
        int abordercolor= multiplyAlpha(bordercolor,alpha);

        RectF       backgroundRect((float) x, (float) y, (float) width, (float) height);
        RectF       progressRect((float) x, (float) y, (float) (width*progress/100.0f), (float) height);

        SolidBrush  textBrush(afontcolor);
        SolidBrush  bkBrush(abkcolor);
        SolidBrush  fillbkBrush(afillcolor);
        Pen         pen(abordercolor, (float)borderwidth);

        g.FillRectangle(&bkBrush, backgroundRect);
        g.FillRectangle(&fillbkBrush, progressRect);
        g.DrawRectangle(&pen, backgroundRect);

        StringFormat format = StringFormat::GenericTypographic();
        format.SetAlignment(StringAlignmentCenter);
        format.SetLineAlignment(StringAlignmentCenter);

        std::wstringstream ss;
        ss << prefix_text.c_str() << progress << L"%" << postfix_text.c_str(); // Should the "%" be removed and the json can add if the user wants?
        std::wstring text = ss.str();
        g.DrawString(text.c_str(), (INT) text.length(), font, backgroundRect, &format, &textBrush);
        g.SetSmoothingMode(Gdiplus::SmoothingModeDefault);
        g.SetTextRenderingHint(Gdiplus::TextRenderingHintSystemDefault);
    }

    void Update(double elapsedTimeMs)
    {
        _opacity.Update(elapsedTimeMs);

        if (autohide) 
        {
            if (!_opacity.IsCompleted())
                _window->Invalidate();
        }
    }

    void Initialize(IWindow* window, IShellContext* context) override
    {
        IWidget::Initialize(window, context);
        _window = window;
        font = new Gdiplus::Font(fontName.c_str(), (float)fontSize, fontstyle);
        _opacity.OnComplete([=](int) { _window->Invalidate(); });
    }

    void OnData(DWORD id, const char* data, size_t length)
    {
        if (dataid == id && data != nullptr && length > 0)
        {
            progress = atoi(data);

            Log(LogTarget::File, "ProgressBarWidget():OnData(): progress = '%d'.\n", progress);

            if (autohide)
            {
                if ((progress <= 0) || (progress >= 100))
                {
                    _opacity.PushTo(aristocrat::interpolation::smoothstep,0.0,2000.0); // start fading out
                }
                else // if (progress >= 0 && progress < 100)
                {
                    Log(LogTarget::File, "ProgressBarWidget():OnData(): Hiding the Progress Bar.\n");

                    _opacity.PushTo(aristocrat::interpolation::smoothstep,1.0,300.0); // start fading in
                }
            }

            _window->Invalidate();
        }
    }

    static ProgressBarWidget* from_json(const Json::Value& v)
    {
        ProgressBarWidget* cw = new ProgressBarWidget();

        cw->load_json(v);

        return cw;
    }

protected:
    void load_json(const Json::Value& v) override
    {
        IWidget::load_json(v);
        auto rect = v["rect"];

        fontName      = aristocrat::MultiByte2WideCharacterString(ReferenceMap::Get(v,"font","arial").asCString());
        fontSize      = ReferenceMap::Get(v,"fontsize",9).asInt();

        //
        // other
        //
        
        dataid        = ReferenceMap::Get(v,"dataid",0).asInt();
        autohide      = ReferenceMap::Get(v,"autohide",false).asBool();
        fontcolor     = strcolor(ReferenceMap::Get(v,"fontcolor","ffffffff").asString());
        fillcolor     = strcolor(ReferenceMap::Get(v,"fillcolor","ffffffff").asString());
        borderwidth   = ReferenceMap::Get(v,"borderwidth", 1).asInt();
        bordercolor   = strcolor(ReferenceMap::Get(v,"bordercolor","ffffffff").asString());
        bkcolor       = strcolor(ReferenceMap::Get(v,"backgroundcolor","00000000").asString());
        fontstyle     = strfontstyle(v.get("fontstyle","default").asString());
        texthint      = strtextrenderhint(v.get("texthint","default").asString());
        smoothingmode = strsmoothingmode(v.get("smoothingmode","default").asString());
        prefix_text   = v.get("prefix","").asString();
        postfix_text  = v.get("postfix", "").asString();

        if (!this->autohide)
            _opacity.SetValue(1.0);
    }
};


class GuiWindow : public IShellContextService, public IDataReceiver, public IWindow
{
    //
    // JSon properties
    //

    std::string _title;
    std::string _monitorName;
    std::string _backgroundImageFileName;
    int         _monitorIndex             = 0;

    //
    // Graphics
    //

    Image*                                       _background   = nullptr;
    ShellWindow*                                 _window       = nullptr;
    std::vector<IWidget*>                        _widgets;
    NameClassFactory<IWidget,const Json::Value&>& _factory;
    IShellContext*                               _shellContext = nullptr;

public:
    MONACO_BEGIN_INTERFACE_MAP(GuiWindow)
        MONACO_INTERFACE_ENTRY(IShellContextService)
        MONACO_INTERFACE_ENTRY(IDataReceiver)
        MONACO_INTERFACE_ENTRY(IWindow)
    MONACO_END_INTERFACE_MAP

    GuiWindow(CabinetInfo* pCabinetInfoObj) :
        _factory(NameClassFactory<IWidget, const Json::Value&>::Instance()),
        _pMonInfo(nullptr)
    {
        Log(LogTarget::File, "GuiWindow(): Constructing.\n");

        _factory.Register<ButtonWidget>([](auto v)      { return ButtonWidget::from_json(v); });
        _factory.Register<LabelWidget>([](auto v)       { return LabelWidget::from_json(v); });
        _factory.Register<TextStreamWidget>([](auto v)  { return TextStreamWidget::from_json(v); });
        _factory.Register<ProgressBarWidget>([](auto v) { return ProgressBarWidget::from_json(v); });
        _factory.Register<RectWidget>([](auto v)        { return RectWidget::from_json(v); });
        _factory.Register<PanelWidget>([](auto v)       { return PanelWidget::from_json(v); });
        _factory.Register<DateTimeWidget>([](auto v)    { return DateTimeWidget::from_json(v); });

        _pCabinetInfo = pCabinetInfoObj;
        _pMonInfo = new MonitorInfo(_pCabinetInfo);
    }

    void Initialize(IShellContext* shellContext)
    {
        _shellContext = shellContext;
        Initialize();
    }

    virtual int Width()
    {
        return _window ? _window->Width() : 0;
    }

    virtual int Height()
    {
        return _window ? _window->Height() : 0;
    }

    void Hide()
    {
        if (_window)
            _window->Hide();
    }

    virtual ~GuiWindow()
    {
        if (_background)
        {
            delete _background;
            _background = nullptr;
        }

        for (auto w: _widgets)
            w->Release();

        _widgets.clear();

        if (_window)
            delete _window;

        if (_pMonInfo)
            delete _pMonInfo;

        _pCabinetInfo = nullptr;
    }

    ShellWindow* window()
    {
        return _window;
    }

    void Initialize()
    {
        Log(LogTarget::File, "GuiWindow():Initialize(): _title: '%s'.\n", _title.c_str());

        auto replaceStr = [](const std::wstring& str, const std::wstring& from, const std::wstring& to) -> std::wstring {
                size_t start_pos = str.find(from);

                if (start_pos == std::string::npos)
                    return str;

                std::wstring replacedStr = str;
                replacedStr.replace(start_pos, from.length(), to);

                return replacedStr;
        };

        _window = new ShellWindow(_pCabinetInfo);

        if (E_FAIL != _window->CreateMonitorIndex(_title.c_str(), _monitorIndex))
        {
            std::wstring imagefile = aristocrat::MultiByte2WideCharacterString(_backgroundImageFileName.c_str());
            std::vector<std::wstring> suffixesToTry = {
                std::to_wstring(Width()) + L"x" + std::to_wstring(Height()),
                Height() > Width() ? L"portrait" : L"landscape",
                L"default",
                L""
            };

            Log(LogTarget::File, "GuiWindow():Initialize(): Width & Height: '%dx%d'.\n", Width(), Height());

            for (auto&& suffix : suffixesToTry)
            {
                auto imgFile = replaceStr(imagefile, L"%s", suffix);
                if (!imgFile.empty() && PathFileExistsW(imgFile.c_str()))
                {
                    Log(LogTarget::File, "GuiWindow():Initialize(): imgFile: '%ws'.\n", imgFile.c_str());

                    _background = Image::FromFile(imgFile.c_str());
                    if (_background)
                    {
                        break;
                    }
                }
            }

            if (!imagefile.empty() && !_background)
            {
                Log(LogTarget::File, "GuiWindow():Initialize(): imagefile: '%ws'.\n", imagefile.c_str());

                _background = Image::FromFile(imagefile.c_str());
            }
        }

        for (auto w : _widgets)
        {
            w->Initialize(this, _shellContext);            
        }

        _window->OnMouseClick([&](int x, int y, MouseButton mb, bool pressed)
        {
            for (auto w : _widgets)
            {
                if (w->HitTest(x,y,mb,pressed))
                    break;
            }
        });

        _window->OnPaint([=](Graphics& g)
        {
            using namespace Gdiplus;

            if (_background)
            {
                g.DrawImage(_background,0,0, _window->Width(), _window->Height());
            }

            for (auto w : _widgets)
            {
                w->OnPaint(g);
            }
        });

        Log(LogTarget::File, "GuiWindow():Initialize(): Complete.\n");
    }

    static GuiWindow* from_json(const Json::Value& v, CabinetInfo* pCabinetInfoObj)
    {
        GuiWindow* window = new GuiWindow(pCabinetInfoObj);
        window->load_json(v);

        return window;
    }

    void load_json(const Json::Value& v)
    {
        Log(LogTarget::File, "GuiWindow():load_json()\n");

        _title                      = v.get("title","no title").asCString();
        _backgroundImageFileName    = ReferenceMap::Get(v,"background","").asCString();
        auto monitor                = ReferenceMap::Get(v,"monitor",0);

        Log(LogTarget::File, "GuiWindow():load_json(): monitor: '%s'.\n", monitor.asCString());

        //
        // If the monitor item is a string, we'll look for the int value
        // from an external utility
        //

        if (monitor.isString())
        {
            _monitorIndex = -1;

            //
            // We'll try to get the proper monitor indexes uses Windows data
            // structures.
            //

            if (_pMonInfo != nullptr)
            {
                _monitorIndex = _pMonInfo->GetDisplayInfoByRole(monitor.asCString());
            }
        }
        else if (monitor.isInt())
        {
            _monitorIndex = monitor.asInt();
        }

        Log(LogTarget::File, "GuiWindow():load_json(): _monitorIndex: '%d'.\n", _monitorIndex);

        auto widgets = v["widgets"];
        for (auto it = widgets.begin(); it != widgets.end();++it)
        {
            std::string type = (*it)["type"].asCString();
            auto widget = _factory.create(type.c_str(),*it);

            if (widget != nullptr)
            {
                _widgets.push_back(widget);
            }
        }

        Log(LogTarget::File, "GuiWindow():load_json(): Complete.\n");
    }

    void Update(double elapsedTimeMs)
    {
        for (auto w : _widgets)
        {
            w->Update(elapsedTimeMs);
        }
    }

    //
    // IDataReceiver interface
    //

    void OnData(DWORD id, const char* data, size_t length)
    {
        for (auto w : _widgets)
        {
            IDataReceiver* dr = nullptr;

            if (w->QueryInterface<IDataReceiver>(&dr))
                dr->OnData(id,data,length);
        }
    }

    //
    // IWindow interface
    //

    void Invalidate() override
    {
        if (_window != nullptr)
            _window->Invalidate();
    }

public:
    void Release()
    {
        delete this;
    }

private:
    MonitorInfo* _pMonInfo;
    CabinetInfo* _pCabinetInfo;
};


class GuiManager : public IShellContextService, public IDataReceiver
{
    GuiManager(CabinetInfo* pCabinetInfoObj) :
        _pCabinetInfo(pCabinetInfoObj),
        _on_display_configuration_change(nullptr),
        _on_mouse_down(nullptr),
        _shellContext(nullptr)
    {
        Log(LogTarget::File, "GuiManager(): Constructing.\n");
    }

    virtual ~GuiManager()
    {
        std::for_each(_windows.begin(),_windows.end(), [](auto a){a->Release();});
        _windows.clear();
        _pCabinetInfo = nullptr;
        _on_display_configuration_change = nullptr;
        _on_mouse_down = nullptr;
        _shellContext = nullptr;
    }

public:
    MONACO_BEGIN_INTERFACE_MAP(GuiManager)
        MONACO_INTERFACE_ENTRY(IShellContextService)
        MONACO_INTERFACE_ENTRY(IDataReceiver)
    MONACO_END_INTERFACE_MAP

    void Release()
    {
        delete this;
    }

    static GuiManager* from_json(const Json::Value& v, CabinetInfo* pCabinetInfoObj)
    {
        GuiManager* ui = new GuiManager(pCabinetInfoObj);

        //
        // config
        //

        ui->load_json(v);

        return ui;
    }

    void load_json(const Json::Value& v)
    {
         _title = v.get("title","no title").asCString();

        //
        // applications node
        //

        auto windows = v["windows"];
        for (auto w : windows)
        {
            auto win = GuiWindow::from_json(w, _pCabinetInfo);
            if (win != nullptr)
            {
                _windows.push_back(win);
            }
        }
    }

    void OnData(DWORD id, const char* data, size_t length)
    {
        if (id > 0xffffff) // filter out data id's less than this
        {
            return;
        }

        for (auto win : _windows)
        {
            win->OnData(id,data,length);
        }
    }

    void Hide()
    {
        for (auto win : _windows)
        {
            win->Hide();
        }
    }

    void BroadcastOnDisplayChanged()
    {
        Log(LogTarget::File, "GuiManager():BroadcastOnDisplayChanged()\n");

        if (_on_display_configuration_change)
            _on_display_configuration_change();
        //else
        //{
        //    for (auto win : _windows) // let every window know that we have a display change.
        //    {
        //        win->window()->UpdateDisplaySettings();
        //    }
        //}
    }

    void BroadcastMouseDown(GuiWindow* window, int x, int y, MouseButton button)
    {
        if (_on_mouse_down)
            _on_mouse_down(window,x,y,button);
    }

    void Update(double elapsedTimeInMs)
    {
        bool wasDirty = StreamManager::Instance()->isDirty();

        for (auto w: _windows)
        {
            w->Update(elapsedTimeInMs);

            if (wasDirty) 
                w->Invalidate();
        }
    }

    void Initialize(IShellContext* shellContext)
    {
        Log(LogTarget::File, "GuiManager():Initialize()\n");

        _shellContext = shellContext;

        IDataReceiver* pDataReceiver = nullptr;
        _shellContext->QueryInterface<IDataReceiver>(&pDataReceiver);

        for (auto win : _windows)
        {
            win->Initialize(_shellContext);
            if (win->window())
            {
                win->window()->OnData([=](DWORD id, const char* data, size_t length)
                {
                    pDataReceiver->OnData(id,data,length);
                });

                win->window()->OnDisplayChanged([=]()
                {
                    BroadcastOnDisplayChanged();
                });

#ifdef _DEBUG
                win->window()->OnMouseDown([=](int x, int y, MouseButton button)
                {
                    BroadcastMouseDown(win, x, y, button);
                });
#endif
            }
        }

        Log(LogTarget::File, "GuiManager():Initialize(): Complete.\n");
    }

    //
    // Events
    //

    void OnDisplayConfigurationChange(std::function<void()> on_display_configuration_change)
    {
        _on_display_configuration_change = on_display_configuration_change;
    }

    void OnMouseDown(std::function<void(GuiWindow* , int , int , MouseButton)> on_mouse_down)
    {
        _on_mouse_down = on_mouse_down;
    }

private:
    std::function<void()>   _on_display_configuration_change;
    std::function<void(GuiWindow*,int,int,MouseButton)> _on_mouse_down;
    IShellContext*          _shellContext;
    std::vector<GuiWindow*> _windows;
    std::string             _title;
    CabinetInfo*            _pCabinetInfo;
};