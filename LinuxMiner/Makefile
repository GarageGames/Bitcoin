CC=gcc
CFLAGS=-c -Wall -I ../MinerLib_cpp/include -D LINUX -D_REENTRANT -msse4.1 -Ofast
LFLAGS=-Wall -L ../MinerLib_cpp/LinuxBuild/ -static
LIBS=-lMiner -lstdc++ -lrt -lpthread
OBJECTS=LinuxMiner.o
OUTDIR=LinuxBuild
OUT=LinuxMiner

all: $(OBJECTS) $(OUT)

clean:
	rm $(OUT) $(OBJECTS)
	
$(OUT): $(OBJECTS)
	$(CC) $(LFLAGS) $(OUTDIR)/$(OBJECTS) $(LIBS) -o $@

%.o: %.cpp
	$(CC) $(CFLAGS) $< -o $(OUTDIR)/$@