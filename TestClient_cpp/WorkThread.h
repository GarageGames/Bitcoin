#pragma once
class WorkThread
{
public:
    WorkThread(void);
    ~WorkThread(void);

    void StartWork(unsigned int hashStart, unsigned int hashCount, Work* work);

    bool WorkDone();


    HANDLE          mKill;
    HANDLE          mDead;
    HANDLE          mWorkToDo;
    HANDLE          mWorkDone;
    HANDLE          mThreadHandle;
    DWORD           mThreadId;

    Work*           mWork;
    unsigned int    mHashStart;    
    unsigned int    mHashCount;
    unsigned int    mHashesDone;
    unsigned int    mSolution;
    bool            mSolutionFound;
};

