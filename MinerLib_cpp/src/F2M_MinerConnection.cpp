#include "F2M_MinerConnection.h"
#include "F2M_Net.h"


#include <stdio.h>

#define NETWORK_VERSION 1

F2M_MinerConnection::F2M_MinerConnection(unsigned int hashChunkSize, const char* agent, const char* platform, const char* location)
{
    mWorkBlock = 0;

    mHashChunkSize = hashChunkSize;
    mAgent = agent;
    mPlatform = platform;
    mLocation = location;

    // Starup network subsystem
    F2M_NetInit();

    // Initially set connection state to disconnected
    mConnectionState = F2M_MinerConnection::Disconnected;

    // Create the underlying socket
    SetupSocket();
}

F2M_MinerConnection::~F2M_MinerConnection(void)
{
    Disconnect();
    SOCKET_CLOSE(mSocket);

    if( mWorkBlock )
        delete mWorkBlock;
}

void F2M_MinerConnection::SetupSocket()
{
    mSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

    // Make the socket non blocking
    F2M_Socket_SetNonBlocking(mSocket);

    // Disable nagle algorithm
    int nagle = 0;
    setsockopt(mSocket, IPPROTO_TCP, TCP_NODELAY, (const char*)&nagle, sizeof(nagle));
}

void F2M_MinerConnection::Update()
{
    fd_set readSet, writeSet, exceptSet;
    FD_ZERO(&readSet);
    FD_ZERO(&writeSet);
    FD_ZERO(&exceptSet);

    FD_SET(mSocket, &readSet);
    FD_SET(mSocket, &writeSet);
    FD_SET(mSocket, &exceptSet);

    timeval waitTime;
    waitTime.tv_sec = 2;
    waitTime.tv_usec = 0;

    int result = select(FD_SETSIZE, &readSet, &writeSet, &exceptSet, &waitTime);
    if( result > 0 )
    {        
        mCanRead = (FD_ISSET(mSocket, &readSet) != 0);
        mCanWrite = (FD_ISSET(mSocket, &writeSet) != 0);

        if( mConnectionState == F2M_MinerConnection::Connecting )
        {
            if( FD_ISSET(mSocket, &writeSet) )
            {
                // Connection succeeded                
                mConnectionState = F2M_MinerConnection::Connected;
                SendIdentityPacket();
            }
            else if( FD_ISSET(mSocket, &exceptSet) )
            {
                // Connection failed
                mConnectionState = F2M_MinerConnection::Disconnected;
            }
        }
    }
    
    // Read data    
    if( mCanRead && mConnectionState == F2M_MinerConnection::Connected )
    {
        char dataBuffer[1024 * 8];
        int bytesRead = recv(mSocket, dataBuffer, sizeof(dataBuffer), 0);
        mAliveTime = time(0);
        if( bytesRead > 0 )
        {
            char* dataPtr = dataBuffer;
            while( bytesRead > 0 )
            {
                int consumed = 0;
                switch( dataPtr[0] )
                {
                    case 3:     // Work Command
                        ProcessWorkCommand(dataPtr);
                        consumed = 269;
                        break;
                    case 4:     // Stop Command
                        break;
                    case 5:     // Ping
                        SendPing();
                        consumed = 1;
                        break;
                }
                bytesRead -= consumed;
                dataPtr += consumed;
            }
        }
        else if( bytesRead == 0 )
        {
            // Connection closed
            Disconnect();
        }
        else
        {
            SocketError err = F2M_Socket_GetLastError();
            switch( err )
            {
                case SE_ConnectionAborted:
                case SE_ConnectionReset:
                    Disconnect();
                    break;
                default:
                    break;
            }
        }
    }

    if( mConnectionState == F2M_MinerConnection::Connected )
    {
        time_t now = time(0);
        unsigned int timeSinceSeen = now - mAliveTime;
        if( timeSinceSeen > 5 )
        {
            SendPing();
            mAliveTime = now;
        }
    }
}

void F2M_MinerConnection::ConnectTo(const char* hostName, unsigned short port)
{
    // Disconnect from any existing connection
    Disconnect();
    
    // Get the IP address from the host name string
    unsigned long hostAddr = inet_addr(hostName);
    if( hostAddr == INADDR_NONE )
    {
        // Not an ip address, do a DNS lookup on the host name
        hostent* host = gethostbyname(hostName);
        if( host )
        	hostAddr = *reinterpret_cast<unsigned long *>( host->h_addr_list[0]);
    }

    // Start the connect to the remote host
    if( hostAddr != INADDR_NONE )
    {
        sockaddr_in addr;
        F2M_Socket_SetupSockAddr(addr, hostAddr, port);
        
        int result = connect(mSocket, (sockaddr*)&addr, sizeof(addr));
        if( result < 0 )
        {
            SocketError err = F2M_Socket_GetLastError();
            if( err == SE_InProgress || err == SE_WouldBlock )
            {
                // Connect started
                result = 0;
            }
        }
        if( result == 0 )
        	mConnectionState = F2M_MinerConnection::Connecting;
            
    }

}

void F2M_MinerConnection::Disconnect()
{
    if( mConnectionState != F2M_MinerConnection::Disconnected )
    {
        // close the connection
        SOCKET_CLOSE(mSocket);
        
        // Reset socket to be used again
        SetupSocket();

        // Change state
        mConnectionState = F2M_MinerConnection::Disconnected;
    }
}

void F2M_MinerConnection::SendPacket(unsigned char* packetData, int packetLen)
{
    if( mCanWrite )
    {
        int sentBytes = send(mSocket, (const char*)packetData, packetLen, 0);
        if( sentBytes < 0 )
        {
        }
    }
}

void F2M_MinerConnection::SendIdentityPacket()
{
    int agentSize = strlen(mAgent) + 1;
    int platformSize = strlen(mPlatform) + 1;
    int locationSize = strlen(mLocation) + 1;
    int packetLen = 7 + agentSize + platformSize + locationSize;
    unsigned char* packet = (unsigned char*)malloc(packetLen);

    packet[0] = 1;                  // Identity Packet
    packet[1] = NETWORK_VERSION;    // Current network version
    packet[2] = 2;                  // Type = C++
   
    *(unsigned int*)&packet[3] = mHashChunkSize;

    memcpy(&packet[7], mAgent, agentSize);
    memcpy(&packet[7 + agentSize], mPlatform, platformSize);
    memcpy(&packet[7 + agentSize + platformSize], mLocation, locationSize);

    SendPacket(packet, packetLen);
    free(packet);
}

void F2M_MinerConnection::SendPing()
{
    unsigned char pingPacket = 5;
    SendPacket(&pingPacket, 1);
}

void F2M_MinerConnection::ProcessWorkCommand(char* data)
{
    F2M_Work* work = new F2M_Work();
    memcpy(work, data + 1, sizeof(F2M_Work));

    if( mWorkBlock )
        delete mWorkBlock;
    mWorkBlock = work;
}

F2M_Work* F2M_MinerConnection::GetWork()
{
    F2M_Work* ret = 0;
    if( mWorkBlock )
    {
        ret = mWorkBlock;
        mWorkBlock = 0;     // Caller owns this now
    }
    return ret;
}

void F2M_MinerConnection::SendWorkComplete(bool solutionFound, unsigned int solution, unsigned int hashesDone)
{
    unsigned char packetData[10];
    
    packetData[0] = 2;      // Work Complete
    packetData[1] = solutionFound ? 1 : 0;
    *(unsigned int*)&packetData[2] = solution;
    *(unsigned int*)&packetData[6] = hashesDone;

    SendPacket(packetData, sizeof(packetData));
}
