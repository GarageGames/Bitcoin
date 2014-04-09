#include <Windows.h>
#include "F2M_Timer.h"

struct TimerDataWin32
{
    double          clocksPerSecond;
    LARGE_INTEGER   start;
    LARGE_INTEGER   stop;
};

F2M_Timer::F2M_Timer()
{
    mTimerData = malloc(sizeof(TimerDataWin32));
    TimerDataWin32* data = (TimerDataWin32*)mTimerData;

    LARGE_INTEGER freq;
    QueryPerformanceFrequency(&freq);
    data->clocksPerSecond = 1.0 / freq.QuadPart;
}

F2M_Timer::~F2M_Timer()
{
    free(mTimerData);
}

void F2M_Timer::Start()
{
    TimerDataWin32* data = (TimerDataWin32*)mTimerData;
    QueryPerformanceCounter(&data->start);
}

void F2M_Timer::Stop()
{
    TimerDataWin32* data = (TimerDataWin32*)mTimerData;
    QueryPerformanceCounter(&data->stop);
}

double F2M_Timer::GetDuration()
{
    TimerDataWin32* data = (TimerDataWin32*)mTimerData;
    double clocks = data->stop.QuadPart - data->start.QuadPart;
    double seconds = clocks * data->clocksPerSecond;
    return seconds;
}