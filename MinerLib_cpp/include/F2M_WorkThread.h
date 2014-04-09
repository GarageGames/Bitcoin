#ifndef _F2M_WORK_THREAD_
#define _F2M_WORK_THREAD_

#ifdef WIN32
#include <Windows.h>
#endif

struct F2M_Work;

class F2M_WorkThread
{
public:
    F2M_WorkThread(bool useSSE);
    ~F2M_WorkThread();
    
    void StartWork(unsigned int hashStart, unsigned int hashCount, F2M_Work* work);

    bool IsWorkDone();

protected:
    void InternalInit();
    void InternalDestroy();
    void InternalStartWork();


public:     // Members public so they can be accessed by threads directly
    F2M_Work*       mWork;
    unsigned int    mHashStart;    
    unsigned int    mHashCount;
    unsigned int    mHashesDone;
    unsigned int    mSolution;
    bool            mSolutionFound;
    
    bool            mUseSSE;

#ifdef WIN32
    HANDLE          mKill;
    HANDLE          mDead;
    HANDLE          mWorkToDo;
    HANDLE          mWorkDone;
    HANDLE          mThreadHandle;
    DWORD           mThreadId;
#endif
};

#endif