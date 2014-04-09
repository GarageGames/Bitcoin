#include "F2M_Sockets.h"

static bool WSAInitialized = false;
static WSADATA WSAData;

void F2M_NetInit()
{
    if( !WSAInitialized )
    {
        WSAStartup(MAKEWORD(2,2), &WSAData);
        WSAInitialized = true;
    }
}

void F2M_NetShutdown()
{
    if( WSAInitialized )
    {
        WSACleanup();
        WSAInitialized = false;
    }
}