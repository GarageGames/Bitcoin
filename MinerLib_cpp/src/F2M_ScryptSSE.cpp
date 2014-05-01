#include "F2M_Work.h"
#include "F2M_Hash.h"

#include <smmintrin.h>
#include <string.h>

struct SSEVector
{
	SSEVector() {}
	SSEVector(__m128i iv) { v = iv; }
	operator __m128i() { return v; }
	__m128i v;
};


PRE_ALIGN(32) unsigned int staticHashw[] POST_ALIGN(32) =
{
    0x6a09e667, 0x6a09e667,0x6a09e667,0x6a09e667,
    0xbb67ae85, 0xbb67ae85,0xbb67ae85,0xbb67ae85,
    0x3c6ef372, 0x3c6ef372,0x3c6ef372,0x3c6ef372,
    0xa54ff53a, 0xa54ff53a,0xa54ff53a,0xa54ff53a,
    0x510e527f, 0x510e527f,0x510e527f,0x510e527f,
    0x9b05688c, 0x9b05688c,0x9b05688c,0x9b05688c,
    0x1f83d9ab, 0x1f83d9ab,0x1f83d9ab,0x1f83d9ab,
    0x5be0cd19, 0x5be0cd19,0x5be0cd19,0x5be0cd19
};
static const SSEVector* staticHashSSE = (SSEVector*)staticHashw;

inline unsigned int ByteReverse(unsigned int value)
{
    value = ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
    return (value<<16) | (value>>16);
}

inline SSEVector vlsh(SSEVector& a, int b)
{
    return _mm_slli_epi32(a, b);
}

inline SSEVector vrsh(SSEVector& a, int b)
{
    return _mm_srli_epi32(a, b);
}

inline SSEVector vmul(SSEVector& a, SSEVector& b)
{
    SSEVector tmp1 = _mm_mul_epu32(a,b);
    SSEVector tmp2 = _mm_mul_epu32( _mm_srli_si128(a,4), _mm_srli_si128(b,4));
    return _mm_unpacklo_epi32(_mm_shuffle_epi32(tmp1, _MM_SHUFFLE (0,0,2,0)), _mm_shuffle_epi32(tmp2, _MM_SHUFFLE (0,0,2,0)));
}

inline SSEVector operator +(SSEVector& a, SSEVector& b)
{
    return _mm_add_epi32(a, b);
}

inline SSEVector operator +(const SSEVector& a, SSEVector& b)
{
    return _mm_add_epi32(a.v, b);
}

inline SSEVector operator |(SSEVector& a, SSEVector& b)
{
    return _mm_or_si128(a, b);
}

inline SSEVector operator ^(SSEVector& a, SSEVector& b)
{
    return _mm_xor_si128(a, b);
}

inline SSEVector operator &(SSEVector& a, SSEVector& b)
{
    return _mm_and_si128(a, b);
}

/*
inline SSEVector ByteReverseSSE(SSEVector value)
{
    SSEVector swapMask = _mm_set_epi8(12, 13, 14, 15, 8, 9, 10, 11, 4, 5, 6, 7, 0, 1, 2, 3);
    return _mm_shuffle_epi8(value, swapMask);
}
*/
inline SSEVector ByteReverseSSE(SSEVector& value)
{
    SSEVector maskA = _mm_set1_epi32(0x00FF00FF);
    SSEVector maskB = _mm_set1_epi32(0xFF00FF00);
    SSEVector shiftedA = _mm_slli_epi32(_mm_and_si128(value, maskA), 8);
    SSEVector shiftedB = _mm_srli_epi32(_mm_and_si128(value, maskB), 8);
    SSEVector result = _mm_or_si128(shiftedA, shiftedB);
    shiftedA = _mm_slli_epi32(result, 16);
    shiftedB = _mm_srli_epi32(result, 16);
    result = _mm_or_si128(shiftedA, shiftedB);
    return result;
}

