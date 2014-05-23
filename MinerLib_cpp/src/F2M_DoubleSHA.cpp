#include "F2M_Platform.h"
#include "F2M_Hash.h"
#include "F2M_Work.h"

extern unsigned int staticHash[];
extern void sha256_block(unsigned int* output, unsigned int* state, const unsigned int* input);
extern unsigned int ComputeOutputMask(F2M_Work* work);

F2M_DoubleSHAData* F2M_DoubleSHAInit(F2M_Work* work)
{
    F2M_DoubleSHAData* data = new F2M_DoubleSHAData;

    sha256_block(data->midstate, staticHash, work->dataFull);
    
    memset(data->workBuffer, 0, sizeof(data->workBuffer));
    data->workBuffer[8] = 0x80000000;
    data->workBuffer[15] = 0x00000100;
    
    data->outputMask = ComputeOutputMask(work);

    return data;
}

void F2M_DoubleSHACleanup(F2M_DoubleSHAData* data)
{
    delete data;
}

bool F2M_DoubleSHAHash(unsigned int nonce, F2M_Work* work, F2M_DoubleSHAData* shaData)
{
    work->dataFull[19] = nonce;
    sha256_block(shaData->workBuffer, shaData->midstate, &work->dataFull[16]);
    sha256_block(shaData->workBuffer, staticHash, shaData->workBuffer);

    if( (shaData->workBuffer[7] & shaData->outputMask) == 0 )
    {
        for( int i = 7; i > 0; i-- )
        {
            unsigned int outputVal = ByteReverse(shaData->workBuffer[i]);
            if( outputVal > work->target[i] )
                break;
            if( outputVal < work->target[i] )
            {
                // Found solution
                return true;
            }
        }
    }

    return false;
}