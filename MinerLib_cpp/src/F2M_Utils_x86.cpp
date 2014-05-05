#include "F2M_Platform.h"
#include "F2M_Utils.h"

#ifdef WIN32
#include <intrin.h> // __cpuid
#else
#include <cpuid.h>
#endif


bool F2M_HardwareSupportsSIMD()
{
    unsigned int CPUInfo[4];
#ifdef WIN32
        __cpuid((int*)CPUInfo, 1);
#else
        __get_cpuid(1, &CPUInfo[0], &CPUInfo[1], &CPUInfo[2], &CPUInfo[3]);
#endif
    bool sse = ((CPUInfo[3] & (1 << 26)) != 0);
    return sse;
}
