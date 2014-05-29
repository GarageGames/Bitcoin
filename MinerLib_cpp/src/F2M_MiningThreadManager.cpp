#include "F2M_MiningThreadManager.h"
#include "F2M_MinerConnection.h"
#include "F2M_WorkThread.h"
#include "F2M_GPUThread.h"
#include "F2M_Work.h"
#include "F2M_Timer.h"

#include <stdio.h>

F2M_MiningThreadManager::F2M_MiningThreadManager(int threadCount, bool useSSE, float gpuPercentage)
{
    mThreadCount = threadCount;
    mThreads = new F2M_WorkThread*[threadCount];
    for( int i = 0; i < threadCount; i++ )
        mThreads[i] = new F2M_WorkThread(i);

    mGPUThreadCount = 0;
    mGPUThreads = 0;
    if( gpuPercentage > 0 )
    {
        mGPUThreadCount = F2M_GPUThread::GetGPUCount();
        printf("setting up %d GPU threads\n", mGPUThreadCount);
        mGPUThreads = new F2M_GPUThread*[mGPUThreadCount];
        for( int i = 0; i < mGPUThreadCount; i++ )
            mGPUThreads[i] = new F2M_GPUThread(gpuPercentage, i);
    }

    mCurrentWork = 0;
    mHashRate = 0;
    
    mTimer = new F2M_Timer();
}

F2M_MiningThreadManager::~F2M_MiningThreadManager()
{
    if( mThreadCount > 0 )
    {
        for( int i = 0; i < mThreadCount; i++ )
        {
            delete mThreads[i];
            mThreads[i] = 0;
        }
        delete[] mThreads;
    }

    if( mGPUThreadCount > 0 )
    {
        for( int i = 0; i < mGPUThreadCount; i++ )
        {
            delete mGPUThreads[i];
            mGPUThreads[i] = 0;
        }
        delete[] mGPUThreads;
    }

    if( mCurrentWork )
        delete mCurrentWork;
    
    delete mTimer;
}

bool F2M_MiningThreadManager::Update(F2M_MinerConnection* connection)
{
    bool bDoingWork = false;

    if( mCurrentWork )
    {
        // Doing work, check to see if all threads done
        bool threadsAllDone = true;
        for( int i = 0; i < mGPUThreadCount; i++ )
        {
            if( !mGPUThreads[i]->IsWorkDone() )
            {
                threadsAllDone = false;
                break;
            }
        }

        if( threadsAllDone )
        {
            for( int i = 0; i < mThreadCount; i++ )
            {
                if( !mThreads[i]->IsWorkDone() )
                {
                    threadsAllDone = false;
                    break;
                }
            }
        }

        if( threadsAllDone )
        {
            // Get the result and total hashes
            unsigned int hashes = 0;
            bool solutionFound = false;
            unsigned int solution = 0;

            for( int i = 0; i < mGPUThreadCount; i++ )
            {                
                hashes += mGPUThreads[i]->GetHashesDone();
                if( mGPUThreads[i]->GetSolutionFound() )
                {
                    solutionFound = true;
                    solution = mGPUThreads[i]->GetSolution();
                }
            }

            for( int i = 0; i < mThreadCount; i++ )
            {
                hashes += mThreads[i]->mHashesDone;
                if( mThreads[i]->mSolutionFound )
                {
                    solutionFound = true;
                    solution = mThreads[i]->mSolution;
                }
            }
            
            connection->SendWorkComplete(solutionFound, ntohl(solution), hashes);            
            delete mCurrentWork;
            mCurrentWork = 0;
            
            mTimer->Stop();
            printf("Duration: %f  ", mTimer->GetDuration());
            mHashRate = (unsigned int)((double)hashes / mTimer->GetDuration());
        }
        else
            bDoingWork = true;
    }

    return bDoingWork;
}

void F2M_MiningThreadManager::StartWork(F2M_Work* work)
{
    mTimer->Start();
    
    if( mCurrentWork )
        delete mCurrentWork;

    mCurrentWork = work;

    double currentHR = 0;
    for( int i = 0; i < mGPUThreadCount; i++ )
        currentHR += mGPUThreads[i]->GetHashrate();
    for( int i = 0; i < mThreadCount; i++ )
        currentHR += mThreads[i]->GetHashrate();

    unsigned int hashStart = work->hashStart;
    unsigned int hashCount = work->hashCount;
    
    double gpu = 0;
    for( int i = 0; i < mGPUThreadCount; i++ )
    {
        double percentage = mGPUThreads[i]->GetHashrate() / currentHR;
        gpu += mGPUThreads[i]->GetHashrate();
        unsigned int hashes = (unsigned int)(percentage * work->hashCount);
        if( hashes > hashCount )
            hashes = hashCount;
        if( mThreadCount == 0 && i == (mGPUThreadCount - 1) )
            hashes = hashCount;     // Last worker gets all remainder
        hashCount -= hashes;

        mGPUThreads[i]->StartWork(hashStart, hashes, work);
        hashStart += hashes;
    }

    double cpu = 0;
    for( int i = 0; i < mThreadCount; i++ )
    {
        double percentage = mThreads[i]->GetHashrate() / currentHR;
        cpu += mThreads[i]->GetHashrate();
        unsigned int hashes = (unsigned int)(percentage * work->hashCount);

        if( hashes > hashCount )
            hashes = hashCount;
        if( i == (mThreadCount - 1) )
            hashes = hashCount;     // Last worker gets all remainder
        hashCount -= hashes;

        mThreads[i]->StartWork(hashStart, hashes, work);
        hashStart += hashes;
    }
    if( cpu < 0.05 )
        printf("");
    printf("Work Divided - CPU(%f), GPU(%f)\n", cpu, gpu);
}

void F2M_MiningThreadManager::StopWork(F2M_MinerConnection* conn)
{
    for( int i = 0; i < mGPUThreadCount; i++ )
        mGPUThreads[i]->SignalStop();
    for( int i = 0; i < mThreadCount; i++ )
        mThreads[i]->SignalStop();

    while( Update(conn) )
        F2M_Sleep(10);
}
