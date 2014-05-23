#ifndef _F2M_UNIT_TEST_H_
#define _F2M_UNIT_TEST_H_

bool F2M_TestAll();

bool F2M_Scrypt_TestAll();
bool F2M_Scrypt_TestStandard();
bool F2M_Scrypt_TestSSE();
bool F2M_Scrypt_TestOpenCL();

bool F2M_DSHA256_TestAll();
bool F2M_DSHA256_TestStandard();
bool F2M_DSHA256_TestSSE();
bool F2M_DSHA256_TestOpenCL();

#endif // _F2M_UNIT_TEST_H_