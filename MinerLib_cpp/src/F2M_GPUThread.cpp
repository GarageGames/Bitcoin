#include "F2M_GPUThread.h"
#include "F2M_Utils.h"
#include "F2M_Work.h"
#include "F2M_Timer.h"
#include <stdlib.h>
#include <memory.h>
#include <stdio.h>


F2M_GPUThread::F2M_GPUThread(float percentage)
{
    mHashRate = 128;
    mTimer = new F2M_Timer();

    cl_int ret = clGetPlatformIDs(1, &mPlatform, 0);
    ret = clGetDeviceIDs(mPlatform, CL_DEVICE_TYPE_GPU, 1, &mDevice, 0);
    printf("clGetDeviceIDs: %d\n", ret);
    mCtx = clCreateContext(0, 1, &mDevice, 0, 0, &ret);    
    
    // Create the command queue for this device
    mQ = clCreateCommandQueue(mCtx, mDevice, 0, &ret);
            
    // Look for the precompiled code for this device
    bool saveBinary = false;
    size_t fileSize;           
    void* data = F2M_FileLoad("scrypt_cl.bin", &fileSize);
    if( data )
    {
        cl_int status;
        mProgram = clCreateProgramWithBinary(mCtx, 1, &mDevice, &fileSize, (const unsigned char**)&data, &status, &ret);        
        F2M_FileUnload(data);
    }
    else
    {
        // Load the source code
        data = F2M_FileLoad("scrypt.cl", &fileSize);

        // Compile the source
        mProgram = clCreateProgramWithSource(mCtx, 1, (const char**)&data, (const size_t*)&fileSize, &ret);        

        F2M_FileUnload(data);
        saveBinary = true;
    }
    ret = clBuildProgram(mProgram, 1, &mDevice, 0, 0, 0);
    if( ret == -11 )
    {
        size_t log_size;
        clGetProgramBuildInfo(mProgram, mDevice, CL_PROGRAM_BUILD_LOG, 0, NULL, &log_size);
        char* log = (char*)malloc(log_size);
        clGetProgramBuildInfo(mProgram, mDevice, CL_PROGRAM_BUILD_LOG, log_size, log, NULL);
        printf("%s\n", log);
        free(log);
    }

    if( saveBinary )
    {
        size_t paramSizeLen;
        ret = clGetProgramInfo(mProgram, CL_PROGRAM_BINARY_SIZES, sizeof(size_t), &fileSize, &paramSizeLen);
        void* data = malloc(fileSize);
        ret = clGetProgramInfo(mProgram, CL_PROGRAM_BINARIES, sizeof(unsigned char*), &data, &paramSizeLen);

        F2M_FileSave("scrypt_cl.bin", data, fileSize);
        free(data);
    }

    // Get the kernel function
    mKernel = clCreateKernel(mProgram, "ScryptHash", &ret);

    // Setup data input area
    mGPUInput = clCreateBuffer(mCtx, CL_MEM_READ_ONLY, 128, 0, &ret);
    ret = clSetKernelArg(mKernel, 0, sizeof(cl_mem), (void*)&mGPUInput);

    // Setup percentage based variables
    mOutputArea = 0;
    SetPercentage(percentage);
    mWorkDoneEvent = 0;
}

F2M_GPUThread::~F2M_GPUThread()
{
    clFlush(mQ);
    clFinish(mQ);

    clReleaseKernel(mKernel);
    clReleaseProgram(mProgram);
    
    clReleaseMemObject(mGPUInput);
    clReleaseMemObject(mGPUScratch);
    clReleaseMemObject(mGPUOutput);
    free(mOutputArea);
    free(mPositivesArea);

    clReleaseCommandQueue(mQ);
    clReleaseContext(mCtx);

    if( mScryptData )
        F2M_ScryptCleanup(mScryptData);

    delete mTimer;
}

