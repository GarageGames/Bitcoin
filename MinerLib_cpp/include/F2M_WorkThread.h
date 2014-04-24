#ifndef _F2M_WORK_THREAD_
#define _F2M_WORK_THREAD_

struct F2M_Work;

class F2M_WorkThread
{
public:
    enum eScryptImplementation
    {
        eSI_Normal,
        eSI_SSE,
        eSI_AVX,
        eSI_Neon,
    };

    F2M_WorkThread();
    ~F2M_WorkThread();
    
    void StartWork(unsigned int hashStart, unsigned int hashCount, F2M_Work* work);

    bool IsWorkDone();
    
    void ScryptHashes();


protected:
    void InternalInit();
    void InternalDestroy();
    void InternalStartWork();

    void ScryptHashes_Normal();
    void ScryptHashes_SSE();

    bool WantsThreadExit();


public:     // Members public so they can be accessed by threads directly
    F2M_Work*       mWork;
    unsigned int    mHashStart;    
    unsigned int    mHashCount;
    unsigned int    mHashesDone;
    unsigned int    mSolution;
    bool            mSolutionFound;
    
    void*           mThreadData;

    static eScryptImplementation    sScryptImplementation;
};

#endif