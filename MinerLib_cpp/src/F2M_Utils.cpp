#include "F2M_Utils.h"
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