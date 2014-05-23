#include "F2M_GPUThread.h"
#include "F2M_Utils.h"
#include "F2M_Work.h"
#include "F2M_Timer.h"


F2M_GPUThread::F2M_GPUThread(float percentage)
{
    mHashRate = 0;
    mHashesDone = 0;
    mSolutionFound = false;
}

F2M_GPUThread::~F2M_GPUThread()
{
}

void F2M_GPUThread::SetPercentage(float percentage)
{
}

void F2M_GPUThread::StartWork(unsigned int hashStart, unsigned int hashCount, F2M_Work* work)
{
}

void F2M_GPUThread::DoWork()
{
}

bool F2M_GPUThread::IsWorkDone()
{
    return true;
}

void F2M_GPUThread::SignalStop()
{
}