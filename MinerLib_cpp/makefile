CC=gcc
CFLAGS=-c -Wall -static -I include -D LINUX -D_REENTRANT -msse4.1 -g
OUTDIR=LinuxBuild
OUT=$(OUTDIR)/libMiner.a

SOURCES=F2M_Net_posix.cpp F2M_GPUThread_none.cpp F2M_MinerConnection.cpp F2M_MiningThreadManager.cpp F2M_Scrypt.cpp F2M_ScryptSSE.cpp F2M_Sockets_posix.cpp F2M_Timer_posix.cpp 
SOURCES+=F2M_UnitTest.cpp F2M_WorkThread_posix.cpp F2M_WorkThread.cpp F2M_Linux.cpp
OBJECTS=$(patsubst %.cpp, $(OUTDIR)/%.o, $(SOURCES))

all: setup $(OBJECTS) $(OUT)

setup:
	mkdir -p LinuxBuild

clean:
	rm $(OUT) $(OBJECTS)
	
$(OUT): $(OBJECTS)
	ar rcs $(OUT) $(OBJECTS)

$(OUTDIR)/%.o: src/%.cpp
	$(CC) $(CFLAGS) $< -o $@
