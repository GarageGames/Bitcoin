#include "F2M_GPUThread.h"
#include "F2M_Utils.h"
#include "F2M_Work.h"
#include "F2M_Timer.h"
#include <stdlib.h>
#include <memory.h>
#include <stdio.h>


int F2M_GPUThread::GetGPUCount()
{
    //cl_platform_id  platform;
    //cl_uint num_devices = 0;
    //cl_int ret = clGetPlatformIDs(1, &platform, 0);
    //ret = clGetDeviceIDs(platform, CL_DEVICE_TYPE_GPU, 0, 0, &num_devices);
    //return (int)num_devices;

    return 1;
}

F2M_GPUThread::F2M_GPUThread(float percentage, int deviceNumber)
{
    mHashRateWriteIndex = 0;
    memset(mHashRates, 0, sizeof(mHashRates));
    
    mOptimized = false;
    mBestScryptHR = 0;
    mBestScryptFunction = 0;
    mBestScryptMult = 0;
    mScryptFunction = 0;
    mScryptMult = 0;

    mMaxOutputItems = 0;
    mGPURate = 1;
    mLastHashRate = 0;
    mHashRate = 5000;
    mAvgHashRate = 5000;
    mTimer = new F2M_Timer();

    cl_int ret = clGetPlatformIDs(1, &mPlatform, 0);
    cl_device_id devices[16];
    cl_uint numDevices = 0;
    ret = clGetDeviceIDs(mPlatform, CL_DEVICE_TYPE_GPU, 16, devices, &numDevices);
    mDevice = devices[deviceNumber];
    mCtx = clCreateContext(0, 1, &mDevice, 0, 0, &ret);    
    ret = clGetDeviceInfo(mDevice, CL_DEVICE_MAX_MEM_ALLOC_SIZE , sizeof(cl_ulong), (void *)&mMaxAlloc, 0);
        
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

    // Get the kernel functions
    mKernels[0] = clCreateKernel(mProgram, "ScryptHash", &ret);
    mKernels[1] = clCreateKernel(mProgram, "ScryptHash2", &ret);
    mKernels[2] = clCreateKernel(mProgram, "ScryptHash4", &ret);
    mKernels[3] = clCreateKernel(mProgram, "ScryptHash8", &ret);
    //mKernels[4] = clCreateKernel(mProgram, "ScryptHash16", &ret);

    // Setup data input area
    mGPUInput = clCreateBuffer(mCtx, CL_MEM_READ_ONLY, 128, 0, &ret);
    for( int i = 0; i < NUM_SCRYPT_FUNCTIONS; i++ )
        ret = clSetKernelArg(mKernels[i], 0, sizeof(cl_mem), (void*)&mGPUInput);

    // Setup percentage based variables
    mOutputArea = 0;
    //SetPercentage(percentage);
    mWorkDoneEvent = 0;
    mGPUOutput = 0;

    // Pick function & thread count
    Optimize();
}

F2M_GPUThread::~F2M_GPUThread()
{
    clFlush(mQ);
    clFinish(mQ);

    for( int i = 0; i < NUM_SCRYPT_FUNCTIONS; i++ )
        clReleaseKernel(mKernels[i]);
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

/*
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
        
    unsigned int maxGPURate = max_alloc / (SCRYPT_BLOCK_SIZE * mScryptBlockCount * SCRYPT_THREAD_MULT);
    if( mGPURate > maxGPURate )
        mGPURate = maxGPURate;

    mGPUThreadCount = SCRYPT_THREAD_MULT * mGPURate;

    if( mGPUThreadCount > mMaxOutputItems )
    {
        unsigned int* outArea = (unsigned int*)malloc(mGPUThreadCount * 4);
        unsigned int* posArea = (unsigned int*)malloc(mGPUThreadCount * 4);

        if( mOutputArea )
        {
            memcpy(posArea, mPositivesArea, mMaxOutputItems * 4);
            free(mOutputArea);
            free(mPositivesArea);
            clReleaseMemObject(mGPUScratch);
            clReleaseMemObject(mGPUOutput);
        }
                
        mMaxOutputItems = mGPUThreadCount;
        mOutputArea = outArea;
        mPositivesArea = posArea;

        size_t scratchSize = SCRYPT_BLOCK_SIZE * mScryptBlockCount * mGPUThreadCount;
        mGPUScratch = clCreateBuffer(mCtx, CL_MEM_READ_WRITE, scratchSize, 0, &ret);
        mGPUOutput = clCreateBuffer(mCtx, CL_MEM_WRITE_ONLY, mGPUThreadCount * 4, 0, &ret);
    }
        
    ret = clSetKernelArg(mKernels[mScryptFunction], 1, sizeof(cl_mem), (void*)&mGPUOutput);
    ret = clSetKernelArg(mKernels[mScryptFunction], 2, sizeof(cl_mem), (void*)&mGPUScratch);
}
*/

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
    status = clSetKernelArg(mKernels[mScryptFunction], 3, 4, (void*)&leTarget);

    DoWork();
}

