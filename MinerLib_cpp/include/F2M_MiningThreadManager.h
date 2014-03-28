#ifndef _F2M_MINING_THREAD_MANAGER_H_
#define _F2M_MINING_THREAD_MANAGER_H_

class F2M_MinerConnection;
class F2M_WorkThread;
struct F2M_Work;

class F2M_MiningThreadManager
{
public:
    F2M_MiningThreadManager(int threadCount, bool useSSE);
    ~F2M_MiningThreadManager();


    void Update(F2M_MinerConnection* connection);
    void StartWork(F2M_Work* work);


protected:
    int                 mThreadCount;
    F2M_WorkThread**    mThreads;
    F2M_Work*           mCurrentWork;
};

#endif