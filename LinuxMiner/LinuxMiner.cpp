#include <F2M_MinerConnection.h>
#include <F2M_MiningThreadManager.h>
#include <F2M_UnitTest.h>
#include <F2M_Platform.h>

#include <stdio.h>
#include <pthread.h>
#include <unistd.h>

static const char* HostAddress = "ronsTestMachine.cloudapp.net";

#include <F2M_Timer.h>
int main(int argc,char** argv)
{
	int numCPUs = sysconf( _SC_NPROCESSORS_ONLN );
	int threadCount = numCPUs - 1;
	if( threadCount < 1 )
		threadCount = 1;

	//threadCount = 2;
	printf("CPUS: %d, Threads: %d\n", numCPUs, threadCount);
	if( numCPUs > threadCount )
	{
		cpu_set_t cpuset;
		CPU_ZERO(&cpuset);

		pthread_getaffinity_np(pthread_self(), sizeof(cpu_set_t), &cpuset);
		for( int i = 0; i < CPU_SETSIZE; i++ )
		{
			if( CPU_ISSET(i, &cpuset))
			{
				printf("CurrentAffinity: %d\n", i);
				break;
			}
		}

		CPU_ZERO(&cpuset);
		CPU_SET(7, &cpuset);

		pthread_setaffinity_np(pthread_self(), sizeof(cpu_set_t), &cpuset);

		CPU_ZERO(&cpuset);

		pthread_getaffinity_np(pthread_self(), sizeof(cpu_set_t), &cpuset);
		for( int i = 0; i < CPU_SETSIZE; i++ )
		{
			if( CPU_ISSET(i, &cpuset))
			{
				printf("CurrentAffinity: %d\n", i);
				break;
			}
		}
	}


	bool testNormal = F2M_TestStandard();
	bool testSSE = F2M_TestSSE();
	bool testOpenCL = F2M_TestOpenCL();
	printf("UnitTests: normal(%d), sse(%d), openCL(%d)\n", testNormal, testSSE, testOpenCL);


	F2M_MiningThreadManager* mtm = new F2M_MiningThreadManager(threadCount, false, 0);
	F2M_MinerConnection* conn = new F2M_MinerConnection(5000 * threadCount, "MiningTest", "Linux", "LinuxMiner");
	conn->ConnectTo(HostAddress);

	while( 1 )
	{
		conn->Update();
		mtm->Update(conn);
		if( conn->GetState() == F2M_MinerConnection::Connected)
		{
			F2M_Work* work = conn->GetWork();
			if( work )
			{
				printf("starting work.  HR: %d\n", mtm->GetHashRate());
				mtm->StartWork(work);
			}
		}
		else if( conn->GetState() == F2M_MinerConnection::Disconnected )
		{
			conn->ConnectTo(HostAddress);
		}

		usleep(10 * 1000);
	}



}
