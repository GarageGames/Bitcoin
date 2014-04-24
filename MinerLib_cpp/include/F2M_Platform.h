#ifndef _F2M_PLATFORM_H_
#define _F2M_PLATFORM_H_

#ifdef WIN32
    #define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
    #include <Windows.h>
    #include <winsock2.h>
    #include <stdlib.h>
    #include <Mswsock.h>

    #define SOCKET_CLOSE    closesocket

    #define SSE_MINING
#elif defined(ANDROID)
    #include <unistd.h>
    #include <sys/socket.h>
    #include <netinet/in.h>
    #include <netinet/tcp.h>
    #include <sys/ioctl.h>
    #include <arpa/inet.h>
    #include <netdb.h>
    #define SOCKET_CLOSE    close
    #define SOCKET          int
    
    typedef long long __int64;
#endif


#endif // _F2M_PLATFORM_H_