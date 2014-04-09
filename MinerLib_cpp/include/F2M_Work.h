#ifndef _F2M_WORK_H_
#define _F2M_WORK_H_

struct F2M_Work
{
    unsigned int    hashStart;
    unsigned int    hashCount;
    unsigned int    midstate[8];
    unsigned int    data64[16];
    unsigned int    target[8];
    unsigned int    currency;
    unsigned int    dataFull[32];
};

#endif // _F2M_WORK_H_