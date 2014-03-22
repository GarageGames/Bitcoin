#include "StdAfx.h"
#include "MinerConnection.h"

#include <Mswsock.h>
#include <stdio.h>

#define NETWORK_VERSION 1

static bool WSAInitialized = false;
static WSADATA WSAData;

MinerConnection::MinerConnection(unsigned int hashChunkSize, const char* agent, const char* platform, const char* location)
{
    mWorkBlock = 0;

    mHashChunkSize = hashChunkSize;
    mAgent = agent;
    mPlatform = platform;
    mLocation = location;

    // Starup winsock
    if( !WSAInitialized )
    {
        WSAStartup(MAKEWORD(2,2), &WSAData);
        WSAInitialized = true;
    }

    // Initially set connection state to disconnected
    mConnectionState = MinerConnection::Disconnected;

    // Create the underlying socket
    SetupSocket();
}

MinerConnection::~MinerConnection(void)
{
    Disconnect();
    closesocket(mSocket);

    if( mWorkBlock )
        delete mWorkBlock;
}

void MinerConnection::SetupSocket()
{
    mSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

    // Make the socket non blocking
    u_long arg = 1;
    ioctlsocket(mSocket, FIONBIO, &arg);

    // Disable nagle algorithm
    BOOL nagle = FALSE;
    setsockopt(mSocket, IPPROTO_TCP, TCP_NODELAY, (const char*)&nagle, sizeof(nagle));
}

void MinerConnection::Update()
{
    fd_set readSet, writeSet, exceptSet;
    FD_ZERO(&readSet);
    FD_ZERO(&writeSet);
    FD_ZERO(&exceptSet);

    FD_SET(mSocket, &readSet);
    FD_SET(mSocket, &writeSet);
    FD_SET(mSocket, &exceptSet);

    timeval waitTime;
    waitTime.tv_sec = 0;
    waitTime.tv_usec = 100;

    int result = select(0, &readSet, &writeSet, &exceptSet, &waitTime);
    if( result > 0 )
    {
        mCanRead = (FD_ISSET(mSocket, &readSet) != 0);
        mCanWrite = (FD_ISSET(mSocket, &writeSet) != 0);

        if( mConnectionState == MinerConnection::Connecting )
        {
            if( FD_ISSET(mSocket, &writeSet) )
            {
                // Connection succeeded
                mConnectionState = MinerConnection::Connected;
                SendIdentityPacket();
            }
            else if( FD_ISSET(mSocket, &exceptSet) )
            {
                // Connection failed
                mConnectionState = MinerConnection::Disconnected;
            }
        }
    }
    
    // Read data    
    if( mCanRead && mConnectionState == MinerConnection::Connected )
    {
        char dataBuffer[1024 * 8];
        int bytesRead = recv(mSocket, dataBuffer, sizeof(dataBuffer), 0);
        printf("recv (%d)\n", bytesRead);
        if( bytesRead > 0 )
        {
            char* dataPtr = dataBuffer;
            while( bytesRead > 0 )
            {
                int consumed = 0;
                switch( dataPtr[0] )
                {
                    case 3:     // Work Command
                        ProcessWorkCommand(dataBuffer);
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
            int err = WSAGetLastError();
            switch( err )
            {
                case WSAECONNABORTED:
                case WSAECONNRESET:
                    Disconnect();
                    break;
                default:
                    printf("recv failed: (%d)0x%8.8x\n", err, err);
                    break;
            }
        }
    }
}

void MinerConnection::ConnectTo(const char* hostName, unsigned short port)
{
    // Disconnect from any existing connection
    Disconnect();

    // Get the IP address from the host name string
    unsigned long hostAddr = inet_addr(hostName);
    if( hostAddr == INADDR_NONE )
    {
        // Not an ip address, do a DNS lookup on the host name
        hostent* host = gethostbyname(hostName);
        hostAddr = *reinterpret_cast<unsigned long *>( host->h_addr_list[0]);
    }

    // Start the connect to the remote host
    if( hostAddr != INADDR_NONE )
    {
        SOCKADDR_IN addr;
        addr.sin_family = AF_INET;
        addr.sin_addr.S_un.S_addr = hostAddr;
        addr.sin_port = htons(port);
        memset(addr.sin_zero, 0, sizeof(addr.sin_zero));

        int result = connect(mSocket, (SOCKADDR*)&addr, sizeof(addr));
        mConnectionState = MinerConnection::Connecting;
    }
}

void MinerConnection::Disconnect()
{
    if( mConnectionState != MinerConnection::Disconnected )
    {
        // close the connection
        closesocket(mSocket);
        
        // Reset socket to be used again
        SetupSocket();

        // Change state
        mConnectionState = MinerConnection::Disconnected;
    }
}

void MinerConnection::SendPacket(unsigned char* packetData, int packetLen)
{
    if( mCanWrite )
    {
        int sentBytes = send(mSocket, (const char*)packetData, packetLen, 0);
        printf("SendPacket (%d)\n", sentBytes);
        if( sentBytes < 0 )
        {
            int err = WSAGetLastError();
        }
    }
}

void MinerConnection::SendIdentityPacket()
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

void MinerConnection::SendPing()
{
    unsigned char pingPacket = 5;
    SendPacket(&pingPacket, 1);
}

void MinerConnection::ProcessWorkCommand(char* data)
{
    Work* work = new Work();
    memcpy(work, data + 1, sizeof(Work));

    if( mWorkBlock )
        delete mWorkBlock;
    mWorkBlock = work;
}

Work* MinerConnection::GetWork()
{
    Work* ret = 0;
    if( mWorkBlock )
    {
        ret = mWorkBlock;
        mWorkBlock = 0;     // Caller owns this now
    }
    return ret;
}

void MinerConnection::SendWorkComplete(bool solutionFound, unsigned int solution, unsigned int hashesDone)
{
    unsigned char packetData[10];
    
    packetData[0] = 2;      // Work Complete
    packetData[1] = solutionFound ? 1 : 0;
    *(unsigned int*)&packetData[2] = solution;
    *(unsigned int*)&packetData[6] = hashesDone;

    SendPacket(packetData, sizeof(packetData));
}