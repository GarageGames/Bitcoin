#ifndef _HASH_H_
#define _HASH_H_


struct ScryptData
{
    unsigned int pad36[16];
    unsigned int pad5c[16];
    unsigned int dataBuffer[16];
    unsigned int dataBuffer2[16];
    unsigned int tempHash[16];
    unsigned int output[8];
};

ScryptData* ScryptInit(Work* work);
bool ScryptHash(unsigned int nonce, Work* work, ScryptData* data);

#endif 