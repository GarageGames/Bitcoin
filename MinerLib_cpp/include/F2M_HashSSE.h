#ifndef _F2M_HASH_SSE_H_
#define _F2M_HASH_SSE_H_

#include "F2M_Platform.h"

#ifdef SSE_MINING
#include <emmintrin.h>

struct F2M_Work;
class F2M_WorkThread;

struct F2M_ScryptDataSSE
{
    __m128i input[16];
    __m128i inputB[16];
    __m128i inputB2[16];

    __m128i pad36[16];
    __m128i pad5c[16];
    __m128i dataBuffer2[16];
    __m128i tempHash[16];
    __m128i output[8];
    __m128i outputMask;
};

F2M_ScryptDataSSE* F2M_ScryptInitSSE(F2M_Work* work);
void F2M_ScryptCleanupSSE(F2M_ScryptDataSSE* scryptData);
int F2M_ScryptHashSSE(__m128i nonce,  F2M_Work* work, F2M_ScryptDataSSE* data);

void F2M_ScryptHashWork_SIMD(F2M_WorkThread* thread);
#endif  // SSE_MINING

#endif // _F2M_HASH_SSE_H_