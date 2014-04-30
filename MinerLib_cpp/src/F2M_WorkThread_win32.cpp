#include "F2M_Hash.h"
#include "F2M_Work.h"
#include "F2M_WorkThread.h"

struct WinThreadData
{
    HANDLE          mKill;
    HANDLE          mDead;
    HANDLE          mWorkToDo;
    HANDLE          mWorkDone;
    HANDLE          mThreadHandle;
    DWORD           mThreadId;
};

DWORD WINAPI HashWorkThread(void* param)
{
    F2M_WorkThread* tinfo = (F2M_WorkThread*)param;
    WinThreadData* td = (WinThreadData*)tinfo->mThreadData;

    while( 1 )
    {
        if( WaitForSingleObject(td->mKill, 0) == WAIT_OBJECT_0 )
            break;

        if( WaitForSingleObject(td->mWorkToDo, 0) == WAIT_OBJECT_0 )
        {
            // There is work to be done!
            //printf("Thread(%d) starting work\n", tinfo->mThreadId);
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
            int success = SetEvent(td->mWorkDone);
            //printf("Thread(%d) done with work\n", tinfo->mThreadId);
        }

        Sleep(1);
    }

    SetEvent(td->mDead);
    return 0;
}

void F2M_WorkThread::InternalInit(int threadIndex)
{
    WinThreadData* td = new WinThreadData;
    td->mKill = CreateEvent(NULL, TRUE, FALSE, NULL);
    td->mDead = CreateEvent(NULL, FALSE, FALSE, NULL);
    td->mWorkToDo = CreateEvent(NULL, FALSE, FALSE, NULL);
    td->mWorkDone = CreateEvent(NULL, TRUE, FALSE, NULL);
    td->mThreadHandle = CreateThread(NULL, 0, HashWorkThread, this, 0, &td->mThreadId);
    mThreadData = td;
}

void F2M_WorkThread::InternalDestroy()
{
    WinThreadData* td = (WinThreadData*)mThreadData;

    // Stop the thread
    SetEvent(td->mKill);
    WaitForSingleObject(td->mDead, INFINITE);
    CloseHandle(td->mKill);
    CloseHandle(td->mDead);
    CloseHandle(td->mWorkToDo);
    CloseHandle(td->mWorkDone);

    delete td;
    mThreadData = 0;
}

void F2M_WorkThread::InternalStartWork()
{
    WinThreadData* td = (WinThreadData*)mThreadData;
    ResetEvent(td->mWorkDone);
    SetEvent(td->mWorkToDo);
}

bool F2M_WorkThread::IsWorkDone()
{
    WinThreadData* td = (WinThreadData*)mThreadData;
    return (WaitForSingleObject(td->mWorkDone, 0) == WAIT_OBJECT_0);
}

bool F2M_WorkThread::WantsThreadExit()
{
    WinThreadData* td = (WinThreadData*)mThreadData;
    return (WaitForSingleObject(td->mKill, 0) == WAIT_OBJECT_0);
}

void F2M_WorkThread::ScryptHashes_SSE()
{
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
}