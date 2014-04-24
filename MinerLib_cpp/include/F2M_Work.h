#ifndef _F2M_WORK_H_
#define _F2M_WORK_H_

#ifdef WIN32
#ifdef _DEBUG   
#include <memory.h>
#include <crtdbg.h>
    #ifndef DBG_NEW      
        #define DBG_NEW new ( _NORMAL_BLOCK , __FILE__ , __LINE__ )      
        #define new DBG_NEW   
    #endif
#endif  // _DEBUG
#endif // WIN32

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