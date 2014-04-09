#ifndef _F2M_SOCKETS_H_
#define _F2M_SOCKETS_H_

#ifdef WIN32
#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#include <Windows.h>
#include <winsock2.h>
#include <stdlib.h>
#include <Mswsock.h>
#endif

#endif