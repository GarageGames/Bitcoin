#ifndef _MINER_CONNECTION_H_
#define _MINER_CONNECTION_H_

#include <time.h>
#include "F2M_Work.h"
#include "F2M_Sockets.h"

class F2M_MinerConnection
{
public:
    enum ConnectionState
    {
        Connecting,
        Connected,
        Disconnected
    };


    F2M_MinerConnection(unsigned int hashChunkSize, const char* agent, const char* platform, const char* location);
    ~F2M_MinerConnection(void);

    void Update();
    void ConnectTo(const char* host, unsigned short port = 80);
    void Disconnect();

    void SendWorkComplete(bool solutionFound, unsigned int solution, unsigned int hashesDone);

    F2M_Work* GetWork();
    ConnectionState GetState()  { return mConnectionState; }
    bool CanRead()              { return mCanRead; }
    bool CanWrite()             { return mCanWrite; }
    bool WantsStopWork()        { return mStopWork; }

private:
    void SetupSocket();
    void SendPacket(unsigned char* packetData, int packetLen);
    void SendIdentityPacket();
    void SendPing();
    void ProcessWorkCommand(char* packetData);


protected:
    ConnectionState mConnectionState;
    SOCKET mSocket;
    time_t mAliveTime;
    bool mCanRead;
    bool mCanWrite;
    bool mStopWork;

    unsigned int mHashChunkSize;
    const char* mAgent;
    const char* mPlatform;
    const char* mLocation;

    F2M_Work* mWorkBlock;
};

#endif // _MINER_CONNECTION_H_

