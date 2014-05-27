#ifndef _F2M_MINING_THREAD_MANAGER_H_
#define _F2M_MINING_THREAD_MANAGER_H_

class F2M_MinerConnection;
class F2M_WorkThread;
class F2M_GPUThread;
class F2M_Timer;
struct F2M_Work;

class F2M_MiningThreadManager
{
public:
    F2M_MiningThreadManager(int threadCount, bool useSSE, float gpuPercentage);
    ~F2M_MiningThreadManager();


    bool Update(F2M_MinerConnection* connection);
    void StartWork(F2M_Work* work);
    void StopWork(F2M_MinerConnection* connection);

    unsigned int GetHashRate()  { return mHashRate; }

protected:
    int                 mThreadCount;
    unsigned int        mHashRate;
    F2M_Timer*          mTimer;
    int                 mGPUThreadCount;
    F2M_GPUThread**     mGPUThreads;
    F2M_WorkThread**    mThreads;
    F2M_Work*           mCurrentWork;
};

#endif