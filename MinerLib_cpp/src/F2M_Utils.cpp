#include "F2M_Platform.h"
#include "F2M_Utils.h"
#include "F2M_Hash.h"
#include <stdio.h>
#include <stdlib.h>


static F2M_FileLoadFunction sFileLoadFunction = 0;
static F2M_FileUnloadFunction sFileUnloadFunction = 0;
static F2M_FileSaveFunction sFileSaveFunction = 0;

void F2M_SetFileFunctions(F2M_FileLoadFunction loadFunction, F2M_FileUnloadFunction unloadFunction, F2M_FileSaveFunction saveFunction)
{
    sFileLoadFunction = loadFunction;
    sFileUnloadFunction = unloadFunction;
    sFileSaveFunction = saveFunction;
}

void* F2M_FileLoad(const char* fileName, size_t* outFileSize)
{
    if( sFileLoadFunction )
    {
        return sFileLoadFunction(fileName, outFileSize);
    }
    else
    {
        FILE* file = fopen(fileName, "rb");
        if( !file )
        {
            fprintf(stderr, "failed to open file: %s\n", fileName);
            return 0;
        }

        fseek(file, 0, SEEK_END);
        long len = ftell(file);
        fseek(file, 0, SEEK_SET);

        void* data = malloc(len);
        fread(data, 1, len, file);
        fclose(file);
        
        if( outFileSize )
            *outFileSize = (size_t)len;

        return data;
    }
}

void F2M_FileUnload(void* fileData)
{
    if( sFileUnloadFunction )
        sFileUnloadFunction(fileData);
    else
        free(fileData);
}

void F2M_FileSave(const char* fileName, void* data, size_t dataSize)
{
    if( sFileSaveFunction )
        sFileSaveFunction(fileName, data, dataSize);
    else
    {
        FILE* file = fopen(fileName, "wb");
        fwrite(data, 1, dataSize, file);
        fclose(file);
    }
}

void F2M_LogHashAttempt(const char* src, unsigned int nonce, unsigned int* target, unsigned int* hash)
{
    char logStr[1024 * 32];
    sprintf_s(logStr, sizeof(logStr), "(%s) 0x%8.8x - 0x%8.8x %8.8x %8.8x%8.8x%8.8x%8.8x%8.8x%8.8x < 0x%8.8x %8.8x %8.8x%8.8x%8.8x%8.8x%8.8x%8.8x\n", src, nonce, 
        ByteReverse(hash[7]), ByteReverse(hash[6]), ByteReverse(hash[5]), ByteReverse(hash[4]), ByteReverse(hash[3]), ByteReverse(hash[2]), ByteReverse(hash[1]), ByteReverse(hash[0]), 
        (target[7]), (target[6]), (target[5]), (target[4]), (target[3]), (target[2]), (target[1]), (target[0]));

    FILE* logFile = fopen("hashes.log", "ab");
    fwrite(logStr, strlen(logStr), 1, logFile);
    fclose(logFile);
}