void F2M_GPUThread::DoWork()
{
    //OptimizeStep();

    mTimer->Start();
    size_t offset = (size_t)mHashStart;
    size_t globalItems = mGPUThreadCount;
    size_t localItems = mGPUThreadCount < 128 ? mGPUThreadCount : 128;
    //cl_int status = clEnqueueNDRangeKernel(mQ, mKernels[mScryptFunction], 1, &offset, &globalItems, &localItems, 0, 0, 0);
    cl_int status = clEnqueueNDRangeKernel(mQ, mKernels[mScryptFunction], 1, &offset, &globalItems, 0, 0, 0, 0);
    
    status = clEnqueueReadBuffer(mQ, mGPUOutput, CL_TRUE, 0, mGPUThreadCount * 4, mOutputArea, 0, 0, &mWorkDoneEvent);
    clFlush(mQ);
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
        mLastDuration = mTimer->GetDuration();
        mHashRate = (unsigned int)(mGPUThreadCount / mLastDuration);        

        /*
        if( !mOptimized )
        {
            if( mHashRate > mBestScryptHR )
            {
                mBestScryptHR = mHashRate;
                mBestScryptFunction = mScryptFunction;
                mBestScryptMult = mScryptMult;
                printf("Setting Best: (%d, %d)\n", mScryptFunction, mScryptMult);
            }
        }
        */


        mHashRates[mHashRateWriteIndex++] = mHashRate;
        if( mHashRateWriteIndex >= HR_HISTORY_COUNT )
            mHashRateWriteIndex = 0;
        mAvgHashRate = 0;
        int contrib = 0;
        for( int i = 0; i < HR_HISTORY_COUNT; i++ )
        {
            if( mHashRates[i] > 0 )
            {
                mAvgHashRate += mHashRates[i];
                contrib++;
            }
        }
        if( contrib > 0 )
            mAvgHashRate /= contrib;

        /*
        if( mHashRateWriteIndex == 0 )
        {
            unsigned int hr = mAvgHashRate;            
            if( hr > mLastHashRate )
            {
                unsigned int oldRate = mGPURate;
                long diff = (long)hr - (long)mLastHashRate;
                mGPURate += (diff + 511) / 512;
                //SetPercentage(0);
                printf("setting GPU Rate: %d->%d (%d/%d)\n", oldRate, mGPURate, hr, mLastHashRate);
            }
            else if( hr < mLastHashRate)
            {
                unsigned int oldRate = mGPURate;
                
                long diff = (long)mLastHashRate - (long)hr;
                mGPURate -= (diff / 512);
                //mGPURate--;
                if( mGPURate < 1 )
                    mGPURate = 1;
                //SetPercentage(0);
                printf("setting GPU Rate: %d->%d (%d/%d)\n", oldRate, mGPURate, hr, mLastHashRate);
            }
            mLastHashRate = hr;
        }
        */

        // Check for the end
        mHashesDone += mGPUThreadCount;
        mHashStart += mGPUThreadCount;
        
        // Start more work
        if( mHashStart < mHashEnd )
            DoWork();
        
        // Check all positives
        if( positive > 0 )
        {
            printf("positives: %d\n", positive);
            for( unsigned int i = 0; i < positive; i++ )
            {
                bool success = F2M_ScryptHash(mPositivesArea[i], mWork, mScryptData);
                F2M_LogHashAttempt("GPU", mPositivesArea[i], mWork->target, mScryptData->output);
                if( success )
                {
                    mSolutionFound = true;
                    mSolution = mPositivesArea[i];
                    mWorkDoneEvent = 0;
                    F2M_ScryptCleanup(mScryptData);
                    mScryptData = 0;

                    clFinish(mQ);
                    return true;
                }
            }
        }

        if( mHashStart >= mHashEnd )
        {
            F2M_ScryptCleanup(mScryptData);
            mScryptData = 0;
            mWorkDoneEvent = 0;   
            return true;
        }
    }

    return false;
}

