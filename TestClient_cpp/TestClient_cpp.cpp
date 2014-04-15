// TestClient_cpp.cpp : Defines the entry point for the console application.
//

#include <tchar.h>
#include <crtdbg.h>

#include <F2M_MinerConnection.h>
#include <F2M_MiningThreadManager.h>
#include <F2M_Net.h>
#include <intrin.h> // __cpuid
#include <stdio.h>


static const int HostPort = 80;
static const char* HostAddress = "ronsTestMachine.cloudapp.net";
//static const int HostPort = 805;
//static const char* HostAddress = "127.0.0.1";

int _tmain(int argc, _TCHAR* argv[])
{   
    //_CrtSetBreakAlloc(144);

    char agentName[128];
    strcpy_s(agentName, sizeof(agentName), "C++ Test Miner");
    
    int CPUInfo[4];
        __cpuid(CPUInfo, 1);
    bool sse = ((CPUInfo[3] & (1 << 26)) != 0);
    if( sse )
        strcat_s(agentName, sizeof(agentName) - 15, " - sse");
    else
        strcat_s(agentName, sizeof(agentName) - 15, " - x86");

    SYSTEM_INFO sysinfo;
    GetSystemInfo( &sysinfo );

    int threadCount = sysinfo.dwNumberOfProcessors - 1;
    //threadCount = 1;
    F2M_MiningThreadManager* threadManager = new F2M_MiningThreadManager(threadCount, sse, 50);

    
    // Call F2M_NetInit here to initialize winsock so gethostname/gethostbyname work
    F2M_NetInit();

    char myName[128];
    char myIPAddress[32];
    gethostname(myName, sizeof(myName));
    hostent* he = gethostbyname(myName);
    if( he )
        strcpy_s(myIPAddress, sizeof(myIPAddress), inet_ntoa(*(in_addr*)he->h_addr_list[0]));
    
    unsigned int hashBlockCount = threadCount * 5000;
    F2M_MinerConnection* conn = new F2M_MinerConnection(hashBlockCount, agentName, "Windows", myIPAddress );
    conn->ConnectTo(HostAddress, HostPort);

    _CrtMemState checkpoint;
    _CrtMemCheckpoint(&checkpoint);
    while( 1 )
    {
        if( GetAsyncKeyState(VK_ESCAPE) != 0 )
            break;
        if( GetAsyncKeyState(VK_SPACE) != 0 )
        {
            _CrtMemDumpAllObjectsSince(&checkpoint);
            _CrtMemCheckpoint(&checkpoint);
        }
        

        // Update connection to server
        conn->Update();
        threadManager->Update(conn);
        if( conn->GetState() == F2M_MinerConnection::Connected )
        {
            F2M_Work* work = conn->GetWork();
            if( work )
            {
                printf("starting work, %d hashes\n", work->hashCount);
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

	return 0;
}

