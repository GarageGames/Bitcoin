#ifndef _F2M_WORK_THREAD_
#define _F2M_WORK_THREAD_

struct F2M_Work;

class F2M_WorkThread
{
public:
    F2M_WorkThread(int threadIndex);
    ~F2M_WorkThread();
    
    void StartWork(unsigned int hashStart, unsigned int hashCount, F2M_Work* work);

    bool IsWorkDone();
    void SignalStop();
    
    void DoubleSHAHashes();
    void ScryptHashes();
    bool WantsThreadStop();

protected:
    void InternalInit(int threadIndex);
    void InternalDestroy();
    void InternalStartWork();

    void DoubleSHAHashes_Normal();
    void ScryptHashes_Normal();



public:     // Members public so they can be accessed by threads directly
    F2M_Work*       mWork;
    unsigned int    mHashStart;    
    unsigned int    mHashCount;
    unsigned int    mHashesDone;
    unsigned int    mSolution;
    bool            mSolutionFound;
    
    void*           mThreadData;
    int				mThreadIndex;
};

#endif
