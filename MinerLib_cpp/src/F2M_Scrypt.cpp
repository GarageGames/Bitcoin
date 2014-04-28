#include "F2M_Work.h"
#include "F2M_Hash.h"
#include <stdio.h>
#include <string.h>

#define ROTATE(a,n)     (((a)<<(n))|(((a)&0xffffffff)>>(32-(n))))

#define Sigma0(x)   (ROTATE((x),30) ^ ROTATE((x),19) ^ ROTATE((x),10))
#define Sigma1(x)   (ROTATE((x),26) ^ ROTATE((x),21) ^ ROTATE((x),7))
#define sigma0(x)   (ROTATE((x),25) ^ ROTATE((x),14) ^ ((x)>>3))
#define sigma1(x)   (ROTATE((x),15) ^ ROTATE((x),13) ^ ((x)>>10))
#define Ch(x,y,z)   (((x) & (y)) ^ ((~(x)) & (z)))
#define Maj(x,y,z)  (((x) & (y)) ^ ((x) & (z)) ^ ((y) & (z)))

unsigned int K256[] = { 0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5, 0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5, 0xd807aa98,0x12835b01,0x243185be,0x550c7dc3, 0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174, 0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc, 0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da, 0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7, 0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967, 0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13, 0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85, 0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3, 0xd192e819,0xd6990624,0xf40e3585,0x106aa070, 0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5, 0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3, 0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208, 0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2 };
unsigned int staticHash[] = {0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19};

inline unsigned int ByteReverse(unsigned int value)
{
    value = ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
    return (value<<16) | (value>>16);
}
#define ROTL(a, b) (((a) << (b)) | ((a) >> (32 - (b))))
static inline void xor_salsa8(unsigned int B[16], const unsigned int Bx[16])
{
	unsigned int x00,x01,x02,x03,x04,x05,x06,x07,x08,x09,x10,x11,x12,x13,x14,x15;
	int i;

	x00 = (B[ 0] ^= Bx[ 0]);
	x01 = (B[ 1] ^= Bx[ 1]);
	x02 = (B[ 2] ^= Bx[ 2]);
	x03 = (B[ 3] ^= Bx[ 3]);
	x04 = (B[ 4] ^= Bx[ 4]);
	x05 = (B[ 5] ^= Bx[ 5]);
	x06 = (B[ 6] ^= Bx[ 6]);
	x07 = (B[ 7] ^= Bx[ 7]);
	x08 = (B[ 8] ^= Bx[ 8]);
	x09 = (B[ 9] ^= Bx[ 9]);
	x10 = (B[10] ^= Bx[10]);
	x11 = (B[11] ^= Bx[11]);
	x12 = (B[12] ^= Bx[12]);
	x13 = (B[13] ^= Bx[13]);
	x14 = (B[14] ^= Bx[14]);
	x15 = (B[15] ^= Bx[15]);
	for (i = 0; i < 8; i += 2) {
		/* Operate on columns. */
		x04 ^= ROTL(x00 + x12,  7);  x09 ^= ROTL(x05 + x01,  7);
		x14 ^= ROTL(x10 + x06,  7);  x03 ^= ROTL(x15 + x11,  7);

		x08 ^= ROTL(x04 + x00,  9);  x13 ^= ROTL(x09 + x05,  9);
		x02 ^= ROTL(x14 + x10,  9);  x07 ^= ROTL(x03 + x15,  9);

		x12 ^= ROTL(x08 + x04, 13);  x01 ^= ROTL(x13 + x09, 13);
		x06 ^= ROTL(x02 + x14, 13);  x11 ^= ROTL(x07 + x03, 13);

		x00 ^= ROTL(x12 + x08, 18);  x05 ^= ROTL(x01 + x13, 18);
		x10 ^= ROTL(x06 + x02, 18);  x15 ^= ROTL(x11 + x07, 18);

		/* Operate on rows. */
		x01 ^= ROTL(x00 + x03,  7);  x06 ^= ROTL(x05 + x04,  7);
		x11 ^= ROTL(x10 + x09,  7);  x12 ^= ROTL(x15 + x14,  7);

		x02 ^= ROTL(x01 + x00,  9);  x07 ^= ROTL(x06 + x05,  9);
		x08 ^= ROTL(x11 + x10,  9);  x13 ^= ROTL(x12 + x15,  9);

		x03 ^= ROTL(x02 + x01, 13);  x04 ^= ROTL(x07 + x06, 13);
		x09 ^= ROTL(x08 + x11, 13);  x14 ^= ROTL(x13 + x12, 13);

		x00 ^= ROTL(x03 + x02, 18);  x05 ^= ROTL(x04 + x07, 18);
		x10 ^= ROTL(x09 + x08, 18);  x15 ^= ROTL(x14 + x13, 18);
	}
	B[ 0] += x00;
	B[ 1] += x01;
	B[ 2] += x02;
	B[ 3] += x03;
	B[ 4] += x04;
	B[ 5] += x05;
	B[ 6] += x06;
	B[ 7] += x07;
	B[ 8] += x08;
	B[ 9] += x09;
	B[10] += x10;
	B[11] += x11;
	B[12] += x12;
	B[13] += x13;
	B[14] += x14;
	B[15] += x15;
}

