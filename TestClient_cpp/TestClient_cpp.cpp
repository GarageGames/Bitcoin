// TestClient_cpp.cpp : Defines the entry point for the console application.
//

#include <tchar.h>
#include <crtdbg.h>

#include <F2M_MinerConnection.h>
#include <F2M_MiningThreadManager.h>
#include <F2M_Net.h>
#include <F2M_UnitTest.h>

#include <intrin.h> // __cpuid
#include <stdio.h>

#ifdef MINER_DLL
#include <F2M_Dll.h>
#endif

static const int HostPort = 80;
static const char* HostAddress = "ronsTestMachine.cloudapp.net";
//static const int HostPort = 805;
//static const char* HostAddress = "127.0.0.1";

int _tmain(int argc, _TCHAR* argv[])
{   
    int CPUInfo[4];
        __cpuid(CPUInfo, 1);
    bool sse = ((CPUInfo[3] & (1 << 26)) != 0);

    SYSTEM_INFO sysinfo;
    GetSystemInfo( &sysinfo );
    int threadCount = sysinfo.dwNumberOfProcessors - 1;

#ifdef _DEBUG
    threadCount = 1;
#endif

    float gpuPercentage = 0;

#ifdef MINER_DLL
    F2M_Initialize();
    void* threadManager = F2M_MTM_Create(threadCount, sse, gpuPercentage);
    void* conn = F2M_Connection_Create("MiningTest", "C++ Dll", "Windows", threadCount * 5000);
    F2M_Connection_Connect(conn, HostAddress, HostPort);

    while( 1 )
    {
        F2M_Connection_Update(conn);
        F2M_MTM_Update(threadManager, conn);
        int state = F2M_Connection_GetState(conn);
        switch( state )
        {
            case 1: // Connected
                {
                    void* work = F2M_Connection_GetWork(conn);
                    if( work )
                    {
                        printf("starting work, %d hashes/sec\n", F2M_MTM_GetHashRate(threadManager));
                        F2M_MTM_StartWork(threadManager, work);
                    }
                }
                break;
            case 2: // Disconnected
                F2M_Connection_Connect(conn, HostAddress, HostPort);
                break;
        }
        Sleep(10);
    }
    F2M_Connection_Destroy(conn);
    F2M_MTM_Destroy(conn);
    F2M_Shutdown();
#else   // MINER_DLL
    bool testSuccess = F2M_TestAll();
    
    F2M_MiningThreadManager* threadManager = new F2M_MiningThreadManager(threadCount, sse, gpuPercentage);

    
    F2M_NetInit();        
    unsigned int hashBlockCount = threadCount * 5000;
    F2M_MinerConnection* conn = new F2M_MinerConnection(hashBlockCount, "MiningTest", "Windows", "WinC++" );
    conn->ConnectTo(HostAddress, HostPort);

    _CrtMemState checkpoint;
    _CrtMemCheckpoint(&checkpoint);
    while( 1 )
    {
        //if( GetAsyncKeyState(VK_ESCAPE) != 0 )
        //    break;
        //if( GetAsyncKeyState(VK_SPACE) != 0 )
        //{
        //    _CrtMemDumpAllObjectsSince(&checkpoint);
        //    _CrtMemCheckpoint(&checkpoint);
        //}
        

        // Update connection to server
        conn->Update();
        threadManager->Update(conn);
        if( conn->GetState() == F2M_MinerConnection::Connected )
        {
            if( conn->WantsStopWork() )
            {
                printf("stopping work\n");
                threadManager->StopWork(conn);
            }
            F2M_Work* work = conn->GetWork();
            if( work )
            {
                printf("starting work, %d hashes/sec\n", threadManager->GetHashRate());
                threadManager->StartWork(work);
            }
        }
        else if( conn->GetState() == F2M_MinerConnection::Disconnected )
        {
            // Attempt to reconnect
            conn->ConnectTo(HostAddress, HostPort);
        }

        // Sleep
        Sleep(10);
    }
    delete threadManager;
    delete conn;

    _CrtDumpMemoryLeaks();
#endif  // MINER_DLL
	return 0;
}

