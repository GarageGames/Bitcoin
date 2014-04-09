#ifndef _F2M_GPUTHREAD_H_
#define _F2M_GPUTHREAD_H_

#include "CL/cl.h"
#include "F2M_Hash.h"

struct F2M_Work;
class F2M_Timer;


class F2M_GPUThread
{
public:
    F2M_GPUThread(float percentage);
    ~F2M_GPUThread();

    bool GetSolutionFound()         { return mSolutionFound; }
    unsigned int GetSolution()      { return mSolution; }
    unsigned int GetHashrate()      { return mHashRate; }
    unsigned int GetHashesDone()    { return mHashesDone; }

    void SetPercentage(float percentage);
    
    void StartWork(unsigned int hashStart, unsigned int hashCount, F2M_Work* work);
    bool IsWorkDone(); 

private:
    void DoWork();

protected:
    float mPercentage;

    bool                mSolutionFound;
    unsigned int        mSolution;
    unsigned int        mHashRate;
    unsigned int        mHashesDone;

    unsigned int        mGPUThreadCount;
    unsigned int*       mOutputArea;
    unsigned int*       mPositivesArea;

    cl_platform_id      mPlatform;
    cl_device_id        mDevice;
    cl_context          mCtx;
    cl_command_queue    mQ;
    cl_program          mProgram;
    cl_kernel           mKernel;

    cl_mem              mGPUInput;
    cl_mem              mGPUScratch;
    cl_mem              mGPUOutput;

    __int64             mHashStart;
    __int64             mHashEnd;
    F2M_Work*           mWork;
    F2M_ScryptData*     mScryptData;
    cl_event            mWorkDoneEvent;

    F2M_Timer*          mTimer;
};


#endif