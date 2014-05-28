#include "F2M_WorkThread.h"
#include "F2M_Work.h"
#include "F2M_Hash.h"
#include "F2M_Utils.h"
#include "F2M_Timer.h"

F2M_WorkThread::F2M_WorkThread(int threadIndex)
{
    mTimer = new F2M_Timer();
    mWork = new F2M_Work;
    mHashCount = 0;
    mThreadIndex = threadIndex;
    mHashrate = 5000;

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
    
    mTimer->Start();
    InternalStartWork();
}

void F2M_WorkThread::WorkDone()
{
    mTimer->Stop();
    mHashrate = (unsigned int)(mHashesDone / mTimer->GetDuration());
    if( mHashrate < 256 )
        mHashrate = 256;
}

void F2M_WorkThread::DoubleSHAHashes()
{
    mHashesDone = 0;
    mSolutionFound = false;

    if( HAS_SIMD_IMPLEMENTATION && F2M_HardwareSupportsSIMD() ) 
        F2M_DoubleSHAHashWork_SIMD(this);
    else
        DoubleSHAHashes_Normal();
}

void F2M_WorkThread::DoubleSHAHashes_Normal()
{
    __int64 end = mHashStart + mHashCount;

    F2M_DoubleSHAData* shaData = F2M_DoubleSHAInit(mWork);
    for( __int64 i = mHashStart; i < end; i++ )
    {
        if( WantsThreadStop() )
            break;

        mHashesDone++;
        bool success = F2M_DoubleSHAHash((unsigned int)i, mWork, shaData);
        if( success )
        {
            mSolution = (unsigned int)i;
            mSolutionFound = true;
            break;
        }
    }
    F2M_DoubleSHACleanup(shaData);
}

void F2M_WorkThread::ScryptHashes()
{
    mHashesDone = 0;
    mSolutionFound = false;

    if( HAS_SIMD_IMPLEMENTATION && F2M_HardwareSupportsSIMD() ) 
        F2M_ScryptHashWork_SIMD(this);
    else
        ScryptHashes_Normal();
}

void F2M_WorkThread::ScryptHashes_Normal()
{
    __int64 end = mHashStart + mHashCount;
    F2M_ScryptData* scryptData = F2M_ScryptInit(mWork);
    for( __int64 i = mHashStart; i < end; i++ )
    {
        if( WantsThreadStop() )
            break;

        mHashesDone++;
        bool success = F2M_ScryptHash((unsigned int)i, mWork, scryptData);
        if( success )
        {
            F2M_LogHashAttempt("CPU", i, mWork->target, scryptData->output);
            mSolution = (unsigned int)i;
            mSolutionFound = true;
            break;
        }
    }
    F2M_ScryptCleanup(scryptData);
}
