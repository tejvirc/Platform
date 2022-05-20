#pragma once

#ifndef __PERF_TIMER_H__
#define __PERF_TIMER_H__

#include <windows.h>
#include <stdint.h>

namespace aristocrat
{
    class timer
    {
    private:
        double _dStartTimeInMicroSec;
        double _dEndTimeInMicroSec;
        uint64_t _start;
        uint64_t _end;

    public:
        timer();
        virtual ~timer();

        void reset();
        double getElapsedTimeInSec();
        double getElapsedTimeInMilliSec();
        double getElapsedTimeInMicroSec();
        static uint64_t getTime();
        double getTimeInSec();
        double getTimeInMilliSec();
        double getTimeInMicroSec();
        static double getElapsedTimeInMicroSec(uint64_t then);
        static double getElapsedTimeInMilliSec(uint64_t then);
        static double getElapsedTimeInSec(uint64_t then);
        static double getElapsedTimeInMilliSec(uint64_t now,uint64_t then);
        static double getElapsedTimeInMicroSec(uint64_t now, uint64_t then);
        static double getElapsedTimeInSec(uint64_t now, uint64_t then);
    };
}

#endif // __PERF_TIMER_H__