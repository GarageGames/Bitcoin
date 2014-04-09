#include "F2M_WorkThread.h"
#include "F2M_Work.h"

F2M_WorkThread::F2M_WorkThread(bool sse)
{
    mWork = new F2M_Work();
    mHashCount = 0;
    mUseSSE = sse;

    InternalInit();
}

F2M_WorkThread::~F2M_WorkThread(void)
{
    InternalDestroy();
}

void F2M_WorkThread::StartWork(unsigned int hashStart, unsigned int hashCount, F2M_Work* work)
{  
    mWork[0] = *work;
   
    mHashStart = hashStart;
    mHashCount = hashCount;

    mHashesDone = 0;
    mSolutionFound = false;

    InternalStartWork();
}
