#include "F2M_Sockets.h"

void F2M_Socket_SetNonBlocking(SOCKET s)
{
    u_long arg = 1;
    ioctlsocket(s, FIONBIO, &arg);
}

SocketError F2M_Socket_GetLastError()
{
    SocketError errVal = SE_Unknown;

    int err = WSAGetLastError();
    switch( err )
    {
        case WSAECONNABORTED:
            errVal = SE_ConnectionAborted;
            break;
        case WSAECONNRESET:
            errVal = SE_ConnectionReset;
            break;
        case WSAEWOULDBLOCK:
            errVal = SE_WouldBlock;
            break;
    }
    return  errVal;
}

void F2M_Socket_SetupSockAddr(sockaddr_in& addr, unsigned long hostAddr, unsigned short port)
{
    addr.sin_family = AF_INET;
    addr.sin_addr.S_un.S_addr = hostAddr;
    addr.sin_port = htons(port);
    memset(addr.sin_zero, 0, sizeof(addr.sin_zero));
}