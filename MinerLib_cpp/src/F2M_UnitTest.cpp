#include "F2M_Platform.h"
#include "F2M_UnitTest.h"
#include "F2M_Work.h"
#include "F2M_Hash.h"
#include "F2M_GPUThread.h"

static const unsigned int sData[32] = {0x02000000, 0x18cf342a, 0xa354e9f3, 0xfb4bd876, 0xf9b46596, 0xc07f2e60, 0x4b930aa9, 0x1a1ba39a, 0x639d7bbf, 0x847fe2ad, 0xe767d0f9, 0xe7d22804, 0x5f40582d, 0x6f6242db, 0xd2e066e1, 0xe4aaef45, 0xd9db9063, 0xED272A53, 0x0d1b011c, 0x0005B26B, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000280};
static const unsigned int sTarget[8] = {0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x011b0d00, 0x00000000};
static const unsigned int sNOnce = 373355;

static const unsigned int sDSHAData[32] = {0x02000000, 0x352e4fbc, 0x21019b3f, 0x486867e7, 0x4abd0d95, 0x5c3789d0, 0x7d2eb432, 0x00000000, 0x00000000, 0xf14b294e, 0xed57e176, 0xb33f4b7a, 0x3cc7cea9, 0x974e70b6, 0xcfb42386, 0x49f74a96, 0x1000d6eb, 0xac647e53, 0x53307c18, 0xd5a6e0ae, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000280};
static const unsigned int sDSHATarget[8] = {0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x7c305300, 0x00000000, 0x00000000};

bool F2M_TestAll()
{
    if( !F2M_Scrypt_TestAll() )
        return false;
    if( !F2M_DSHA256_TestAll() )
        return false;
    return true;
}

bool F2M_Scrypt_TestAll()
{
    if( !F2M_Scrypt_TestStandard() )
        return false;
    if( !F2M_Scrypt_TestSSE() )
        return false;
    if( !F2M_Scrypt_TestOpenCL() )
        return false;
    
    return true;
}

bool F2M_DSHA256_TestAll()
{
    if( !F2M_DSHA256_TestStandard() )
        return false;
    if( !F2M_DSHA256_TestSSE() )
        return false;
    if( !F2M_DSHA256_TestOpenCL() )
        return false;
    
    return true;
}

bool F2M_Scrypt_TestStandard()
{
    F2M_Work work;
    memcpy(work.dataFull, sData, sizeof(sData));
    memcpy(work.target, sTarget, sizeof(sTarget));    

    F2M_ScryptData* data = F2M_ScryptInit(&work);
    bool result = F2M_ScryptHash(sNOnce, &work, data);
    F2M_ScryptCleanup(data);

    return result;
}

bool F2M_Scrypt_TestSSE()
{
#ifdef SSE_MINING
    F2M_Work work;
    memcpy(work.dataFull, sData, sizeof(sData));
    memcpy(work.target, sTarget, sizeof(sTarget));    

    F2M_ScryptDataSSE* data = F2M_ScryptInitSSE(&work);
    __m128i nonce = _mm_set_epi32(sNOnce, sNOnce + 1, sNOnce + 2, sNOnce + 3);
    bool result = F2M_ScryptHashSSE(nonce, &work, data) >= 0;
    F2M_ScryptCleanupSSE(data);

    return result;
#else
    return false;
#endif
}

bool F2M_Scrypt_TestOpenCL()
{
    F2M_Work work;
    memcpy(work.dataFull, sData, sizeof(sData));
    memcpy(work.target, sTarget, sizeof(sTarget));

    F2M_GPUThread* gpu = new F2M_GPUThread(50, 0);
    gpu->StartWork(sNOnce, 128, &work);

    while( !gpu->IsWorkDone() )
    {
        F2M_Sleep(1000);
    }

    bool result = gpu->GetSolutionFound();
    delete gpu;

    return result;
}

bool F2M_DSHA256_TestStandard()
{
    F2M_Work work;
    memcpy(work.dataFull, sDSHAData, sizeof(sDSHAData));
    memcpy(work.target, sDSHATarget, sizeof(sDSHATarget));

    F2M_DoubleSHAData* data = F2M_DoubleSHAInit(&work);
    bool result = F2M_DoubleSHAHash(sDSHAData[19], &work, data);
    F2M_DoubleSHACleanup(data);

    return result;
}

bool F2M_DSHA256_TestSSE()
{
#ifdef SSE_MINING
    F2M_Work work;
    memcpy(work.dataFull, sDSHAData, sizeof(sDSHAData));
    memcpy(work.target, sDSHATarget, sizeof(sDSHATarget));

    F2M_DoubleSHADataSSE* data = F2M_DoubleSHAInitSSE(&work);
    __m128i nonce = _mm_set_epi32(sDSHAData[19], sDSHAData[19] + 1, sDSHAData[19] + 2, sDSHAData[19] + 3);
    bool result = F2M_DoubleSHAHashSSE(nonce, &work, data);
    F2M_DoubleSHACleanupSSE(data);

    return result;
#else
    return false;
#endif
}

bool F2M_DSHA256_TestOpenCL()
{

    return false;
}