void F2M_GPUThread::SetPercentage(float percentage)
{
    mPercentage = percentage / 100;
    if( mPercentage > 1 )
        mPercentage = 1;

    cl_ulong max_alloc;
    cl_int ret = clGetDeviceInfo(mDevice, CL_DEVICE_MAX_MEM_ALLOC_SIZE , sizeof(cl_ulong), (void *)&max_alloc, 0);
    mGPUThreadCount = (int)(((double)max_alloc * mPercentage) / (1024 * 64));
    mGPUThreadCount /= 128;
    mGPUThreadCount *= 128;
    if( mGPUThreadCount < 128 )
        mGPUThreadCount = 128;

    if( mOutputArea )
    {
        free(mOutputArea);
        free(mPositivesArea);
        clReleaseMemObject(mGPUScratch);
        clReleaseMemObject(mGPUOutput);
    }
    mOutputArea = (unsigned int*)malloc(mGPUThreadCount * 4);
    mPositivesArea = (unsigned int*)malloc(mGPUThreadCount * 4);

    size_t scratchSize = 64 * 1024 * mGPUThreadCount;
    mGPUScratch = clCreateBuffer(mCtx, CL_MEM_READ_WRITE, scratchSize, 0, &ret);
    mGPUOutput = clCreateBuffer(mCtx, CL_MEM_WRITE_ONLY, mGPUThreadCount * 4, 0, &ret);
        
    ret = clSetKernelArg(mKernel, 1, sizeof(cl_mem), (void*)&mGPUOutput);
    ret = clSetKernelArg(mKernel, 2, sizeof(cl_mem), (void*)&mGPUScratch);
}

void F2M_GPUThread::StartWork(unsigned int hashStart, unsigned int hashCount, F2M_Work* work)
{
    mSolutionFound = false;
    mHashStart = hashStart;
    mHashEnd = mHashStart + hashCount;
    mWork = work;
    mHashesDone = 0;

    mScryptData = F2M_ScryptInit(work);

    cl_int status = clEnqueueWriteBuffer(mQ, mGPUInput, CL_FALSE, 0, 128, work->dataFull, 0, 0, 0);

    cl_uint leTarget = work->target[7];
    status = clSetKernelArg(mKernel, 3, 4, (void*)&leTarget);

    DoWork();
}

void F2M_GPUThread::DoWork()
{
    mTimer->Start();
    size_t offset = (size_t)mHashStart;
    size_t globalItems = mGPUThreadCount;
    size_t localItems = mGPUThreadCount < 128 ? mGPUThreadCount : 128;
    cl_int status = clEnqueueNDRangeKernel(mQ, mKernel, 1, &offset, &globalItems, &localItems, 0, 0, 0);
    
    status = clEnqueueReadBuffer(mQ, mGPUOutput, CL_FALSE, 0, mGPUThreadCount * 4, mOutputArea, 0, 0, &mWorkDoneEvent);
}

bool F2M_GPUThread::IsWorkDone()
{
    if( !mWorkDoneEvent )
        return true;

    cl_int gpuStatus;
    cl_int ret = clGetEventInfo(mWorkDoneEvent, CL_EVENT_COMMAND_EXECUTION_STATUS, sizeof(cl_int), &gpuStatus, 0);
    if( gpuStatus == CL_COMPLETE )
    {
        // Copy positives
        unsigned int positive = 0;
        for( unsigned int i = 0; i < mGPUThreadCount; i++ )
        {
            if( mOutputArea[i] )
                mPositivesArea[positive++] = mOutputArea[i];
        }
        mTimer->Stop();
        mHashRate = (unsigned int)(mGPUThreadCount / mTimer->GetDuration());

        // Check for the end
        mHashesDone += mGPUThreadCount;
        mHashStart += mGPUThreadCount;
        if( mHashStart >= mHashEnd )
        {
            F2M_ScryptCleanup(mScryptData);
            mScryptData = 0;
            mWorkDoneEvent = 0;   
            return true;
        }

        // Start more work
        DoWork();

        // Check all positives
        for( unsigned int i = 0; i < positive; i++ )
        {
            if( F2M_ScryptHash(mPositivesArea[i], mWork, mScryptData) )
            {
                mSolutionFound = true;
                mSolution = mPositivesArea[i];
                mWorkDoneEvent = 0;                

                clFinish(mQ);
                return true;
            }
        }
    }

    return false;
}