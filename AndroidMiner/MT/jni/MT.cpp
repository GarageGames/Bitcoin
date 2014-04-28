#include <jni.h>

#include <pthread.h>
#include <unistd.h>
#include <android/log.h>

#include <F2M_Net.h>
#include <F2M_MinerConnection.h>
#include <F2M_MiningThreadManager.h>
#include <F2M_UnitTest.h>

static const int HostPort = 80;
static const char* HostAddress = "ronsTestMachine.cloudapp.net";

pthread_t gMiningThread;
pthread_attr_t gMiningThreadAttr;
volatile bool gMiningThreadKill = true;

void* MiningThread(void* arg)
{
    bool testSuccess = F2M_TestStandard();
    __android_log_print(ANDROID_LOG_DEBUG, "TAG", "testingSuccess: %d\n", testSuccess);

    F2M_MiningThreadManager* threadManager = new F2M_MiningThreadManager(1, false, 0);

    F2M_NetInit();
    F2M_MinerConnection* conn = new F2M_MinerConnection(500, "MiningTest", "Android", "AndroidNative" );
    conn->ConnectTo(HostAddress, HostPort);

    while( 1 )
    {
        if( gMiningThreadKill )
            break;

        conn->Update();
        threadManager->Update(conn);
        if( conn->GetState() == F2M_MinerConnection::Connected )
        {
            F2M_Work* work = conn->GetWork();
            if( work )
            {
            	__android_log_print(ANDROID_LOG_DEBUG, "TAG", "Got Work");
                threadManager->StartWork(work);
            }
        }
        else if( conn->GetState() == F2M_MinerConnection::Disconnected )
        {
            // Attempt to reconnect
            conn->ConnectTo(HostAddress, HostPort);
        }

        usleep(1000 * 10);
    }
    delete threadManager;
    delete conn;
    F2M_NetShutdown();

    return 0;
}

void StartMining()
{
    if( gMiningThreadKill )
    {
        gMiningThreadKill = false;
        __android_log_print(ANDROID_LOG_WARN, "TAG", "Starting Mine");

        pthread_attr_init(&gMiningThreadAttr);
        pthread_attr_setdetachstate(&gMiningThreadAttr, PTHREAD_CREATE_JOINABLE);
        pthread_create(&gMiningThread, &gMiningThreadAttr, MiningThread, 0);
    }
}

void StopMining()
{
    if( !gMiningThreadKill )
    {
        gMiningThreadKill = true;
        __android_log_print(ANDROID_LOG_WARN, "TAG", "Stopping Mine");

        pthread_join(gMiningThread, 0);
        pthread_attr_destroy(&gMiningThreadAttr);
        __android_log_print(ANDROID_LOG_WARN, "TAG", "Mine Stopped");
    }
}

extern "C" {
	JNIEXPORT void JNICALL Java_com_example_androidminer_MainActivity_nativeStartMining(JNIEnv *, jobject)
	{
        if( gMiningThreadKill )
		    StartMining();  
        else
            StopMining();
	}
}
