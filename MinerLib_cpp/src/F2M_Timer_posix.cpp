#include "F2M_Platform.h"
#include "F2M_Timer.h"
#include <time.h>

struct TimerDataPosix
{
    timespec start;
    timespec end;
};

F2M_Timer::F2M_Timer()
{
    mTimerData = malloc(sizeof(TimerDataPosix));
    TimerDataPosix* data = (TimerDataPosix*)mTimerData;
}

F2M_Timer::~F2M_Timer()
{
    free(mTimerData);
}

void F2M_Timer::Start()
{
    TimerDataPosix* data = (TimerDataPosix*)mTimerData;
    clock_gettime(CLOCK_PROCESS_CPUTIME_ID, &data->start);
}

void F2M_Timer::Stop()
{
    TimerDataPosix* data = (TimerDataPosix*)mTimerData;
    clock_gettime(CLOCK_PROCESS_CPUTIME_ID, &data->end);
}

double F2M_Timer::GetDuration()
{
    TimerDataPosix* data = (TimerDataPosix*)mTimerData;

    timespec temp;
	if ( (data->end.tv_nsec - data->start.tv_nsec) < 0 ) 
    {
		temp.tv_sec = data->end.tv_sec - data->start.tv_sec - 1;
		temp.tv_nsec = 1000000000 + data->end.tv_nsec - data->start.tv_nsec;
	} 
    else 
    {
		temp.tv_sec = data->end.tv_sec - data->start.tv_sec;
		temp.tv_nsec = data->end.tv_nsec - data->start.tv_nsec;
	}

    double seconds = temp.tv_sec + (temp.tv_nsec / 1000000000);

    return seconds;
}