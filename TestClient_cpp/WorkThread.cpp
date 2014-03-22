#include "StdAfx.h"

#include <MinerConnection.h>
#include <Hash.h>

#include "WorkThread.h"

void ScryptHashes(WorkThread* tinfo)
{
    ScryptData* scryptData = ScryptInit(tinfo->mWork);

    tinfo->mSolutionFound = false;
    unsigned int hashesDone = 0;
    __int64 end = tinfo->mHashStart + tinfo->mHashCount;
    for( __int64 i = tinfo->mHashStart; i < end; i++ )
    {
        if( WaitForSingleObject(tinfo->mKill, 0) == WAIT_OBJECT_0 )
            break;

        hashesDone++;
        bool success = ScryptHash(i, tinfo->mWork, scryptData);
        if( success )
        {
            tinfo->mSolution = i;
            tinfo->mSolutionFound = true;
            break;
        }
    }

    delete scryptData;

    tinfo->mHashesDone = hashesDone;
}

DWORD WINAPI HashWorkThread(void* param)
{
    WorkThread* tinfo = (WorkThread*)param;

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


WorkThread::WorkThread(void)
{
    mWork = new Work();
    mHashCount = 0;

    mKill = CreateEvent(NULL, FALSE, FALSE, NULL);
    mDead = CreateEvent(NULL, FALSE, FALSE, NULL);
    mWorkToDo = CreateEvent(NULL, FALSE, FALSE, NULL);
    mWorkDone = CreateEvent(NULL, TRUE, FALSE, NULL);
    mThreadHandle = CreateThread(NULL, 0, HashWorkThread, this, 0, &mThreadId);
}

WorkThread::~WorkThread(void)
{
}

void WorkThread::StartWork(unsigned int hashStart, unsigned int hashCount, Work* work)
{  
    mWork[0] = *work;
   
    mHashStart = hashStart;
    mHashCount = hashCount;

    mHashesDone = 0;
    mSolutionFound = false;

    ResetEvent(mWorkDone);
    SetEvent(mWorkToDo);
}

bool WorkThread::WorkDone()
{
    return (WaitForSingleObject(mWorkDone, 0) == WAIT_OBJECT_0);
}