#define ROTATESSE(a,n)     ((vlsh((a),n))|(vrsh((a), 32-(n))))

#define Sigma0SSE(x)   (ROTATESSE((x),30) ^ ROTATESSE((x),19) ^ ROTATESSE((x),10))
#define Sigma1SSE(x)   (ROTATESSE((x),26) ^ ROTATESSE((x),21) ^ ROTATESSE((x),7))
#define sigma0SSE(x)   (ROTATESSE((x),25) ^ ROTATESSE((x),14) ^ (vrsh((x),3)))
#define sigma1SSE(x)   (ROTATESSE((x),15) ^ ROTATESSE((x),13) ^ (vrsh((x),10)))
#define ChSSE(x,y,z)   (SSEVector(_mm_xor_si128(_mm_and_si128(x, y), _mm_andnot_si128(x, z))))
#define MajSSE(x,y,z)  (((x) & (y)) ^ ((x) & (z)) ^ ((y) & (z)))

static void sha256_blockSSEu(SSEVector* output, const SSEVector* state, const SSEVector* input)
{
	SSEVector a, b, c, d, e, f, g, h, t1, t2;
	SSEVector W[64];
	int i;

	/* load the input */
	for (i = 0; i < 16; i++)
        W[i] = input[i];

	/* now blend */
	for (i = 16; i < 64; i++)
    {
        W[i] = sigma1SSE(W[i-2]) + W[i-7] + sigma0SSE(W[i-15]) + W[i-16];
    }

	/* load the state into our registers */
	a=state[0];  b=state[1];  c=state[2];  d=state[3];
	e=state[4];  f=state[5];  g=state[6];  h=state[7];

	/* now iterate */
	t1 = h + Sigma1SSE(e) + ChSSE(e,f,g) + SSEVector(_mm_set1_epi32(0x428a2f98)) + W[ 0];	t2 = Sigma0SSE(a) + MajSSE(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1SSE(d) + ChSSE(d,e,f) + SSEVector(_mm_set1_epi32(0x71374491)) + W[ 1];	t2 = Sigma0SSE(h) + MajSSE(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1SSE(c) + ChSSE(c,d,e) + SSEVector(_mm_set1_epi32(0xb5c0fbcf)) + W[ 2];	t2 = Sigma0SSE(g) + MajSSE(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1SSE(b) + ChSSE(b,c,d) + SSEVector(_mm_set1_epi32(0xe9b5dba5)) + W[ 3];	t2 = Sigma0SSE(f) + MajSSE(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1SSE(a) + ChSSE(a,b,c) + SSEVector(_mm_set1_epi32(0x3956c25b)) + W[ 4];	t2 = Sigma0SSE(e) + MajSSE(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1SSE(h) + ChSSE(h,a,b) + SSEVector(_mm_set1_epi32(0x59f111f1)) + W[ 5];	t2 = Sigma0SSE(d) + MajSSE(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1SSE(g) + ChSSE(g,h,a) + SSEVector(_mm_set1_epi32(0x923f82a4)) + W[ 6];	t2 = Sigma0SSE(c) + MajSSE(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1SSE(f) + ChSSE(f,g,h) + SSEVector(_mm_set1_epi32(0xab1c5ed5)) + W[ 7];	t2 = Sigma0SSE(b) + MajSSE(b,c,d);    e = e + t1;    a=t1+t2;
                                                                               
	t1 = h + Sigma1SSE(e) + ChSSE(e,f,g) + SSEVector(_mm_set1_epi32(0xd807aa98)) + W[ 8];	t2 = Sigma0SSE(a) + MajSSE(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1SSE(d) + ChSSE(d,e,f) + SSEVector(_mm_set1_epi32(0x12835b01)) + W[ 9];	t2 = Sigma0SSE(h) + MajSSE(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1SSE(c) + ChSSE(c,d,e) + SSEVector(_mm_set1_epi32(0x243185be)) + W[10];	t2 = Sigma0SSE(g) + MajSSE(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1SSE(b) + ChSSE(b,c,d) + SSEVector(_mm_set1_epi32(0x550c7dc3)) + W[11];	t2 = Sigma0SSE(f) + MajSSE(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1SSE(a) + ChSSE(a,b,c) + SSEVector(_mm_set1_epi32(0x72be5d74)) + W[12];	t2 = Sigma0SSE(e) + MajSSE(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1SSE(h) + ChSSE(h,a,b) + SSEVector(_mm_set1_epi32(0x80deb1fe)) + W[13];	t2 = Sigma0SSE(d) + MajSSE(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1SSE(g) + ChSSE(g,h,a) + SSEVector(_mm_set1_epi32(0x9bdc06a7)) + W[14];	t2 = Sigma0SSE(c) + MajSSE(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1SSE(f) + ChSSE(f,g,h) + SSEVector(_mm_set1_epi32(0xc19bf174)) + W[15];	t2 = Sigma0SSE(b) + MajSSE(b,c,d);    e = e + t1;    a=t1+t2;
                                                                               
	t1 = h + Sigma1SSE(e) + ChSSE(e,f,g) + SSEVector(_mm_set1_epi32(0xe49b69c1)) + W[16];	t2 = Sigma0SSE(a) + MajSSE(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1SSE(d) + ChSSE(d,e,f) + SSEVector(_mm_set1_epi32(0xefbe4786)) + W[17];	t2 = Sigma0SSE(h) + MajSSE(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1SSE(c) + ChSSE(c,d,e) + SSEVector(_mm_set1_epi32(0x0fc19dc6)) + W[18];	t2 = Sigma0SSE(g) + MajSSE(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1SSE(b) + ChSSE(b,c,d) + SSEVector(_mm_set1_epi32(0x240ca1cc)) + W[19];	t2 = Sigma0SSE(f) + MajSSE(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1SSE(a) + ChSSE(a,b,c) + SSEVector(_mm_set1_epi32(0x2de92c6f)) + W[20];	t2 = Sigma0SSE(e) + MajSSE(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1SSE(h) + ChSSE(h,a,b) + SSEVector(_mm_set1_epi32(0x4a7484aa)) + W[21];	t2 = Sigma0SSE(d) + MajSSE(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1SSE(g) + ChSSE(g,h,a) + SSEVector(_mm_set1_epi32(0x5cb0a9dc)) + W[22];	t2 = Sigma0SSE(c) + MajSSE(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1SSE(f) + ChSSE(f,g,h) + SSEVector(_mm_set1_epi32(0x76f988da)) + W[23];	t2 = Sigma0SSE(b) + MajSSE(b,c,d);    e = e + t1;    a=t1+t2;
                                                                               
	t1 = h + Sigma1SSE(e) + ChSSE(e,f,g) + SSEVector(_mm_set1_epi32(0x983e5152)) + W[24];	t2 = Sigma0SSE(a) + MajSSE(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1SSE(d) + ChSSE(d,e,f) + SSEVector(_mm_set1_epi32(0xa831c66d)) + W[25];	t2 = Sigma0SSE(h) + MajSSE(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1SSE(c) + ChSSE(c,d,e) + SSEVector(_mm_set1_epi32(0xb00327c8)) + W[26];	t2 = Sigma0SSE(g) + MajSSE(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1SSE(b) + ChSSE(b,c,d) + SSEVector(_mm_set1_epi32(0xbf597fc7)) + W[27];	t2 = Sigma0SSE(f) + MajSSE(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1SSE(a) + ChSSE(a,b,c) + SSEVector(_mm_set1_epi32(0xc6e00bf3)) + W[28];	t2 = Sigma0SSE(e) + MajSSE(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1SSE(h) + ChSSE(h,a,b) + SSEVector(_mm_set1_epi32(0xd5a79147)) + W[29];	t2 = Sigma0SSE(d) + MajSSE(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1SSE(g) + ChSSE(g,h,a) + SSEVector(_mm_set1_epi32(0x06ca6351)) + W[30];	t2 = Sigma0SSE(c) + MajSSE(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1SSE(f) + ChSSE(f,g,h) + SSEVector(_mm_set1_epi32(0x14292967)) + W[31];	t2 = Sigma0SSE(b) + MajSSE(b,c,d);    e = e + t1;    a=t1+t2;
                                                                               
	t1 = h + Sigma1SSE(e) + ChSSE(e,f,g) + SSEVector(_mm_set1_epi32(0x27b70a85)) + W[32];	t2 = Sigma0SSE(a) + MajSSE(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1SSE(d) + ChSSE(d,e,f) + SSEVector(_mm_set1_epi32(0x2e1b2138)) + W[33];	t2 = Sigma0SSE(h) + MajSSE(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1SSE(c) + ChSSE(c,d,e) + SSEVector(_mm_set1_epi32(0x4d2c6dfc)) + W[34];	t2 = Sigma0SSE(g) + MajSSE(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1SSE(b) + ChSSE(b,c,d) + SSEVector(_mm_set1_epi32(0x53380d13)) + W[35];	t2 = Sigma0SSE(f) + MajSSE(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1SSE(a) + ChSSE(a,b,c) + SSEVector(_mm_set1_epi32(0x650a7354)) + W[36];	t2 = Sigma0SSE(e) + MajSSE(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1SSE(h) + ChSSE(h,a,b) + SSEVector(_mm_set1_epi32(0x766a0abb)) + W[37];	t2 = Sigma0SSE(d) + MajSSE(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1SSE(g) + ChSSE(g,h,a) + SSEVector(_mm_set1_epi32(0x81c2c92e)) + W[38];	t2 = Sigma0SSE(c) + MajSSE(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1SSE(f) + ChSSE(f,g,h) + SSEVector(_mm_set1_epi32(0x92722c85)) + W[39];	t2 = Sigma0SSE(b) + MajSSE(b,c,d);    e = e + t1;    a=t1+t2;
                                                                               
	t1 = h + Sigma1SSE(e) + ChSSE(e,f,g) + SSEVector(_mm_set1_epi32(0xa2bfe8a1)) + W[40];	t2 = Sigma0SSE(a) + MajSSE(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1SSE(d) + ChSSE(d,e,f) + SSEVector(_mm_set1_epi32(0xa81a664b)) + W[41];	t2 = Sigma0SSE(h) + MajSSE(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1SSE(c) + ChSSE(c,d,e) + SSEVector(_mm_set1_epi32(0xc24b8b70)) + W[42];	t2 = Sigma0SSE(g) + MajSSE(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1SSE(b) + ChSSE(b,c,d) + SSEVector(_mm_set1_epi32(0xc76c51a3)) + W[43];	t2 = Sigma0SSE(f) + MajSSE(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1SSE(a) + ChSSE(a,b,c) + SSEVector(_mm_set1_epi32(0xd192e819)) + W[44];	t2 = Sigma0SSE(e) + MajSSE(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1SSE(h) + ChSSE(h,a,b) + SSEVector(_mm_set1_epi32(0xd6990624)) + W[45];	t2 = Sigma0SSE(d) + MajSSE(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1SSE(g) + ChSSE(g,h,a) + SSEVector(_mm_set1_epi32(0xf40e3585)) + W[46];	t2 = Sigma0SSE(c) + MajSSE(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1SSE(f) + ChSSE(f,g,h) + SSEVector(_mm_set1_epi32(0x106aa070)) + W[47];	t2 = Sigma0SSE(b) + MajSSE(b,c,d);    e = e + t1;    a=t1+t2;
                                                                               
	t1 = h + Sigma1SSE(e) + ChSSE(e,f,g) + SSEVector(_mm_set1_epi32(0x19a4c116)) + W[48];	t2 = Sigma0SSE(a) + MajSSE(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1SSE(d) + ChSSE(d,e,f) + SSEVector(_mm_set1_epi32(0x1e376c08)) + W[49];	t2 = Sigma0SSE(h) + MajSSE(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1SSE(c) + ChSSE(c,d,e) + SSEVector(_mm_set1_epi32(0x2748774c)) + W[50];	t2 = Sigma0SSE(g) + MajSSE(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1SSE(b) + ChSSE(b,c,d) + SSEVector(_mm_set1_epi32(0x34b0bcb5)) + W[51];	t2 = Sigma0SSE(f) + MajSSE(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1SSE(a) + ChSSE(a,b,c) + SSEVector(_mm_set1_epi32(0x391c0cb3)) + W[52];	t2 = Sigma0SSE(e) + MajSSE(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1SSE(h) + ChSSE(h,a,b) + SSEVector(_mm_set1_epi32(0x4ed8aa4a)) + W[53];	t2 = Sigma0SSE(d) + MajSSE(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1SSE(g) + ChSSE(g,h,a) + SSEVector(_mm_set1_epi32(0x5b9cca4f)) + W[54];	t2 = Sigma0SSE(c) + MajSSE(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1SSE(f) + ChSSE(f,g,h) + SSEVector(_mm_set1_epi32(0x682e6ff3)) + W[55];	t2 = Sigma0SSE(b) + MajSSE(b,c,d);    e = e + t1;    a=t1+t2;
                                                                               
	t1 = h + Sigma1SSE(e) + ChSSE(e,f,g) + SSEVector(_mm_set1_epi32(0x748f82ee)) + W[56];	t2 = Sigma0SSE(a) + MajSSE(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1SSE(d) + ChSSE(d,e,f) + SSEVector(_mm_set1_epi32(0x78a5636f)) + W[57];	t2 = Sigma0SSE(h) + MajSSE(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1SSE(c) + ChSSE(c,d,e) + SSEVector(_mm_set1_epi32(0x84c87814)) + W[58];	t2 = Sigma0SSE(g) + MajSSE(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1SSE(b) + ChSSE(b,c,d) + SSEVector(_mm_set1_epi32(0x8cc70208)) + W[59];	t2 = Sigma0SSE(f) + MajSSE(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1SSE(a) + ChSSE(a,b,c) + SSEVector(_mm_set1_epi32(0x90befffa)) + W[60];	t2 = Sigma0SSE(e) + MajSSE(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1SSE(h) + ChSSE(h,a,b) + SSEVector(_mm_set1_epi32(0xa4506ceb)) + W[61];	t2 = Sigma0SSE(d) + MajSSE(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1SSE(g) + ChSSE(g,h,a) + SSEVector(_mm_set1_epi32(0xbef9a3f7)) + W[62];	t2 = Sigma0SSE(c) + MajSSE(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1SSE(f) + ChSSE(f,g,h) + SSEVector(_mm_set1_epi32(0xc67178f2)) + W[63];	t2 = Sigma0SSE(b) + MajSSE(b,c,d);    e = e + t1;    a=t1+t2;

    output[0] = state[0] + a;
    output[1] = state[1] + b; 
    output[2] = state[2] + c; 
    output[3] = state[3] + d;
	output[4] = state[4] + e; 
    output[5] = state[5] + f; 
    output[6] = state[6] + g; 
    output[7] = state[7] + h;
}

static inline void xor_salsa8SSE(SSEVector B[16], SSEVector Bx[16])
{
	SSEVector x00,x01,x02,x03,x04,x05,x06,x07,x08,x09,x10,x11,x12,x13,x14,x15;
	int i;

	x00 = B[ 0] = (B[ 0] ^ Bx[ 0]);
	x01 = B[ 1] = (B[ 1] ^ Bx[ 1]);
	x02 = B[ 2] = (B[ 2] ^ Bx[ 2]);
	x03 = B[ 3] = (B[ 3] ^ Bx[ 3]);
	x04 = B[ 4] = (B[ 4] ^ Bx[ 4]);
	x05 = B[ 5] = (B[ 5] ^ Bx[ 5]);
	x06 = B[ 6] = (B[ 6] ^ Bx[ 6]);
	x07 = B[ 7] = (B[ 7] ^ Bx[ 7]);
	x08 = B[ 8] = (B[ 8] ^ Bx[ 8]);
	x09 = B[ 9] = (B[ 9] ^ Bx[ 9]);
	x10 = B[10] = (B[10] ^ Bx[10]);
	x11 = B[11] = (B[11] ^ Bx[11]);
	x12 = B[12] = (B[12] ^ Bx[12]);
	x13 = B[13] = (B[13] ^ Bx[13]);
	x14 = B[14] = (B[14] ^ Bx[14]);
	x15 = B[15] = (B[15] ^ Bx[15]);
	for (i = 0; i < 8; i += 2) {
		/* Operate on columns. */
		x04 = x04 ^ ROTATESSE(x00 + x12,  7);  x09 = x09 ^ ROTATESSE(x05 + x01,  7);
		x14 = x14 ^ ROTATESSE(x10 + x06,  7);  x03 = x03 ^ ROTATESSE(x15 + x11,  7);

		x08 = x08 ^ ROTATESSE(x04 + x00,  9);  x13 = x13 ^ ROTATESSE(x09 + x05,  9);
		x02 = x02 ^ ROTATESSE(x14 + x10,  9);  x07 = x07 ^ ROTATESSE(x03 + x15,  9);

		x12 = x12 ^ ROTATESSE(x08 + x04, 13);  x01 = x01 ^ ROTATESSE(x13 + x09, 13);
		x06 = x06 ^ ROTATESSE(x02 + x14, 13);  x11 = x11 ^ ROTATESSE(x07 + x03, 13);

		x00 = x00 ^ ROTATESSE(x12 + x08, 18);  x05 = x05 ^ ROTATESSE(x01 + x13, 18);
		x10 = x10 ^ ROTATESSE(x06 + x02, 18);  x15 = x15 ^ ROTATESSE(x11 + x07, 18);

		/* Operate on rows. */
		x01 = x01 ^ ROTATESSE(x00 + x03,  7);  x06 = x06 ^ ROTATESSE(x05 + x04,  7);
		x11 = x11 ^ ROTATESSE(x10 + x09,  7);  x12 = x12 ^ ROTATESSE(x15 + x14,  7);

		x02 = x02 ^ ROTATESSE(x01 + x00,  9);  x07 = x07 ^ ROTATESSE(x06 + x05,  9);
		x08 = x08 ^ ROTATESSE(x11 + x10,  9);  x13 = x13 ^ ROTATESSE(x12 + x15,  9);

		x03 = x03 ^ ROTATESSE(x02 + x01, 13);  x04 = x04 ^ ROTATESSE(x07 + x06, 13);
		x09 = x09 ^ ROTATESSE(x08 + x11, 13);  x14 = x14 ^ ROTATESSE(x13 + x12, 13);

		x00 = x00 ^ ROTATESSE(x03 + x02, 18);  x05 = x05 ^ ROTATESSE(x04 + x07, 18);
		x10 = x10 ^ ROTATESSE(x09 + x08, 18);  x15 = x15 ^ ROTATESSE(x14 + x13, 18);
	}
	B[ 0] = B[ 0] + x00;
	B[ 1] = B[ 1] + x01;
	B[ 2] = B[ 2] + x02;
	B[ 3] = B[ 3] + x03;
	B[ 4] = B[ 4] + x04;
	B[ 5] = B[ 5] + x05;
	B[ 6] = B[ 6] + x06;
	B[ 7] = B[ 7] + x07;
	B[ 8] = B[ 8] + x08;
	B[ 9] = B[ 9] + x09;
	B[10] = B[10] + x10;
	B[11] = B[11] + x11;
	B[12] = B[12] + x12;
	B[13] = B[13] + x13;
	B[14] = B[14] + x14;
	B[15] = B[15] + x15;
}



void ScryptHashSSE(F2M_ScryptDataSSE* data)
{
    SSEVector inner[8];
    SSEVector outer[8];
    sha256_blockSSEu(inner, staticHashSSE, (const SSEVector*)data->input);
    sha256_blockSSEu(inner, inner, (const SSEVector*)data->inputB);
    	
    SSEVector const36 = _mm_set1_epi8(0x36);
    SSEVector const5c = _mm_set1_epi8(0x5c);
    for( int i = 0; i < 8; i++ )
    {
        data->pad36[i] = inner[i] ^ const36;
        data->pad5c[i] = inner[i] ^ const5c;
    }
    sha256_blockSSEu(inner, staticHashSSE, (const SSEVector*)data->pad36);
    sha256_blockSSEu(outer, staticHashSSE, (const SSEVector*)data->pad5c);

    SSEVector salted[8];
    sha256_blockSSEu(salted, inner, (const SSEVector*)data->input);

    SSEVector bp[32];
    for( int i = 0; i < 4; i++ )
    {
        data->inputB2[4] = _mm_set1_epi32(i + 1);

        sha256_blockSSEu((SSEVector*)data->tempHash, salted, (const SSEVector*)data->inputB2);
        sha256_blockSSEu(&bp[i * 8], outer, (const SSEVector*)data->tempHash);
    }
	
    SSEVector X[32];
    SSEVector V[1024 * 32];
	for (int i = 0; i < 32; i++)
        X[i] = ByteReverseSSE(bp[i]);

	for (int i = 0; i < 1024; i++) 
    {
        for( int j = 0; j < 32; j++ )
            V[(i * 32) + j] = X[j];
		xor_salsa8SSE(&X[0], &X[16]);
		xor_salsa8SSE(&X[16], &X[0]);
	}

    SSEVector const1023 = _mm_set1_epi32(0x3FF);
    SSEVector const32 = _mm_set1_epi32(32);
    SSEVector quadMask1 = _mm_set_epi32(0xFFFFFFFF, 0, 0, 0);
    SSEVector quadMask2 = _mm_set_epi32(0, 0xFFFFFFFF, 0, 0);
    SSEVector quadMask3 = _mm_set_epi32(0, 0, 0xFFFFFFFF, 0);
    SSEVector quadMask4 = _mm_set_epi32(0, 0, 0, 0xFFFFFFFF);
	for (int i = 0; i < 1024; i++) 
    {
        SSEVector lessThan1024 = (X[16] & const1023);
		SSEVector j = vmul(const32, lessThan1024);
		for (unsigned int k = 0; k < 32; k++)
        {
            SSEVector vk = _mm_set1_epi32(k);
            SSEVector idx = j + vk;

            __m128 abcd;
            _mm_store_si128((__m128i*)&abcd, idx);
            unsigned int* pieces = (unsigned int*)&abcd;
            SSEVector Va = V[pieces[3]] & quadMask1;
            SSEVector Vb = V[pieces[2]] & quadMask2;
            SSEVector Vc = V[pieces[1]] & quadMask3;
            SSEVector Vd = V[pieces[0]] & quadMask4;
            SSEVector vtotal = Va | Vb | Vc | Vd;
            X[k] = X[k] ^ vtotal;
        }
		xor_salsa8SSE(&X[0], &X[16]);
		xor_salsa8SSE(&X[16], &X[0]);
	}

    for (int i = 0; i < 32; i++)
        bp[i] = ByteReverseSSE(X[i]);
    
    sha256_blockSSEu(salted, inner, bp);
    sha256_blockSSEu(salted, salted, bp + 16);

    sha256_blockSSEu((SSEVector*)data->tempHash, salted, (const SSEVector*)data->dataBuffer2);
    sha256_blockSSEu((SSEVector*)data->output, outer, (const SSEVector*)data->tempHash);
}

F2M_ScryptDataSSE* F2M_ScryptInitSSE(F2M_Work* work)
{
    F2M_ScryptDataSSE* data = (F2M_ScryptDataSSE*)_aligned_malloc(sizeof(F2M_ScryptDataSSE), 128);
    memset(data, 0, sizeof(F2M_ScryptDataSSE));

    unsigned int dataBuffer[16] = {0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x000004A0};	
    dataBuffer[0] = work->dataFull[16];
    dataBuffer[1] = work->dataFull[17];
    dataBuffer[2] = work->dataFull[18];
    dataBuffer[3] = work->dataFull[19];

    for( int i = 0; i < 16; i++ )
    {
        data->input[i] = _mm_set1_epi32(work->dataFull[i]);
        data->inputB[i] = _mm_set1_epi32(work->dataFull[16 + i]);
        data->inputB2[i] = _mm_set1_epi32(dataBuffer[i]);
    }

    for( int i = 8; i < 16; i++ )
    {
        data->pad36[i] = _mm_set1_epi8(0x36);
        data->pad5c[i] = _mm_set1_epi8(0x5c);
    }
    data->dataBuffer2[0] = _mm_set1_epi32(0x00000001);
    data->dataBuffer2[1] = _mm_set1_epi32(0x80000000);
    data->dataBuffer2[15] = _mm_set1_epi32(0x00000620);

    data->tempHash[8] = _mm_set1_epi32(0x80000000);
    data->tempHash[15] = _mm_set1_epi32(0x00000300);
    
    unsigned int difficulty = work->dataFull[18];
    unsigned diffZeros = 32 - (difficulty & 0xFF);
    switch( diffZeros )
    {
        case 0:
            data->outputMask = _mm_set1_epi32(0);
            break;
        case 1:
            data->outputMask = _mm_set1_epi32(0xFF000000);
            break;
        case 2:
            data->outputMask = _mm_set1_epi32(0xFFFF0000);
            break;
        case 3:
            data->outputMask = _mm_set1_epi32(0x00FFFFFF);
            break;
        default:
            data->outputMask = _mm_set1_epi32(0xFFFFFFFF);
            break;
    }

    return data;
}

void F2M_ScryptCleanupSSE(F2M_ScryptDataSSE* scryptData)
{
    _aligned_free(scryptData);
}

int F2M_ScryptHashSSE(__m128i nonce,  F2M_Work* work, F2M_ScryptDataSSE* data)
{
    data->inputB[3] = data->inputB2[3] = nonce;
    ScryptHashSSE(data);


    SSEVector shifted = _mm_shuffle_epi32(data->output[7], 0x93);
    SSEVector aandbcandd = _mm_and_si128(data->output[7], shifted);
    shifted = _mm_shuffle_epi32(aandbcandd, 0x55);
    SSEVector test = _mm_and_si128(aandbcandd, shifted);
    SSEVector masked = _mm_and_si128(test, data->outputMask);

    __m128 testVal;
    _mm_store_si128((__m128i*)&testVal, masked);
    unsigned int* testPtr = (unsigned int*)&testVal;
    if( testPtr[3] == 0 )
    {
        // Full test, one of these 4 looks good
        __m128* memHash = (__m128*)data->output;
        for( int j = 0; j < 4; j++ )
        {
            for( int k = 7; k > 0; k-- )
            {
                testPtr = (unsigned int*)&memHash[k];
                unsigned int hashval = ByteReverse(testPtr[j]);
                if( hashval > work->target[k] )
                    break;
                if( hashval < work->target[k] )
                {
                    return (3 - j);
                }        
            }
        }
    }
    return -1;
}
