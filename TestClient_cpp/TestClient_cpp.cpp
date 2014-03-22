// TestClient_cpp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <MinerConnection.h>

#include "WorkThread.h"

static const int HostPort = 80;
static const char* HostAddress = "ronsTestMachine.cloudapp.net";
//static const int HostPort = 805;
//static const char* HostAddress = "127.0.0.1";


Work* gCurrentWork = 0;
int gThreadCount = 0;
WorkThread** gThreads = 0;


void DoWork(Work* work)
{
    gCurrentWork = work;
    printf("starting work\n");

    unsigned int hashesPerThread = work->hashCount / gThreadCount;
    
    unsigned int hashStart = work->hashStart;
    unsigned int hashCount = work->hashCount;
    for( int i = 0; i < gThreadCount; i++ )
    {
        int hashes = hashesPerThread;
        if( hashes > hashCount )
            hashes = hashCount;
        hashCount -= hashes;

        gThreads[i]->StartWork(hashStart, hashes, work);
        hashStart += hashes;
    }
}

void CreateThreads()
{
    gThreads = new WorkThread*[gThreadCount];
    for( int i = 0; i < gThreadCount; i++ )
    {
        gThreads[i] = new WorkThread();
    }
}

void UpdateThreads(MinerConnection* connection)
{
    if( gCurrentWork )
    {
        // Doing work, check to see if all threads done
        bool threadsAllDone = true;
        for( int i = 0; i < gThreadCount; i++ )
        {
            if( !gThreads[i]->WorkDone() )
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

            for( int i = 0; i < gThreadCount; i++ )
            {
                hashes += gThreads[i]->mHashesDone;
                if( gThreads[i]->mSolutionFound )
                {
                    solutionFound = true;
                    solution = gThreads[i]->mSolution;
                }
            }

            printf("submitting work\n");
            if( solutionFound )
                printf("found!! %d\n", solution);
            connection->SendWorkComplete(solutionFound, solution, hashes);
            delete gCurrentWork;
            gCurrentWork = 0;
        }
    }
}

int _tmain(int argc, _TCHAR* argv[])
{
    SYSTEM_INFO sysinfo;
    GetSystemInfo( &sysinfo );

    gThreadCount = sysinfo.dwNumberOfProcessors - 1;
    unsigned int hashBlockCount = gThreadCount * 5000;
    CreateThreads();

    MinerConnection* conn = new MinerConnection(hashBlockCount, "C++ Test Miner", "Windows", "" );
    conn->ConnectTo(HostAddress, HostPort);

    while( 1 )
    {
        // Update connection to server
        conn->Update();
        UpdateThreads(conn);
        if( conn->GetState() == MinerConnection::Connected )
        {
            Work* work = conn->GetWork();
            if( work )
                DoWork(work);
        }
        else if( conn->GetState() == MinerConnection::Disconnected )
        {
            // Attempt to reconnect
            conn->ConnectTo(HostAddress, HostPort);
        }

        // Sleep
        Sleep(10);
    }

	return 0;
}