void sha256_block(unsigned int* output, unsigned int* state, const unsigned int* input)
{
	unsigned int a,b,c,d,e,f,g,h,s0,s1,T1,T2;
	unsigned int	X[16];
	int i;
	
	a = state[0];	b = state[1];	c = state[2];	d = state[3];
	e = state[4];	f = state[5];	g = state[6];	h = state[7];

	for (i=0;i<16;i++)
	{
        T1 = X[i] = input[i];
	    T1 += h;
        T1 += Sigma1(e);
        T1 += Ch(e,f,g);
        T1 += K256[i];
	    T2 = Sigma0(a) + Maj(a,b,c);
	    h = g;	g = f;	f = e;	e = d + T1;
	    d = c;	c = b;	b = a;	a = T1 + T2;
	}

	for (;i<64;i++)
	{
	    s0 = X[(i+1)&0x0f];	s0 = sigma0(s0);
	    s1 = X[(i+14)&0x0f];	s1 = sigma1(s1);

	    T1 = X[i&0xf] += s0 + s1 + X[(i+9)&0xf];
	    T1 += h + Sigma1(e) + Ch(e,f,g) + K256[i];
	    T2 = Sigma0(a) + Maj(a,b,c);
	    h = g;	g = f;	f = e;	e = d + T1;
	    d = c;	c = b;	b = a;	a = T1 + T2;
	}
    
    output[0] = state[0] + a;
    output[1] = state[1] + b;
    output[2] = state[2] + c;
    output[3] = state[3] + d;
    output[4] = state[4] + e;
    output[5] = state[5] + f;
    output[6] = state[6] + g;
    output[7] = state[7] + h;
}

void ScryptHashOpt(const unsigned int *inputA, const unsigned int* inputB, F2M_ScryptData* scrypt)
{
    unsigned int inner[8];
    unsigned int outer[8];
    sha256_block(inner, staticHash, inputA);
    sha256_block(inner, inner, inputB);
    	
    for( int i = 0; i < 8; i++ )
    {
        scrypt->pad36[i] = inner[i] ^ 0x36363636;
        scrypt->pad5c[i] = inner[i] ^ 0x5c5c5c5c;
    }
    sha256_block(inner, staticHash, scrypt->pad36);
    sha256_block(outer, staticHash, scrypt->pad5c);

    unsigned int salted[8];
    sha256_block(salted, inner, inputA);

    unsigned int bp[32];
    for( int i = 0; i < 4; i++ )
    {
        scrypt->dataBuffer[4] = (i + 1);

        sha256_block(scrypt->tempHash, salted, scrypt->dataBuffer);
        sha256_block(&bp[i * 8], outer, scrypt->tempHash);
    }
	
    unsigned int X[32];
    unsigned int V[1024 * 32];
	for (int i = 0; i < 32; i++)
        X[i] = ByteReverse(bp[i]);

	for (int i = 0; i < 1024; i++) {
		memcpy(&V[i * 32], X, 128);
		xor_salsa8(&X[0], &X[16]);
		xor_salsa8(&X[16], &X[0]);
	}
	for (int i = 0; i < 1024; i++) {
		unsigned int j = 32 * (X[16] & 1023);
		for (unsigned int k = 0; k < 32; k++)
			X[k] ^= V[j + k];
		xor_salsa8(&X[0], &X[16]);
		xor_salsa8(&X[16], &X[0]);
	}

    for (int i = 0; i < 32; i++)
        bp[i] = ByteReverse(X[i]);
    
    sha256_block(salted, inner, bp);
    sha256_block(salted, salted, bp + 16);

    sha256_block(scrypt->tempHash, salted, scrypt->dataBuffer2);
    sha256_block(scrypt->output, outer, scrypt->tempHash);
}

F2M_ScryptData* F2M_ScryptInit(F2M_Work* work)
{
    F2M_ScryptData* data = new F2M_ScryptData();

    
    unsigned int pad36[16] = {0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x36363636, 0x36363636, 0x36363636, 0x36363636, 0x36363636, 0x36363636, 0x36363636, 0x36363636};	
    unsigned int pad5c[16] = {0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c};
    unsigned int dataBuffer[16] = {0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x000004A0};	
    unsigned int dataBuffer2[16] = {0x00000001, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000620};	
    unsigned int tempHash[16] = {0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000300};    
    memcpy(data->pad36, pad36, sizeof(pad36));
    memcpy(data->pad5c, pad5c, sizeof(pad5c));
    memcpy(data->dataBuffer, dataBuffer, sizeof(dataBuffer));
    memcpy(data->dataBuffer2, dataBuffer2, sizeof(dataBuffer2));
    memcpy(data->tempHash, tempHash, sizeof(tempHash));

    data->dataBuffer[0] = work->dataFull[16];
    data->dataBuffer[1] = work->dataFull[17];
    data->dataBuffer[2] = work->dataFull[18];

    unsigned int difficulty = work->dataFull[18];
    unsigned diffZeros = 32 - (difficulty & 0xFF);
    switch( diffZeros )
    {
        case 0:
            data->outputMask = 0;
            break;
        case 1:
            data->outputMask = 0xFF000000;
            break;
        case 2:
            data->outputMask = 0xFFFF0000;
            break;
        case 3:
            data->outputMask = 0x00FFFFFF;
            break;
        default:
            data->outputMask = 0xFFFFFFFF;
            break;
    }

    return data;
}

void F2M_ScryptCleanup(F2M_ScryptData* scryptData)
{
    delete scryptData;
}

bool F2M_ScryptHash(unsigned int nonce, F2M_Work* work, F2M_ScryptData* scrypt)
{
    scrypt->dataBuffer[3] = work->dataFull[19] = nonce;
    ScryptHashOpt(work->dataFull, &work->dataFull[16], scrypt);

    if( (scrypt->output[7] & scrypt->outputMask) == 0 )
    {
        for( int i = 7; i > 0; i-- )
        {
            unsigned int outputVal = ByteReverse(scrypt->output[i]);
            if( outputVal > work->target[i] )
                break;
            if( outputVal < work->target[i] )
            {
                // Found solution
                return true;
            }
        }
    }

    return false;
}