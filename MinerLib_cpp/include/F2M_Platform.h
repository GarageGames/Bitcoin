#ifndef _F2M_PLATFORM_H_
#define _F2M_PLATFORM_H_

#ifdef WIN32
    #define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
    #include <Windows.h>
    #include <winsock2.h>
    #include <stdlib.h>
    #include <Mswsock.h>

    #define F2M_Sleep   Sleep

    #define PRE_ALIGN(x)    __declspec(align(x))
    #define POST_ALIGN(x)   

    #define SOCKET_CLOSE    closesocket

    #define SSE_MINING
    #define HAS_SIMD_IMPLEMENTATION 1
#elif defined(ANDROID)
    #include <unistd.h>
    #include <sys/socket.h>
    #include <netinet/in.h>
    #include <netinet/tcp.h>
    #include <sys/ioctl.h>
    #include <arpa/inet.h>
    #include <netdb.h>
	#include <string.h>
	#include <stdlib.h>

    #define F2M_Sleep(x)    usleep(x * 1000)
    
    #define PRE_ALIGN(x)
    #define POST_ALIGN(x)   __attribute__((aligned(x)))

    #define SOCKET_CLOSE    close
    #define SOCKET          int

    #define HAS_SIMD_IMPLEMENTATION 0
    
    typedef long long __int64;
#elif defined(LINUX)
	#include <unistd.h>
    #include <sys/socket.h>
    #include <netinet/in.h>
    #include <netinet/tcp.h>
    #include <sys/ioctl.h>
    #include <arpa/inet.h>
    #include <netdb.h>
	#include <string.h>
	#include <stdlib.h>
	#include <malloc.h>

    #define F2M_Sleep(x)    usleep(x * 1000)

    #define PRE_ALIGN(x)
    #define POST_ALIGN(x)   __attribute__((aligned(x)))

    #define SOCKET_CLOSE    close
    #define SOCKET          int

    //typedef long long __int64;

	#define SSE_MINING
    #define HAS_SIMD_IMPLEMENTATION 1

	#ifndef aligned_alloc
    extern void* _aligned_alloc_impl(size_t align, size_t size);
	#define aligned_alloc _aligned_alloc_impl
	#endif

	#define _aligned_malloc(size, align)	aligned_alloc(align, size)
	#define _aligned_free					free
#elif defined(__APPLE__)
    #include <unistd.h>
    #include <sys/socket.h>
    #include <netinet/in.h>
    #include <netinet/tcp.h>
    #include <sys/ioctl.h>
    #include <arpa/inet.h>
    #include <netdb.h>
    #include <string.h>
    #include <stdlib.h>

    #define F2M_Sleep(x)    usleep(x * 1000)

    #define PRE_ALIGN(x)
    #define POST_ALIGN(x)   __attribute__((aligned(x)))

    #define SOCKET_CLOSE    close
    #define SOCKET          int

    typedef long long __int64;

    #define SSE_MINING
    #define HAS_SIMD_IMPLEMENTATION 1

    #define _aligned_malloc(size, align) malloc(size)
    #define _aligned_free free
#endif


#endif // _F2M_PLATFORM_H_
