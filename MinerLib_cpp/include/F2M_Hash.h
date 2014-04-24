#ifndef _F2M_HASH_H_
#define _F2M_HASH_H_

#include "F2M_HashSSE.h"

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

F2M_ScryptData* F2M_ScryptInit(F2M_Work* work);
void F2M_ScryptCleanup(F2M_ScryptData* scryptData);
bool F2M_ScryptHash(unsigned int nonce, F2M_Work* work, F2M_ScryptData* data);


#endif 