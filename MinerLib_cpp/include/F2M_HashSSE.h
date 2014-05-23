#ifndef _F2M_HASH_SSE_H_
#define _F2M_HASH_SSE_H_

#include "F2M_Platform.h"

#ifdef SSE_MINING
#include <emmintrin.h>

struct F2M_Work;
class F2M_WorkThread;


#ifdef WIN32
typedef __m128i SSEVector;
#else
struct SSEVector
{
	SSEVector() {}
	SSEVector(__m128i iv) { v = iv; }
	operator __m128i() { return v; }
	__m128i v;
};
#endif

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


struct F2M_DoubleSHADataSSE
{
    __m128i input[16];
    __m128i workBuffer[16];
    __m128i midstate[8];
    __m128i outputMask;
};

F2M_DoubleSHADataSSE* F2M_DoubleSHAInitSSE(F2M_Work* work);
void F2M_DoubleSHACleanupSSE(F2M_DoubleSHADataSSE* scryptData);
int F2M_DoubleSHAHashSSE(__m128i nonce,  F2M_Work* work, F2M_DoubleSHADataSSE* data);
void F2M_DoubleSHAHashWork_SIMD(F2M_WorkThread* thread);
#endif  // SSE_MINING

#endif // _F2M_HASH_SSE_H_