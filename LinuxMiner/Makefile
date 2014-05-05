CC=icpc
CFLAGS=-c -Wall -I ../MinerLib_cpp/include -D LINUX -D_REENTRANT -msse4.1 -g
LFLAGS=-Wall -g
RLIBDIRS=-L ../MinerLib_cpp/LinuxBuild/ 
DLIBDIRS=-L ../MinerLib_cpp/LinuxBuildD/
LIBS=-lstdc++ -lrt -lpthread
RLIBS=-lMiner
DLIBS=-lMinerD

OUTDIR=LinuxBuild
OUTDIRD=LinuxBuildD
OUT=LinuxMiner
OUTD=LinuxMinerD

SOURCES=LinuxMiner.cpp
OBJECTS=$(patsubst %.cpp, $(OUTDIR)/%.o, $(SOURCES))
OBJECTSD=$(patsubst %.cpp, $(OUTDIRD)/%.o, $(SOURCES))

all: setup $(OBJECTS) $(OUT)

debug: setupd $(OBJECTSD) $(OUTD)

clean:
	rm $(OUT) $(OBJECTS)
	
cleand:
	rm $(OUTD) $(OBJECTSD)
	
setup:
	mkdir -p $(OUTDIR)
	
setupd:
	mkdir -p $(OUTDIRD)
	
$(OUT): $(OBJECTS)
	$(CC) $(LFLAGS) $(RLIBDIRS) $(OBJECTS) $(RLIBS) $(LIBS) -o $@
	
$(OUTD): $(OBJECTSD)
	$(CC) $(LFLAGS) $(DLIBDIRS) $(OBJECTSD) $(DLIBS) $(LIBS) -o $@

$(OUTDIR)/%.o: %.cpp
	$(CC) $(CFLAGS) -Ofast $< -o $@
	
$(OUTDIRD)/%.o: %.cpp
	$(CC) $(CFLAGS) $< -o $@