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



struct F2M_DoubleSHAData
{
    unsigned int midstate[8];
    unsigned int workBuffer[16];
    unsigned int outputMask;
};

F2M_DoubleSHAData* F2M_DoubleSHAInit(F2M_Work* work);
void F2M_DoubleSHACleanup(F2M_DoubleSHAData* data);
bool F2M_DoubleSHAHash(unsigned int nonce, F2M_Work* work, F2M_DoubleSHAData* shaData);




inline unsigned int ByteReverse(unsigned int value)
{
    value = ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
    return (value<<16) | (value>>16);
}


#endif 