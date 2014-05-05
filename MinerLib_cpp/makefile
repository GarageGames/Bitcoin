CC=gcc
CFLAGS=-c -Wall -static -I include -D LINUX -D_REENTRANT -msse4.1 -g
OUTDIR=LinuxBuild
OUTDIRD=LinuxBuildD
OUT=$(OUTDIR)/libMiner.a
OUTD=$(OUTDIRD)/libMinerD.a

SOURCES=F2M_Net_posix.cpp F2M_GPUThread_none.cpp F2M_MinerConnection.cpp F2M_MiningThreadManager.cpp F2M_Scrypt.cpp F2M_ScryptSSE.cpp F2M_Sockets_posix.cpp F2M_Timer_posix.cpp 
SOURCES+=F2M_UnitTest.cpp F2M_WorkThread_posix.cpp F2M_WorkThread.cpp F2M_Linux.cpp F2M_Utils_x86.cpp
OBJECTS=$(patsubst %.cpp, $(OUTDIR)/%.o, $(SOURCES))
OBJECTSD=$(patsubst %.cpp, $(OUTDIRD)/%.o, $(SOURCES))

all: setup $(OBJECTS) $(OUT)

debug: setupd $(OBJECTSD) $(OUTD)

setup:
	mkdir -p $(OUTDIR)
	
setupd:
	mkdir -p $(OUTDIRD)

clean:
	rm $(OUT) $(OBJECTS)
	
cleand:
	rm $(OUTD) $(OBJECTSD)
	
$(OUT): $(OBJECTS)
	ar rcs $(OUT) $(OBJECTS)
	
$(OUTD): $(OBJECTSD)
	ar rcs $(OUTD) $(OBJECTSD)

$(OUTDIR)/%.o: src/%.cpp
	$(CC) $(CFLAGS) -Ofast $< -o $@

$(OUTDIRD)/%.o: src/%.cpp
	$(CC) $(CFLAGS) -g $< -o $@