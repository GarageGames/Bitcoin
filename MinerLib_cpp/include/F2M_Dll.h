#ifndef _F2M_DLL_H_
#define _F2M_DLL_H_

#ifdef _DLIB
#define DLL_MARKER  _declspec(dllexport)
#else
#define DLL_MARKER  _declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

DLL_MARKER void         F2M_Initialize();
DLL_MARKER void         F2M_Shutdown();

DLL_MARKER void*        F2M_MTM_Create(int threadCount, bool useSSE, float gpuPercentage);
DLL_MARKER void         F2M_MTM_Destroy(void* threadManager);
DLL_MARKER void         F2M_MTM_Update(void* threadManager, void* connection);
DLL_MARKER void         F2M_MTM_StartWork(void* threadManager, void* work);
DLL_MARKER unsigned int F2M_MTM_GetHashRate(void* threadManager);

DLL_MARKER void*        F2M_Connection_Create(const char* memberName, const char* productName, const char* platform, int initialHashes = 5000);
DLL_MARKER void         F2M_Connection_Destroy(void* connection);
DLL_MARKER void         F2M_Connection_Connect(void* connection, const char* hostAddress, const unsigned short port);
DLL_MARKER void         F2M_Connection_Update(void* connection);
DLL_MARKER int          F2M_Connection_GetState(void* connection);
DLL_MARKER void*        F2M_Connection_GetWork(void* connection);

#ifdef __cplusplus
}
#endif

#endif  // _F2M_DLL_H_