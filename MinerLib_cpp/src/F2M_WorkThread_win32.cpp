#include <Windows.h>
#include "F2M_WorkThread.h"
#include "F2M_Hash.h"
#include "F2M_Work.h"

void ScryptHashes(F2M_WorkThread* tinfo)
{    
    __int64 end = tinfo->mHashStart + tinfo->mHashCount;
    tinfo->mSolutionFound = false;
    
    unsigned int hashesDone = 0;

    if( tinfo->mUseSSE )
    {
        F2M_ScryptDataSSE* scryptData = F2M_ScryptInitSSE(tinfo->mWork);
        for( __int64 i = tinfo->mHashStart; i < end; i += 4 )
        {
            if( WaitForSingleObject(tinfo->mKill, 0) == WAIT_OBJECT_0 )
                break;

            unsigned int inonce = (unsigned int)i;
            __m128i nonce = _mm_set_epi32(inonce, inonce + 1, inonce + 2, inonce + 3);
            int success = F2M_ScryptHashSSE(nonce, tinfo->mWork, scryptData);
            hashesDone += 4;
            if( success >= 0 )
            {
                tinfo->mSolution = inonce + success;
                tinfo->mSolutionFound = true;
                //printf("found solution: %d(%d)\n", tinfo->mSolution, i);
                break;
            }
        }
        F2M_ScryptCleanupSSE(scryptData);
    }
    else
    {
        F2M_ScryptData* scryptData = F2M_ScryptInit(tinfo->mWork);
        for( __int64 i = tinfo->mHashStart; i < end; i++ )
        {
            if( WaitForSingleObject(tinfo->mKill, 0) == WAIT_OBJECT_0 )
                break;

            hashesDone++;
            bool success = F2M_ScryptHash((unsigned int)i, tinfo->mWork, scryptData);
            if( success )
            {
                tinfo->mSolution = (unsigned int)i;
                tinfo->mSolutionFound = true;
                break;
            }
        }
        F2M_ScryptCleanup(scryptData);
    }

    tinfo->mHashesDone = hashesDone;    
}

DWORD WINAPI HashWorkThread(void* param)
{
    F2M_WorkThread* tinfo = (F2M_WorkThread*)param;

    while( 1 )
    {
        if( WaitForSingleObject(tinfo->mKill, 0) == WAIT_OBJECT_0 )
            break;

        if( WaitForSingleObject(tinfo->mWorkToDo, 0) == WAIT_OBJECT_0 )
        {
            // There is work to be done!
            //printf("Thread(%d) starting work\n", tinfo->mThreadId);
            switch( tinfo->mWork->currency )
            {
                case 0:     // SHA256^2
                    break;
                case 1:     // Scrypt
                case 2:                    
                    ScryptHashes(tinfo);
                    break;
                default:  
                    break;
            }

            // All done
            int success = SetEvent(tinfo->mWorkDone);
            //printf("Thread(%d) done with work\n", tinfo->mThreadId);
        }

        Sleep(1);
    }

    SetEvent(tinfo->mDead);
    return 0;
}

void F2M_WorkThread::InternalInit()
{
    mKill = CreateEvent(NULL, TRUE, FALSE, NULL);
    mDead = CreateEvent(NULL, FALSE, FALSE, NULL);
    mWorkToDo = CreateEvent(NULL, FALSE, FALSE, NULL);
    mWorkDone = CreateEvent(NULL, TRUE, FALSE, NULL);
    mThreadHandle = CreateThread(NULL, 0, HashWorkThread, this, 0, &mThreadId);
}

void F2M_WorkThread::InternalDestroy()
{
    // Stop the thread
    SetEvent(mKill);
    WaitForSingleObject(mDead, INFINITE);
    CloseHandle(mKill);
    CloseHandle(mDead);
    CloseHandle(mWorkToDo);
    CloseHandle(mWorkDone);
}

void F2M_WorkThread::InternalStartWork()
{
    ResetEvent(mWorkDone);
    SetEvent(mWorkToDo);
}

bool F2M_WorkThread::IsWorkDone()
{
    return (WaitForSingleObject(mWorkDone, 0) == WAIT_OBJECT_0);
}