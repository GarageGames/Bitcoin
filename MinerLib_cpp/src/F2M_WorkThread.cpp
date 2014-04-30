#include "F2M_WorkThread.h"
#include "F2M_Work.h"
#include "F2M_Hash.h"

F2M_WorkThread::eScryptImplementation F2M_WorkThread::sScryptImplementation = F2M_WorkThread::eSI_Normal;

F2M_WorkThread::F2M_WorkThread(int threadIndex)
{
    mWork = new F2M_Work;
    mHashCount = 0;

    InternalInit(threadIndex);
}

F2M_WorkThread::~F2M_WorkThread(void)
{
    InternalDestroy();
    delete mWork;
}

void F2M_WorkThread::StartWork(unsigned int hashStart, unsigned int hashCount, F2M_Work* work)
{  
    mWork[0] = *work;
   
    mHashStart = hashStart;
    mHashCount = hashCount;

    mHashesDone = 0;
    mSolutionFound = false;

    InternalStartWork();
}

void F2M_WorkThread::ScryptHashes()
{
    mHashesDone = 0;
    mSolutionFound = false;

    switch( sScryptImplementation )
    {
        case eSI_Normal:
            ScryptHashes_Normal();
            break;
        case eSI_SSE:
            ScryptHashes_SSE();
            break;
        case eSI_AVX:
            break;
        case eSI_Neon:
            break;
    }
}

void F2M_WorkThread::ScryptHashes_Normal()
{
    __int64 end = mHashStart + mHashCount;
    F2M_ScryptData* scryptData = F2M_ScryptInit(mWork);
    for( __int64 i = mHashStart; i < end; i++ )
    {
        if( WantsThreadExit() )
            break;

        mHashesDone++;
        bool success = F2M_ScryptHash((unsigned int)i, mWork, scryptData);
        if( success )
        {
            mSolution = (unsigned int)i;
            mSolutionFound = true;
            break;
        }
    }
    F2M_ScryptCleanup(scryptData);
}