void F2M_GPUThread::SignalStop()
{
    mWorkDoneEvent = 0;
    if( mScryptData )
        F2M_ScryptCleanup(mScryptData);
    mScryptData = 0;
    mTimer->Stop();
}

/*
void F2M_GPUThread::SetupMemory()
{    
    clFlush(mQ);
    clFinish(mQ);
    if( mGPUOutput )
    {
        clReleaseMemObject(mGPUOutput);
        clReleaseMemObject(mGPUScratch);
    }

    mGPUThreadCount = SCRYPT_THREAD_MULT * mScryptMult;
    
    if( mGPUThreadCount > mMaxOutputItems )
    {
        // Grow only
        unsigned int* outArea = (unsigned int*)malloc(mGPUThreadCount * 4);
        unsigned int* posArea = (unsigned int*)malloc(mGPUThreadCount * 4);

        if( mOutputArea )
        {
            memcpy(posArea, mPositivesArea, mMaxOutputItems * 4);
            free(mOutputArea);
            free(mPositivesArea);
        }

        mMaxOutputItems = mGPUThreadCount;
        mOutputArea = outArea;
        mPositivesArea = posArea;
    }

    cl_int ret;
    mGPUOutput = clCreateBuffer(mCtx, CL_MEM_WRITE_ONLY, mGPUThreadCount * 4, 0, &ret);
    ret = clSetKernelArg(mKernels[mScryptFunction], 1, sizeof(cl_mem), (void*)&mGPUOutput);
    
    // Setup scratch
    size_t scratchSize = SCRYPT_BLOCK_SIZE * mScryptBlockCount * mGPUThreadCount;
    mGPUScratch = clCreateBuffer(mCtx, CL_MEM_READ_WRITE, scratchSize, 0, &ret);            
    ret = clSetKernelArg(mKernels[mScryptFunction], 2, sizeof(cl_mem), (void*)&mGPUScratch);
}


void F2M_GPUThread::OptimizeStep()
{
    if( !mOptimized )
    {
        mScryptMult++;
        printf("Optimize Step (%d, %d) - %f\n", mScryptFunction, mScryptMult, mLastDuration);

        const unsigned int counts[NUM_SCRYPT_FUNCTIONS] = {1024, 512, 256, 128, 64};
        mScryptBlockCount = counts[mScryptFunction];

        unsigned int maxMult = (mMaxAlloc / (SCRYPT_BLOCK_SIZE * mScryptBlockCount * SCRYPT_THREAD_MULT)) / 2;
        if( mScryptMult > maxMult || mLastDuration > 0.4 )
        {
            mScryptMult = 0;
            mScryptFunction++;
            if( mScryptFunction == NUM_SCRYPT_FUNCTIONS )
            {
                mOptimized = true;
                mScryptFunction = mBestScryptFunction;
                mScryptMult = mBestScryptMult;
                
                printf("Optimal Best: (%d, %d)\n", mScryptFunction, mScryptMult);
                SetupMemory();
            }
            else
                OptimizeStep();
        }
        else
        {
            SetupMemory();
        }
    }
}
*/
void F2M_GPUThread::Optimize()
{
    unsigned int counts[5] = {1024, 512, 256, 128, 64};
        
    cl_uint leTarget = 0;
    unsigned char workData[128];

    double rates[NUM_SCRYPT_FUNCTIONS];

    for( int i = 0; i < NUM_SCRYPT_FUNCTIONS; i++ )
    {
        mScryptFunction = i;
        mScryptBlockCount = counts[i];
        unsigned int halfMaxGPURate = (mMaxAlloc / (SCRYPT_BLOCK_SIZE * mScryptBlockCount * SCRYPT_THREAD_MULT)) / 4;
         
        cl_int ret = clSetKernelArg(mKernels[i], 3, 4, (void*)&leTarget);

        mGPUThreadCount = SCRYPT_THREAD_MULT * halfMaxGPURate;
        printf("Testing function %d (%d)", i, halfMaxGPURate);

        size_t outputSize = mGPUThreadCount * 4;
        unsigned int* outArea = (unsigned int*)malloc(outputSize);
        cl_mem gpuOutput = clCreateBuffer(mCtx, CL_MEM_WRITE_ONLY, outputSize, 0, &ret);
        ret = clSetKernelArg(mKernels[i], 1, sizeof(cl_mem), (void*)&gpuOutput);
        
        size_t scratchSize = SCRYPT_BLOCK_SIZE * mScryptBlockCount * mGPUThreadCount;
        cl_mem gpuScratch = clCreateBuffer(mCtx, CL_MEM_READ_WRITE, scratchSize, 0, &ret);            
        ret = clSetKernelArg(mKernels[i], 2, sizeof(cl_mem), (void*)&gpuScratch);

        double seconds = 0;
        F2M_Timer t;
        {
            t.Start();
            size_t offset = (size_t)0;
            size_t globalItems = mGPUThreadCount;
            cl_int status = clEnqueueNDRangeKernel(mQ, mKernels[i], 1, &offset, &globalItems, 0, 0, 0, 0);
            status = clEnqueueReadBuffer(mQ, gpuOutput, CL_TRUE, 0, mGPUThreadCount * 4, outArea, 0, 0, &mWorkDoneEvent);
            clFlush(mQ);
            clFinish(mQ);

            t.Stop();
            seconds = t.GetDuration();
            rates[i] = mGPUThreadCount / seconds;
        }
        printf("%f (%f)\n", rates[i], seconds);
        
        clReleaseMemObject(gpuScratch);
        clReleaseMemObject(gpuOutput);
        free(outArea);
    }

    int best = 0;
    double bestHR = rates[0];
    for( int i = 1; i < NUM_SCRYPT_FUNCTIONS; i++ )
    {
        if( rates[i] > bestHR )
        {
            bestHR = rates[i];
            best = i;
        }
    }

    printf("Using function: %d\n", best);
    mScryptFunction = best;
    mScryptBlockCount = counts[mScryptFunction];
    unsigned int halfMaxGPURate = (mMaxAlloc / (SCRYPT_BLOCK_SIZE * mScryptBlockCount * SCRYPT_THREAD_MULT)) / 4;
    mGPUThreadCount = SCRYPT_THREAD_MULT * halfMaxGPURate;

    mOutputArea = (unsigned int*)malloc(mGPUThreadCount * 4);
    mPositivesArea = (unsigned int*)malloc(mGPUThreadCount * 4);

    cl_int ret;
    mGPUOutput = clCreateBuffer(mCtx, CL_MEM_WRITE_ONLY, mGPUThreadCount * 4, 0, &ret);
    ret = clSetKernelArg(mKernels[mScryptFunction], 1, sizeof(cl_mem), (void*)&mGPUOutput);
    
    // Setup scratch
    size_t scratchSize = SCRYPT_BLOCK_SIZE * mScryptBlockCount * mGPUThreadCount;
    mGPUScratch = clCreateBuffer(mCtx, CL_MEM_READ_WRITE, scratchSize, 0, &ret);            
    ret = clSetKernelArg(mKernels[mScryptFunction], 2, sizeof(cl_mem), (void*)&mGPUScratch);
}