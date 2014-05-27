#ifndef _F2M_UTILS_H_
#define _F2M_UTILS_H_

typedef void* (*F2M_FileLoadFunction)(const char* fileName, size_t* optionalOutFileSize);
typedef void (*F2M_FileUnloadFunction)(void* fileData);
typedef void (*F2M_FileSaveFunction)(const char* fileName, void* data, size_t dataSize);


void F2M_SetFileFunctions(F2M_FileLoadFunction loadFunction, F2M_FileUnloadFunction unloadFunction, F2M_FileSaveFunction saveFunction);

void* F2M_FileLoad(const char* fileName, size_t* outFileSize);
void F2M_FileUnload(void* fileData);
void F2M_FileSave(const char* fileName, void* data, size_t dataSize);

bool F2M_HardwareSupportsSIMD();

void F2M_LogHashAttempt(const char* src, unsigned int nonce, unsigned int* target, unsigned int* hash);

#endif