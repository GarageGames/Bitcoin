#include "F2M_MiningThreadManager.h"
#include "F2M_MinerConnection.h"
#include "F2M_WorkThread.h"
#include "F2M_Work.h"

F2M_MiningThreadManager::F2M_MiningThreadManager(int threadCount, bool useSSE)
{
    mThreadCount = threadCount;
    mThreads = new F2M_WorkThread*[threadCount];
    for( int i = 0; i < threadCount; i++ )
        mThreads[i] = new F2M_WorkThread(useSSE);

    mCurrentWork = 0;
}

F2M_MiningThreadManager::~F2M_MiningThreadManager()
{
    if( mCurrentWork )
        delete mCurrentWork;
}

void F2M_MiningThreadManager::Update(F2M_MinerConnection* connection)
{
    if( mCurrentWork )
    {
        // Doing work, check to see if all threads done
        bool threadsAllDone = true;
        for( int i = 0; i < mThreadCount; i++ )
        {
            if( !mThreads[i]->IsWorkDone() )
            {
                threadsAllDone = false;
                break;
            }
        }

        if( threadsAllDone )
        {
            // Get the result and total hashes
            unsigned int hashes = 0;
            bool solutionFound = false;
            unsigned int solution = 0;

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
        }
    }
}

void F2M_MiningThreadManager::StartWork(F2M_Work* work)
{
    if( mCurrentWork )
        delete mCurrentWork;

    mCurrentWork = work;

    unsigned int hashesPerThread = work->hashCount / mThreadCount;
    
    unsigned int hashStart = work->hashStart;
    unsigned int hashCount = work->hashCount;
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