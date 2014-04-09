#ifndef _F2M_HASH_H_
#define _F2M_HASH_H_

#include <emmintrin.h>
struct F2M_Work;

struct F2M_ScryptData
{
    unsigned int pad36[16];
    unsigned int pad5c[16];
    unsigned int dataBuffer[16];
    unsigned int dataBuffer2[16];
    unsigned int tempHash[16];
    unsigned int output[8];
    unsigned int outputMask;
};

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

F2M_ScryptData* F2M_ScryptInit(F2M_Work* work);
void F2M_ScryptCleanup(F2M_ScryptData* scryptData);
bool F2M_ScryptHash(unsigned int nonce, F2M_Work* work, F2M_ScryptData* data);


F2M_ScryptDataSSE* F2M_ScryptInitSSE(F2M_Work* work);
void F2M_ScryptCleanupSSE(F2M_ScryptDataSSE* scryptData);
int F2M_ScryptHashSSE(__m128i nonce,  F2M_Work* work, F2M_ScryptDataSSE* data);

#endif 