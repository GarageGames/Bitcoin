#ifndef _MINER_CONNECTION_H_
#define _MINER_CONNECTION_H_

#include "Work.h"

class MinerConnection
{
public:
    enum ConnectionState
    {
        Connecting,
        Connected,
        Disconnected
    };


    MinerConnection(unsigned int hashChunkSize, const char* agent, const char* platform, const char* location);
    ~MinerConnection(void);

    void Update();
    void ConnectTo(const char* host, unsigned short port = 80);
    void Disconnect();

    void SendWorkComplete(bool solutionFound, unsigned int solution, unsigned int hashesDone);

    Work* GetWork();
    ConnectionState GetState()  { return mConnectionState; }
    bool CanRead()              { return mCanRead; }
    bool CanWrite()             { return mCanWrite; }

private:
    void SetupSocket();
    void SendPacket(unsigned char* packetData, int packetLen);
    void SendIdentityPacket();
    void SendPing();
    void ProcessWorkCommand(char* packetData);


protected:
    ConnectionState mConnectionState;
    SOCKET  mSocket;
    bool mCanRead;
    bool mCanWrite;

    unsigned int mHashChunkSize;
    const char* mAgent;
    const char* mPlatform;
    const char* mLocation;

    Work* mWorkBlock;
};

#endif // _MINER_CONNECTION_H_

