#include "F2M_Platform.h"
#include "F2M_Dll.h"
#include "F2M_MiningThreadManager.h"
#include "F2M_MinerConnection.h"
#include "F2M_Net.h"

DLL_MARKER void F2M_Initialize()
{
    F2M_NetInit();
}

DLL_MARKER void F2M_Shutdown()
{
    F2M_NetShutdown();
}

DLL_MARKER void* F2M_MTM_Create(int threadCount, bool useSSE, float gpuPercentage)
{
    F2M_MiningThreadManager* tm = new F2M_MiningThreadManager(threadCount, useSSE, gpuPercentage);
    return tm;
}

DLL_MARKER void F2M_MTM_Destroy(void* threadManager)
{
    F2M_MiningThreadManager* tm = (F2M_MiningThreadManager*)threadManager;
    delete tm;
}

DLL_MARKER void F2M_MTM_Update(void* threadManager, void* connection)
{
    F2M_MiningThreadManager* tm = (F2M_MiningThreadManager*)threadManager;
    tm->Update((F2M_MinerConnection*)connection);
}

DLL_MARKER void F2M_MTM_StartWork(void* threadManager, void* work)
{
    F2M_MiningThreadManager* tm = (F2M_MiningThreadManager*)threadManager;
    tm->StartWork((F2M_Work*)work);
}

DLL_MARKER unsigned int F2M_MTM_GetHashRate(void* threadManager)
{
    F2M_MiningThreadManager* tm = (F2M_MiningThreadManager*)threadManager;
    return tm->GetHashRate();
}

DLL_MARKER void* F2M_Connection_Create(const char* memberName, const char* productName, const char* platform, int initialHashes)
{
    F2M_MinerConnection* conn = new F2M_MinerConnection(initialHashes, memberName, platform, productName);
    return conn;
}

DLL_MARKER void F2M_Connection_Destroy(void* connection)
{
     F2M_MinerConnection* conn = ( F2M_MinerConnection*)connection;
     delete conn;
}

DLL_MARKER void F2M_Connection_Connect(void* connection, const char* hostAddress, const unsigned short port)
{
    F2M_MinerConnection* conn = (F2M_MinerConnection*)connection;
    conn->ConnectTo(hostAddress, port);
}

DLL_MARKER void F2M_Connection_Update(void* connection)
{
    F2M_MinerConnection* conn = (F2M_MinerConnection*)connection;
    conn->Update();
}

DLL_MARKER int F2M_Connection_GetState(void* connection)
{
    F2M_MinerConnection* conn = (F2M_MinerConnection*)connection;
    return (int)conn->GetState();
}

DLL_MARKER void* F2M_Connection_GetWork(void* connection)
{
    F2M_MinerConnection* conn = (F2M_MinerConnection*)connection;
    return conn->GetWork();
}