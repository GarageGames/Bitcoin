#include "F2M_Platform.h"
#include "F2M_Hash.h"
#include "F2M_Work.h"
#include "F2M_WorkThread.h"

extern const SSEVector* staticHashSSE;

extern unsigned int ComputeOutputMask(F2M_Work* work);
extern void sha256_blockSSEu(SSEVector* output, const SSEVector* state, const SSEVector* input);



int F2M_DoubleSHAHashSSE(__m128i nonce,  F2M_Work* work, F2M_DoubleSHADataSSE* data)
{
    data->input[3] = nonce;
    sha256_blockSSEu(data->workBuffer, data->midstate, data->input);
    sha256_blockSSEu(data->workBuffer, staticHashSSE, data->workBuffer);


    SSEVector shifted = _mm_shuffle_epi32(data->workBuffer[7], 0x93);
    SSEVector aandbcandd = _mm_and_si128(data->workBuffer[7], shifted);
    shifted = _mm_shuffle_epi32(aandbcandd, 0x55);
    SSEVector test = _mm_and_si128(aandbcandd, shifted);
    SSEVector masked = _mm_and_si128(test, data->outputMask);

    __m128 testVal;
    _mm_store_si128((__m128i*)&testVal, masked);
    unsigned int* testPtr = (unsigned int*)&testVal;
    if( testPtr[3] == 0 )
    {
        // Full test, one of these 4 looks good
        __m128* memHash = (__m128*)data->workBuffer;
        for( int j = 0; j < 4; j++ )
        {
            for( int k = 7; k > 0; k-- )
            {
                testPtr = (unsigned int*)&memHash[k];
                unsigned int hashval = ByteReverse(testPtr[j]);
                if( hashval > work->target[k] )
                    break;
                if( hashval < work->target[k] )
                {
                    return (3 - j);
                }        
            }
        }
    }
    return -1;
}

F2M_DoubleSHADataSSE* F2M_DoubleSHAInitSSE(F2M_Work* work)
{
    F2M_DoubleSHADataSSE* data = (F2M_DoubleSHADataSSE*)_aligned_malloc(sizeof(F2M_DoubleSHADataSSE), 128);
    memset(data, 0, sizeof(F2M_DoubleSHADataSSE));

    __m128i input[16];

    for( int i = 0; i < 16; i++ )
    {
        input[i] = _mm_set1_epi32(work->dataFull[i]);
        data->input[i] = _mm_set1_epi32(work->dataFull[16 + i]);
    }

    // Compute midstate
    sha256_blockSSEu(data->midstate, staticHashSSE, input);

    // Setup workbuffer
    memset(data->workBuffer, 0, sizeof(data->workBuffer));
    data->workBuffer[9] = _mm_set1_epi32(0x80000000);
    data->workBuffer[15] = _mm_set1_epi32(0x00000100);
    
    // Setup output mask
    unsigned int outputMask = ComputeOutputMask(work);
    data->outputMask = _mm_set1_epi32(outputMask);

    return data;
}

void F2M_DoubleSHACleanupSSE(F2M_DoubleSHADataSSE* data)
{
    _aligned_free(data);
}

void F2M_DoubleSHAHashWork_SIMD(F2M_WorkThread* thread)
{
    __int64 end = thread->mHashStart + thread->mHashCount;
    F2M_DoubleSHADataSSE* shaData = F2M_DoubleSHAInitSSE(thread->mWork);
    for( __int64 i = thread->mHashStart; i < end; i += 4 )
    {
        if( thread->WantsThreadStop() )
            break;

        unsigned int inonce = (unsigned int)i;
        __m128i nonce = _mm_set_epi32(inonce, inonce + 1, inonce + 2, inonce + 3);
        int success = F2M_DoubleSHAHashSSE(nonce, thread->mWork, shaData);
        thread->mHashesDone += 4;
        if( success >= 0 )
        {
            thread->mSolution = inonce + success;
            thread->mSolutionFound = true;
            break;
        }
    }
    F2M_DoubleSHACleanupSSE(shaData);
}
