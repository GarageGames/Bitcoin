#include "F2M_Platform.h"
#include "F2M_Timer.h"
#include <CoreServices/CoreServices.h>
#include <mach/mach.h>
#include <mach/mach_time.h>

struct TimerDataOSX
{
    uint64_t start;
    uint64_t end;
    double timebase;
};

F2M_Timer::F2M_Timer()
{
    mTimerData = malloc(sizeof(TimerDataOSX));
    TimerDataOSX* data = (TimerDataOSX*)mTimerData;
    
    mach_timebase_info_data_t timebaseInfo;
    mach_timebase_info(&timebaseInfo);
    data->timebase = ((double)timebaseInfo.numer / (double)timebaseInfo.denom) * (1e-9);
}

F2M_Timer::~F2M_Timer()
{
    free(mTimerData);
}

void F2M_Timer::Start()
{
    TimerDataOSX* data = (TimerDataOSX*)mTimerData;
    data->start = mach_absolute_time();
}

void F2M_Timer::Stop()
{
    TimerDataOSX* data = (TimerDataOSX*)mTimerData;
    data->end = mach_absolute_time();
}

double F2M_Timer::GetDuration()
{
    TimerDataOSX* data = (TimerDataOSX*)mTimerData;

    uint64_t elapsed = data->end - data->start;
    double seconds = elapsed * data->timebase;

    return seconds;
}