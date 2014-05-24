#include "F2M_Hash.h"
#include "F2M_Work.h"
#include "F2M_WorkThread.h"

#include <pthread.h>
#include <unistd.h>

struct PosixThreadData
{
    pthread_t       mThread;
    pthread_attr_t  mThreadAttr;
    volatile bool   mStop;
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
            switch( tinfo->mWork->algorithm )
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
            td->mStop = false;
            td->mWorkToDo = false;
            td->mWorkDone = true;
        }

        //pthread_yield();
    }
    
    return 0;
}

void F2M_WorkThread::InternalInit(int threadIndex)
{
    PosixThreadData* td = new PosixThreadData;
    td->mStop = false;
    td->mKill = false;
    td->mWorkToDo = false;
    td->mWorkDone = true;
    pthread_attr_init(&td->mThreadAttr);
    pthread_attr_setstacksize(&td->mThreadAttr, 1024 * 1024 * 2);
    pthread_attr_setdetachstate(&td->mThreadAttr, PTHREAD_CREATE_JOINABLE);
    pthread_attr_setscope(&td->mThreadAttr, PTHREAD_SCOPE_SYSTEM);
    pthread_create(&td->mThread, &td->mThreadAttr, HashWorkThread, this);
    mThreadData = td;

    //cpu_set_t cpuset;
    //CPU_ZERO(&cpuset);
    //CPU_SET(threadIndex, &cpuset);

    //pthread_setaffinity_np(td->mThread, sizeof(cpu_set_t), &cpuset);

    //sched_param params;
    //params.sched_priority = sched_get_priority_max(SCHED_FIFO);
    //pthread_setschedparam(td->mThread, SCHED_FIFO, &params);
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

bool F2M_WorkThread::WantsThreadStop()
{
    PosixThreadData* td = (PosixThreadData*)mThreadData;
    return td->mStop;
}

void F2M_WorkThread::SignalStop()
{
    PosixThreadData* td = (PosixThreadData*)mThreadData;
    td->mStop = true;
    
}