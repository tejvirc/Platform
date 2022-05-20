#include <timer.h>

#pragma comment(lib, "winmm.lib")

namespace aristocrat
{
    timer::timer()
    {

        #if PLATFORM == PLATFORM_WINDOWS
        timeBeginPeriod(1);
        #endif //
            _start  = 0;
            _end = 0;

        _dStartTimeInMicroSec = 0;
        _dEndTimeInMicroSec = 0;
        
        reset();
    }
    
    timer::~timer() { }

    void timer::reset()
    {
        _start = getTime();
    }

    double timer::getTimeInMicroSec()
    {
        return (double) getTime() - _start;
    }
    double timer::getTimeInMilliSec()
    {
        return getTimeInMicroSec() / 1000.0;
    }
    double timer::getTimeInSec()
    {
        return getTimeInMicroSec() / 1000000;
    }

    double timer::getElapsedTimeInMicroSec()
    {
        _end = getTime();

    #if PLATFORM == PLATFORM_WINDOWS    
        _dStartTimeInMicroSec = (double)_start;
        _dEndTimeInMicroSec = (double)_end;
        
    #else
        _dStartTimeInMicroSec = _start;
        _dEndTimeInMicroSec = _end;
        
    #endif
        _start = _end;
        return _dEndTimeInMicroSec - _dStartTimeInMicroSec;
    }
    

    double timer::getElapsedTimeInMilliSec()
    {
        return getElapsedTimeInMicroSec() / 1000.0;
    }

    double timer::getElapsedTimeInSec()
    {
        return getElapsedTimeInMicroSec() / 1000000;
    }

    double timer::getElapsedTimeInMilliSec(uint64_t then)
    {
        return getElapsedTimeInMicroSec(then) / 1000.0;
    }
    double timer::getElapsedTimeInSec(uint64_t then)
    {
        return getElapsedTimeInMicroSec(then) / 1000000;
    }

    double timer::getElapsedTimeInMilliSec(uint64_t now, uint64_t then)
    {
        return getElapsedTimeInMicroSec(now,then) / 1000.0;
    }
    double timer::getElapsedTimeInSec(uint64_t now, uint64_t then)
    {
        return getElapsedTimeInMicroSec(now, then) / 1000000;
    }
    double timer::getElapsedTimeInMicroSec(uint64_t now, uint64_t then)
    {
#if PLATFORM == PLATFORM_WINDOWS    
        uint64_t dStartTimeInMicroSec = then;
        uint64_t dEndTimeInMicroSec = now;

#else
        uint64_t dStartTimeInMicroSec = then;
        uint64_t dEndTimeInMicroSec = now;
#endif

        return double(dEndTimeInMicroSec - dStartTimeInMicroSec);
    }
    

    double timer::getElapsedTimeInMicroSec(uint64_t then)
    {
        
        uint64_t now = getTime();
        
        
#if PLATFORM == PLATFORM_WINDOWS    
        
        double dStartTimeInMicroSec = (double)then; // *(1000000.0 / freq.QuadPart);
        double dEndTimeInMicroSec = (double)now;// *(1000000.0 / freq.QuadPart);

#else
        double dStartTimeInMicroSec = then;
        double dEndTimeInMicroSec = now;
#endif
        return dEndTimeInMicroSec - dStartTimeInMicroSec;
    }

    uint64_t timer::getTime()
    {
        uint64_t now;
#if PLATFORM == PLATFORM_WINDOWS
        LARGE_INTEGER li;
        QueryPerformanceCounter(&li);
        //now = li.QuadPart;

        LARGE_INTEGER freq = {0,0};
        QueryPerformanceFrequency(&freq);
        li.QuadPart *= 1000000;
        now = static_cast<uint64_t>(li.QuadPart / (freq.QuadPart));
#else
        timeval tv;
        gettimeofday(&tv, 0);
        now = (tv.tv_sec * 1000000.0) + tv.tv_usec;
#endif
        return now;
    }
}