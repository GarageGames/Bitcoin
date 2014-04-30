#include "F2M_Platform.h"

void* _aligned_alloc_impl(size_t align, size_t size)
{
	void* mem = 0;
	posix_memalign(&mem, align, size);
	return mem;
}
