#ifndef _F2M_SOCKETS_H_
#define _F2M_SOCKETS_H_

#include "F2M_Platform.h"

enum SocketError
{
    SE_Unknown,
    SE_ConnectionAborted,
    SE_ConnectionReset,
    SE_InProgress,
    SE_WouldBlock,
};

void F2M_Socket_SetNonBlocking(SOCKET s);
SocketError F2M_Socket_GetLastError();
void F2M_Socket_SetupSockAddr(sockaddr_in& addr, unsigned long hostAddr, unsigned short port);

#endif