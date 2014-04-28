#include "F2M_Hash.h"
#include "F2M_Work.h"
#include "F2M_WorkThread.h"

#include <pthread.h>
#include <unistd.h>

struct PosixThreadData
{
    pthread_t       mThread;
    pthread_attr_t  mThreadAttr;
    volatile bool   mKill;
    volatile bool   mWorkDone;
    volatile bool   mWorkToDo;
};

void* HashWorkThread(void* param)
{
    F2M_WorkThread* tinfo = (F2M_WorkThread*)param;
    PosixThreadData* td = (PosixThreadData*)tinfo->mThreadData;

    while( 1 )
    {
        if( td->mKill )
            break;

        if( td->mWorkToDo )
        {
            // There is work to be done!
            switch( tinfo->mWork->currency )
            {
                case 0:     // SHA256^2
                    break;
                case 1:     // Scrypt
                case 2:                    
                    tinfo->ScryptHashes();
                    break;
                default:  
                    break;
            }

            // All done
            td->mWorkToDo = false;
            td->mWorkDone = true;
        }

        sleep(1);
    }
    
    return 0;
}

void F2M_WorkThread::InternalInit()
{
    PosixThreadData* td = new PosixThreadData;
    td->mKill = false;
    td->mWorkToDo = false;
    td->mWorkDone = true;
    pthread_attr_init(&td->mThreadAttr);
    pthread_attr_setstacksize(&td->mThreadAttr, 1024 * 1024 * 2);
    pthread_attr_setdetachstate(&td->mThreadAttr, PTHREAD_CREATE_JOINABLE);
    pthread_create(&td->mThread, &td->mThreadAttr, HashWorkThread, this);
    mThreadData = td;
}

void F2M_WorkThread::InternalDestroy()
{
    PosixThreadData* td = (PosixThreadData*)mThreadData;

    // Stop the thread
    td->mKill = true;
    pthread_join(td->mThread, 0);
    pthread_attr_destroy(&td->mThreadAttr);

    delete td;
    mThreadData = 0;
}

void F2M_WorkThread::InternalStartWork()
{
    PosixThreadData* td = (PosixThreadData*)mThreadData;
    td->mWorkDone = false;
    td->mWorkToDo = true;
}

bool F2M_WorkThread::IsWorkDone()
{
    PosixThreadData* td = (PosixThreadData*)mThreadData;
    return td->mWorkDone;
}

bool F2M_WorkThread::WantsThreadExit()
{
    PosixThreadData* td = (PosixThreadData*)mThreadData;
    return td->mKill;
}

void F2M_WorkThread::ScryptHashes_SSE()
{
#ifdef SSE_MINING
    __int64 end = mHashStart + mHashCount;
    F2M_ScryptDataSSE* scryptData = F2M_ScryptInitSSE(mWork);
    for( __int64 i = mHashStart; i < end; i += 4 )
    {
        if( WantsThreadExit() )
            break;
        
        unsigned int inonce = (unsigned int)i;
        __m128i nonce = _mm_set_epi32(inonce, inonce + 1, inonce + 2, inonce + 3);
        int success = F2M_ScryptHashSSE(nonce, mWork, scryptData);
        mHashesDone += 4;
        if( success >= 0 )
        {
            mSolution = inonce + success;
            mSolutionFound = true;
            break;
        }
    }
    F2M_ScryptCleanupSSE(scryptData);
#endif
}