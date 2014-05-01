#include "F2M_Platform.h"
#include "F2M_Utils.h"

#include <intrin.h> // __cpuid


bool F2M_HardwareSupportsSIMD()
{
    int CPUInfo[4];
        __cpuid(CPUInfo, 1);
    bool sse = ((CPUInfo[3] & (1 << 26)) != 0);
    return sse;
}