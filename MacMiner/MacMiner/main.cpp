//
//  main.cpp
//  MacMiner
//
//  Created by GarageGames on 4/24/14.
//  Copyright (c) 2014 GarageGames. All rights reserved.
//

#include <F2M_MinerConnection.h>
#include <F2M_MiningThreadManager.h>
#include <F2M_Net.h>
#include <F2M_UnitTest.h>
#include <stdio.h>
#include <sys/param.h>
#include <sys/sysctl.h>

static const int HostPort = 80;
static const char* HostAddress = "ronsTestMachine.cloudapp.net";

int GetCPUCount()
{
    int mib[4];
    mib[0] = CTL_HW;
    mib[1] = HW_AVAILCPU;
    
    int numCPU = 0;
    size_t len = sizeof(numCPU);
    sysctl(mib, 2, &numCPU, &len, 0, 0);
    if( numCPU < 1 )
    {
        mib[1] = HW_NCPU;
        sysctl(mib, 2, &numCPU, &len, 0, 0);
        if( numCPU < 1 )
            numCPU = 1;
    }
    return numCPU;
}


int main(int argc, const char * argv[])
{
    bool testSuccess = F2M_Scrypt_TestStandard() | F2M_Scrypt_TestSSE();
    //bool gpuSuccess = F2M_Scrypt_TestOpenCL();
    //printf("Unit Test Success: %d,  gpuSuccess: %d\n", testSuccess, gpuSuccess);
    
    int cpus = GetCPUCount();
    int threads = cpus - 1;
    if( threads < 1 )
        threads = 1;
    
    printf("Mining with %d threads\n", threads);
    F2M_NetInit();
    F2M_MiningThreadManager* threadManager = new F2M_MiningThreadManager(threads, true, 50);
    
    F2M_MinerConnection* conn = new F2M_MinerConnection(threads * 5000, "MiningTest", "Mac", "MacNative");
    conn->ConnectTo(HostAddress, HostPort);
    
    while( 1 )
    {
        conn->Update();
        threadManager->Update(conn);
        if( conn->GetState() == F2M_MinerConnection::Connected )
        {
            if( conn->WantsStopWork() )
            {
                printf("stopping work");
                threadManager->StopWork(conn);
            }
            F2M_Work* work = conn->GetWork();
            if( work )
            {
                printf("Starting work.  HR: %d\n", threadManager->GetHashRate());
                threadManager->StartWork(work);
            }
        }
        else if( conn->GetState() == F2M_MinerConnection::Disconnected )
        {
            conn->ConnectTo(HostAddress, HostPort);
        }
        
        usleep(1000 * 10);
    }
    delete threadManager;
    delete conn;
    F2M_NetShutdown();

    return 0;
}

