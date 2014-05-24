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

    if( gpuPercentage > 0 )
        mGPUThread = new F2M_GPUThread(gpuPercentage);
    else
        mGPUThread = 0;

    mCurrentWork = 0;
    mHashRate = 0;
    
    mTimer = new F2M_Timer();
}

F2M_MiningThreadManager::~F2M_MiningThreadManager()
{
    for( int i = 0; i < mThreadCount; i++ )
    {
        delete mThreads[i];
        mThreads[i] = 0;
    }
    delete[] mThreads;

    if( mGPUThread )
        delete mGPUThread;

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
        if( mGPUThread )
            threadsAllDone = mGPUThread->IsWorkDone();

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

            if( mGPUThread )
            {
                solutionFound = mGPUThread->GetSolutionFound();
                solution = mGPUThread->GetSolution();
                hashes += mGPUThread->GetHashesDone();
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

    unsigned int hashStart = work->hashStart;
    unsigned int hashCount = work->hashCount;
        
    if( mGPUThread )
    {
        // Find out how many hashes the GPU wants to do
        unsigned int gpuHashes = mGPUThread->GetHashrate() * 4;
        if( gpuHashes > hashCount )
            gpuHashes = hashCount / 2;  // GPU wants all of them, limit to half so the server will give us a bigger chunk next time

        // Give the GPU its hashes
        mGPUThread->StartWork(hashStart, gpuHashes, work);
        hashCount -= gpuHashes;
        hashStart += gpuHashes;
    }
    
    unsigned int hashesPerThread = hashCount / mThreadCount;
    for( int i = 0; i < mThreadCount; i++ )
    {
        unsigned int hashes = hashesPerThread;
        if( hashes > hashCount )
            hashes = hashCount;
        hashCount -= hashes;

        mThreads[i]->StartWork(hashStart, hashes, work);
        hashStart += hashes;
    }
}

void F2M_MiningThreadManager::StopWork(F2M_MinerConnection* conn)
{
    if( mGPUThread )
        mGPUThread->SignalStop();
    for( int i = 0; i < mThreadCount; i++ )
        mThreads[i]->SignalStop();

    while( Update(conn) )
        F2M_Sleep(10);
}
