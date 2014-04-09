#ifndef _F2M_TIMER_H_
#define _F2M_TIMER_H_

class F2M_Timer
{
public:
    F2M_Timer();
    ~F2M_Timer();

    void Start();
    void Stop();

    double GetDuration();

protected:
    void* mTimerData; 
};

#endif