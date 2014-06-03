#ifndef _F2M_GPUTHREAD_H_
#define _F2M_GPUTHREAD_H_

#ifdef __APPLE__
#include <OpenCL/cl.h>
#else
#include <CL/cl.h>
#endif

#include "F2M_Hash.h"

struct F2M_Work;
class F2M_Timer;

#define HR_HISTORY_COUNT        10
#define NUM_SCRYPT_FUNCTIONS    4
#define SCRYPT_BLOCK_SIZE       128
#define SCRYPT_THREAD_MULT      128

class F2M_GPUThread
{
public:
    F2M_GPUThread(float percentage, int deviceNumber);
    ~F2M_GPUThread();

    bool GetSolutionFound()         { return mSolutionFound; }
    unsigned int GetSolution()      { return mSolution; }
    unsigned int GetHashrate()      { return mAvgHashRate; }
    unsigned int GetHashesDone()    { return mHashesDone; }

    void Optimize();
    void SetPercentage(float percentage);
    
    void StartWork(unsigned int hashStart, unsigned int hashCount, F2M_Work* work);
    bool IsWorkDone();
    void SignalStop();

    static int GetGPUCount();

private:
    void DoWork();
    void OptimizeStep();
    void SetupMemory();

protected:
    float mPercentage;

    bool                mSolutionFound;
    unsigned int        mSolution;
    unsigned int        mHashRate;
    unsigned int        mLastHashRate;
    unsigned int        mGPURate;
    unsigned int        mHashesDone;

    unsigned int        mHashRateWriteIndex;
    unsigned int        mHashRates[HR_HISTORY_COUNT];
    unsigned int        mAvgHashRate;

    unsigned int        mGPUThreadCount;
    unsigned int        mMaxOutputItems;
    unsigned int*       mOutputArea;
    unsigned int*       mPositivesArea;    

    unsigned int        mScryptFunction;
    unsigned int        mScryptMult;
    unsigned int        mScryptBlockCount;

    bool                mOptimized;
    unsigned int        mBestScryptHR;
    unsigned int        mBestScryptFunction;
    unsigned int        mBestScryptMult;    
    double              mLastDuration;

    cl_platform_id      mPlatform;
    cl_device_id        mDevice;
    cl_context          mCtx;
    cl_command_queue    mQ;
    cl_program          mProgram;
    cl_ulong            mMaxAlloc;

    cl_kernel           mKernels[NUM_SCRYPT_FUNCTIONS];

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