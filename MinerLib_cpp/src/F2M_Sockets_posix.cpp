#include "F2M_Sockets.h"
#include <fcntl.h>
#include <errno.h>

void F2M_Socket_SetNonBlocking(SOCKET s)
{
    int flags;
    if (-1 == (flags = fcntl(s, F_GETFL, 0)))
        flags = 0;
    fcntl(s, F_SETFL, flags | O_NONBLOCK);
}

SocketError F2M_Socket_GetLastError()
{
    SocketError errVal = SE_Unknown;

    if( errno == ENOTCONN )
        errVal = SE_ConnectionReset;
    if( errno == EINPROGRESS )
        errVal = SE_InProgress;
    
    return  errVal;
}

void F2M_Socket_SetupSockAddr(sockaddr_in& addr, unsigned long hostAddr, unsigned short port)
{
    addr.sin_family = AF_INET;
    addr.sin_addr.s_addr = hostAddr;
    addr.sin_port = htons(port);
    memset(addr.sin_zero, 0, sizeof(addr.sin_zero));
}