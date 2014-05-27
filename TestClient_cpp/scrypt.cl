
#define ROTATE(x,y) rotate(x,(uint)y)
#define ByteReverse(n) (rotate(n & 0x00FF00FF, 24U)|(rotate(n, 8U) & 0x00FF00FF))

#define Sigma0(x)   (ROTATE((x),30) ^ ROTATE((x),19) ^ ROTATE((x),10))
#define Sigma1(x)   (ROTATE((x),26) ^ ROTATE((x),21) ^ ROTATE((x),7))
#define sigma0(x)   (ROTATE((x),25) ^ ROTATE((x),14) ^ ((x)>>3))
#define sigma1(x)   (ROTATE((x),15) ^ ROTATE((x),13) ^ ((x)>>10))
#define Ch(x,y,z) bitselect(z,y,x)
#define Maj(x,y,z) Ch((x^z),y,z)

void sha256Block(uint4* output, const uint4 stateA, const uint4 stateB, const uint4 inA, const uint4 inB, const uint4 inC, const uint4 inD)
{
    uint4 W[16];

    W[0] = inA;
    W[1] = inB;
    W[2] = inC;
    W[3] = inD;

    for( int i = 4; i < 16; i++ )
    {
        W[i] = (uint4)(W[i - 2].y, W[i - 2].z, W[i - 2].w, W[i - 1].x) + W[i - 4] + sigma0((uint4)(W[i - 4].y, W[i - 4].z, W[i - 4].w, W[i - 3].x));
        W[i].xy += sigma1((uint2)(W[i - 1].z, W[i - 1].w));
        W[i].zw += sigma1((uint2)(W[i].x, W[i].y));
    }
    
    uint t1, t2;
    uint a = stateA.x;
    uint b = stateA.y;
    uint c = stateA.z;
    uint d = stateA.w;
    uint e = stateB.x;
    uint f = stateB.y;
    uint g = stateB.z;
    uint h = stateB.w;

    t1 = h + Sigma1(e) + Ch(e,f,g) + (0x428a2f98) + W[ 0].x;
	t2 = Sigma0(a) + Maj(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1(d) + Ch(d,e,f) + (0x71374491) + W[ 0].y;
	t2 = Sigma0(h) + Maj(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1(c) + Ch(c,d,e) + (0xb5c0fbcf) + W[ 0].z;
	t2 = Sigma0(g) + Maj(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1(b) + Ch(b,c,d) + (0xe9b5dba5) + W[ 0].w;
	t2 = Sigma0(f) + Maj(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1(a) + Ch(a,b,c) + (0x3956c25b) + W[ 1].x;
	t2 = Sigma0(e) + Maj(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1(h) + Ch(h,a,b) + (0x59f111f1) + W[ 1].y;
	t2 = Sigma0(d) + Maj(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1(g) + Ch(g,h,a) + (0x923f82a4) + W[ 1].z;
	t2 = Sigma0(c) + Maj(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1(f) + Ch(f,g,h) + (0xab1c5ed5) + W[ 1].w;
	t2 = Sigma0(b) + Maj(b,c,d);    e = e + t1;    a=t1+t2;

	t1 = h + Sigma1(e) + Ch(e,f,g) + (0xd807aa98) + W[ 2].x;
	t2 = Sigma0(a) + Maj(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1(d) + Ch(d,e,f) + (0x12835b01) + W[ 2].y;
	t2 = Sigma0(h) + Maj(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1(c) + Ch(c,d,e) + (0x243185be) + W[ 2].z;
	t2 = Sigma0(g) + Maj(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1(b) + Ch(b,c,d) + (0x550c7dc3) + W[ 2].w;
	t2 = Sigma0(f) + Maj(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1(a) + Ch(a,b,c) + (0x72be5d74) + W[ 3].x;
	t2 = Sigma0(e) + Maj(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1(h) + Ch(h,a,b) + (0x80deb1fe) + W[ 3].y;
	t2 = Sigma0(d) + Maj(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1(g) + Ch(g,h,a) + (0x9bdc06a7) + W[ 3].z;
	t2 = Sigma0(c) + Maj(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1(f) + Ch(f,g,h) + (0xc19bf174) + W[ 3].w;
	t2 = Sigma0(b) + Maj(b,c,d);    e = e + t1;    a=t1+t2;

	t1 = h + Sigma1(e) + Ch(e,f,g) + (0xe49b69c1) + W[ 4].x;
	t2 = Sigma0(a) + Maj(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1(d) + Ch(d,e,f) + (0xefbe4786) + W[ 4].y;
	t2 = Sigma0(h) + Maj(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1(c) + Ch(c,d,e) + (0x0fc19dc6) + W[ 4].z;
	t2 = Sigma0(g) + Maj(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1(b) + Ch(b,c,d) + (0x240ca1cc) + W[ 4].w;
	t2 = Sigma0(f) + Maj(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1(a) + Ch(a,b,c) + (0x2de92c6f) + W[ 5].x;
	t2 = Sigma0(e) + Maj(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1(h) + Ch(h,a,b) + (0x4a7484aa) + W[ 5].y;
	t2 = Sigma0(d) + Maj(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1(g) + Ch(g,h,a) + (0x5cb0a9dc) + W[ 5].z;
	t2 = Sigma0(c) + Maj(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1(f) + Ch(f,g,h) + (0x76f988da) + W[ 5].w;
	t2 = Sigma0(b) + Maj(b,c,d);    e = e + t1;    a=t1+t2;

	t1 = h + Sigma1(e) + Ch(e,f,g) + (0x983e5152) + W[ 6].x;
	t2 = Sigma0(a) + Maj(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1(d) + Ch(d,e,f) + (0xa831c66d) + W[ 6].y;
	t2 = Sigma0(h) + Maj(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1(c) + Ch(c,d,e) + (0xb00327c8) + W[ 6].z;
	t2 = Sigma0(g) + Maj(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1(b) + Ch(b,c,d) + (0xbf597fc7) + W[ 6].w;
	t2 = Sigma0(f) + Maj(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1(a) + Ch(a,b,c) + (0xc6e00bf3) + W[ 7].x;
	t2 = Sigma0(e) + Maj(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1(h) + Ch(h,a,b) + (0xd5a79147) + W[ 7].y;
	t2 = Sigma0(d) + Maj(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1(g) + Ch(g,h,a) + (0x06ca6351) + W[ 7].z;
	t2 = Sigma0(c) + Maj(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1(f) + Ch(f,g,h) + (0x14292967) + W[ 7].w;
	t2 = Sigma0(b) + Maj(b,c,d);    e = e + t1;    a=t1+t2;

	t1 = h + Sigma1(e) + Ch(e,f,g) + (0x27b70a85) + W[ 8].x;
	t2 = Sigma0(a) + Maj(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1(d) + Ch(d,e,f) + (0x2e1b2138) + W[ 8].y;
	t2 = Sigma0(h) + Maj(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1(c) + Ch(c,d,e) + (0x4d2c6dfc) + W[ 8].z;
	t2 = Sigma0(g) + Maj(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1(b) + Ch(b,c,d) + (0x53380d13) + W[ 8].w;
	t2 = Sigma0(f) + Maj(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1(a) + Ch(a,b,c) + (0x650a7354) + W[ 9].x;
	t2 = Sigma0(e) + Maj(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1(h) + Ch(h,a,b) + (0x766a0abb) + W[ 9].y;
	t2 = Sigma0(d) + Maj(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1(g) + Ch(g,h,a) + (0x81c2c92e) + W[ 9].z;
	t2 = Sigma0(c) + Maj(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1(f) + Ch(f,g,h) + (0x92722c85) + W[ 9].w;
	t2 = Sigma0(b) + Maj(b,c,d);    e = e + t1;    a=t1+t2;

	t1 = h + Sigma1(e) + Ch(e,f,g) + (0xa2bfe8a1) + W[10].x;
	t2 = Sigma0(a) + Maj(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1(d) + Ch(d,e,f) + (0xa81a664b) + W[10].y;
	t2 = Sigma0(h) + Maj(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1(c) + Ch(c,d,e) + (0xc24b8b70) + W[10].z;
	t2 = Sigma0(g) + Maj(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1(b) + Ch(b,c,d) + (0xc76c51a3) + W[10].w;
	t2 = Sigma0(f) + Maj(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1(a) + Ch(a,b,c) + (0xd192e819) + W[11].x;
	t2 = Sigma0(e) + Maj(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1(h) + Ch(h,a,b) + (0xd6990624) + W[11].y;
	t2 = Sigma0(d) + Maj(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1(g) + Ch(g,h,a) + (0xf40e3585) + W[11].z;
	t2 = Sigma0(c) + Maj(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1(f) + Ch(f,g,h) + (0x106aa070) + W[11].w;
	t2 = Sigma0(b) + Maj(b,c,d);    e = e + t1;    a=t1+t2;

	t1 = h + Sigma1(e) + Ch(e,f,g) + (0x19a4c116) + W[12].x;
	t2 = Sigma0(a) + Maj(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1(d) + Ch(d,e,f) + (0x1e376c08) + W[12].y;
	t2 = Sigma0(h) + Maj(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1(c) + Ch(c,d,e) + (0x2748774c) + W[12].z;
	t2 = Sigma0(g) + Maj(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1(b) + Ch(b,c,d) + (0x34b0bcb5) + W[12].w;
	t2 = Sigma0(f) + Maj(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1(a) + Ch(a,b,c) + (0x391c0cb3) + W[13].x;
	t2 = Sigma0(e) + Maj(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1(h) + Ch(h,a,b) + (0x4ed8aa4a) + W[13].y;
	t2 = Sigma0(d) + Maj(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1(g) + Ch(g,h,a) + (0x5b9cca4f) + W[13].z;
	t2 = Sigma0(c) + Maj(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1(f) + Ch(f,g,h) + (0x682e6ff3) + W[13].w;
	t2 = Sigma0(b) + Maj(b,c,d);    e = e + t1;    a=t1+t2;

	t1 = h + Sigma1(e) + Ch(e,f,g) + (0x748f82ee) + W[14].x;
	t2 = Sigma0(a) + Maj(a,b,c);    d = d + t1;    h=t1+t2;
	t1 = g + Sigma1(d) + Ch(d,e,f) + (0x78a5636f) + W[14].y;
	t2 = Sigma0(h) + Maj(h,a,b);    c = c + t1;    g=t1+t2;
	t1 = f + Sigma1(c) + Ch(c,d,e) + (0x84c87814) + W[14].z;
	t2 = Sigma0(g) + Maj(g,h,a);    b = b + t1;    f=t1+t2;
	t1 = e + Sigma1(b) + Ch(b,c,d) + (0x8cc70208) + W[14].w;
	t2 = Sigma0(f) + Maj(f,g,h);    a = a + t1;    e=t1+t2;
	t1 = d + Sigma1(a) + Ch(a,b,c) + (0x90befffa) + W[15].x;
	t2 = Sigma0(e) + Maj(e,f,g);    h = h + t1;    d=t1+t2;
	t1 = c + Sigma1(h) + Ch(h,a,b) + (0xa4506ceb) + W[15].y;
	t2 = Sigma0(d) + Maj(d,e,f);    g = g + t1;    c=t1+t2;
	t1 = b + Sigma1(g) + Ch(g,h,a) + (0xbef9a3f7) + W[15].z;
	t2 = Sigma0(c) + Maj(c,d,e);    f = f + t1;    b=t1+t2;
	t1 = a + Sigma1(f) + Ch(f,g,h) + (0xc67178f2) + W[15].w;
	t2 = Sigma0(b) + Maj(b,c,d);    e = e + t1;    a=t1+t2;

    output[0] = stateA + (uint4)(a, b, c, d);
    output[1] = stateB + (uint4)(e, f, g, h);
}


void salsa(uint4* B)
{
	uint x00,x01,x02,x03,x04,x05,x06,x07,x08,x09,x10,x11,x12,x13,x14,x15;
	int i;

	x00 = (B[0].x ^= B[4].x);
	x01 = (B[0].y ^= B[4].y);
	x02 = (B[0].z ^= B[4].z);
	x03 = (B[0].w ^= B[4].w);
	x04 = (B[1].x ^= B[5].x);
	x05 = (B[1].y ^= B[5].y);
	x06 = (B[1].z ^= B[5].z);
	x07 = (B[1].w ^= B[5].w);
	x08 = (B[2].x ^= B[6].x);
	x09 = (B[2].y ^= B[6].y);
	x10 = (B[2].z ^= B[6].z);
	x11 = (B[2].w ^= B[6].w);
	x12 = (B[3].x ^= B[7].x);
	x13 = (B[3].y ^= B[7].y);
	x14 = (B[3].z ^= B[7].z);
	x15 = (B[3].w ^= B[7].w);

    #pragma unroll
	for (i = 0; i < 4; i ++)
    {
		x04 ^= ROTATE(x00 + x12,  7);  x09 ^= ROTATE(x05 + x01,  7);
		x14 ^= ROTATE(x10 + x06,  7);  x03 ^= ROTATE(x15 + x11,  7);

		x08 ^= ROTATE(x04 + x00,  9);  x13 ^= ROTATE(x09 + x05,  9);
		x02 ^= ROTATE(x14 + x10,  9);  x07 ^= ROTATE(x03 + x15,  9);

		x12 ^= ROTATE(x08 + x04, 13);  x01 ^= ROTATE(x13 + x09, 13);
		x06 ^= ROTATE(x02 + x14, 13);  x11 ^= ROTATE(x07 + x03, 13);

		x00 ^= ROTATE(x12 + x08, 18);  x05 ^= ROTATE(x01 + x13, 18);
		x10 ^= ROTATE(x06 + x02, 18);  x15 ^= ROTATE(x11 + x07, 18);		

		x01 ^= ROTATE(x00 + x03,  7);  x06 ^= ROTATE(x05 + x04,  7);
		x11 ^= ROTATE(x10 + x09,  7);  x12 ^= ROTATE(x15 + x14,  7);

		x02 ^= ROTATE(x01 + x00,  9);  x07 ^= ROTATE(x06 + x05,  9);
		x08 ^= ROTATE(x11 + x10,  9);  x13 ^= ROTATE(x12 + x15,  9);

		x03 ^= ROTATE(x02 + x01, 13);  x04 ^= ROTATE(x07 + x06, 13);
		x09 ^= ROTATE(x08 + x11, 13);  x14 ^= ROTATE(x13 + x12, 13);

		x00 ^= ROTATE(x03 + x02, 18);  x05 ^= ROTATE(x04 + x07, 18);
		x10 ^= ROTATE(x09 + x08, 18);  x15 ^= ROTATE(x14 + x13, 18);
	}
 
	x00 = (B[4].x ^= (B[0].x += x00));
	x01 = (B[4].y ^= (B[0].y += x01));
	x02 = (B[4].z ^= (B[0].z += x02));
	x03 = (B[4].w ^= (B[0].w += x03));
	x04 = (B[5].x ^= (B[1].x += x04));
	x05 = (B[5].y ^= (B[1].y += x05));
	x06 = (B[5].z ^= (B[1].z += x06));
	x07 = (B[5].w ^= (B[1].w += x07));
	x08 = (B[6].x ^= (B[2].x += x08));
	x09 = (B[6].y ^= (B[2].y += x09));
	x10 = (B[6].z ^= (B[2].z += x10));
	x11 = (B[6].w ^= (B[2].w += x11));
	x12 = (B[7].x ^= (B[3].x += x12));
	x13 = (B[7].y ^= (B[3].y += x13));
	x14 = (B[7].z ^= (B[3].z += x14));
	x15 = (B[7].w ^= (B[3].w += x15));

    #pragma unroll
    for (i = 0; i < 4; i ++)
    {
		x04 ^= ROTATE(x00 + x12,  7);  x09 ^= ROTATE(x05 + x01,  7);
		x14 ^= ROTATE(x10 + x06,  7);  x03 ^= ROTATE(x15 + x11,  7);

		x08 ^= ROTATE(x04 + x00,  9);  x13 ^= ROTATE(x09 + x05,  9);
		x02 ^= ROTATE(x14 + x10,  9);  x07 ^= ROTATE(x03 + x15,  9);

		x12 ^= ROTATE(x08 + x04, 13);  x01 ^= ROTATE(x13 + x09, 13);
		x06 ^= ROTATE(x02 + x14, 13);  x11 ^= ROTATE(x07 + x03, 13);

		x00 ^= ROTATE(x12 + x08, 18);  x05 ^= ROTATE(x01 + x13, 18);
		x10 ^= ROTATE(x06 + x02, 18);  x15 ^= ROTATE(x11 + x07, 18);		

		x01 ^= ROTATE(x00 + x03,  7);  x06 ^= ROTATE(x05 + x04,  7);
		x11 ^= ROTATE(x10 + x09,  7);  x12 ^= ROTATE(x15 + x14,  7);

		x02 ^= ROTATE(x01 + x00,  9);  x07 ^= ROTATE(x06 + x05,  9);
		x08 ^= ROTATE(x11 + x10,  9);  x13 ^= ROTATE(x12 + x15,  9);

		x03 ^= ROTATE(x02 + x01, 13);  x04 ^= ROTATE(x07 + x06, 13);
		x09 ^= ROTATE(x08 + x11, 13);  x14 ^= ROTATE(x13 + x12, 13);

		x00 ^= ROTATE(x03 + x02, 18);  x05 ^= ROTATE(x04 + x07, 18);
		x10 ^= ROTATE(x09 + x08, 18);  x15 ^= ROTATE(x14 + x13, 18);
	}

    B[4].x += x00;
    B[4].y += x01;
    B[4].z += x02;
    B[4].w += x03;
    B[5].x += x04;
    B[5].y += x05;
    B[5].z += x06;
    B[5].w += x07;
    B[6].x += x08;
    B[6].y += x09;
    B[6].z += x10;
    B[6].w += x11;
    B[7].x += x12;
    B[7].y += x13;
    B[7].z += x14;
    B[7].w += x15;
}

void Swap(uint4* X)
{
    #pragma unroll
	for (int i = 0; i < 8; i++)
        X[i] = ByteReverse(X[i]);
}

void ScryptCore(uint4* X, __global uint4* V)
{
    int i;
    Swap(X);

    //#pragma unroll
	for ( i = 0; i < 1024; i++) 
    {
        V[(i * 8) + 0] = X[0];
        V[(i * 8) + 1] = X[1];
        V[(i * 8) + 2] = X[2];
        V[(i * 8) + 3] = X[3];
        V[(i * 8) + 4] = X[4];
        V[(i * 8) + 5] = X[5];
        V[(i * 8) + 6] = X[6];
        V[(i * 8) + 7] = X[7];
        salsa(X);
    }
    
    //#pragma unroll
	for (i = 0; i < 1024; i++) 
    {        
		uint j = 8 * (X[4].x & 0x3FF);
        #pragma unroll
		for (unsigned int k = 0; k < 8; k++)
        {
            uint4 vval = V[j + k];
            X[k] ^= vval;
        }
		salsa(X);
	}

    Swap(X);
}

void ScryptCoreHalf(uint4* X, __global uint4* V)
{
    int i;
    Swap(X);

	for ( i = 0; i < 512; i++) 
    {
        V[(i * 8) + 0] = X[0];
        V[(i * 8) + 1] = X[1];
        V[(i * 8) + 2] = X[2];
        V[(i * 8) + 3] = X[3];
        V[(i * 8) + 4] = X[4];
        V[(i * 8) + 5] = X[5];
        V[(i * 8) + 6] = X[6];
        V[(i * 8) + 7] = X[7];
        salsa(X);
        salsa(X);
    }
    
	for (i = 0; i < 1024; i++) 
    {
        uint whichBlock = (X[4].x & 0x3FF);
        uint storedBlock = whichBlock / 2;
		uint storedOffset = storedBlock * 8;

        // Get the stored block
        uint4 temp[8];
        #pragma unroll
        for( unsigned int k = 0; k < 8; k++ )
            temp[k] = V[storedOffset + k];

        // Salsa extra that we may need
        uint extra = (whichBlock & 1);
        for( unsigned int k = 0; k < extra; k++ )
            salsa(temp);

        #pragma unroll
		for (unsigned int k = 0; k < 8; k++)
            X[k] ^= temp[k];
		salsa(X);
	}
    Swap(X);
}

void ScryptCoreQuarter(uint4* X, __global uint4* V)
{
    int i;
    Swap(X);

	for ( i = 0; i < 256; i++) 
    {
        V[(i * 8) + 0] = X[0];
        V[(i * 8) + 1] = X[1];
        V[(i * 8) + 2] = X[2];
        V[(i * 8) + 3] = X[3];
        V[(i * 8) + 4] = X[4];
        V[(i * 8) + 5] = X[5];
        V[(i * 8) + 6] = X[6];
        V[(i * 8) + 7] = X[7];
        salsa(X);
        salsa(X);
        salsa(X);
        salsa(X);
    }
    
	for (i = 0; i < 1024; i++) 
    {
        uint whichBlock = (X[4].x & 0x3FF);
        uint storedBlock = whichBlock / 4;
		uint storedOffset = storedBlock * 8;

        // Get the stored block
        uint4 temp[8];
        #pragma unroll
        for( unsigned int k = 0; k < 8; k++ )
            temp[k] = V[storedOffset + k];

        // Salsa extra that we may need
        uint extra = (whichBlock & 3);
        for( unsigned int k = 0; k < extra; k++ )
            salsa(temp);

        #pragma unroll
		for (unsigned int k = 0; k < 8; k++)
            X[k] ^= temp[k];
		salsa(X);
	}
    Swap(X);
}

void ScryptCoreEighth(uint4* X, __global uint4* V)
{
    int i;
    Swap(X);

	for ( i = 0; i < 128; i++) 
    {
        V[(i * 8) + 0] = X[0];
        V[(i * 8) + 1] = X[1];
        V[(i * 8) + 2] = X[2];
        V[(i * 8) + 3] = X[3];
        V[(i * 8) + 4] = X[4];
        V[(i * 8) + 5] = X[5];
        V[(i * 8) + 6] = X[6];
        V[(i * 8) + 7] = X[7];
        salsa(X);
        salsa(X);
        salsa(X);
        salsa(X);
        salsa(X);
        salsa(X);
        salsa(X);
        salsa(X);
    }
    /*
	for (i = 0; i < 1024; i++) 
    {
        uint whichBlock = (X[4].x & 0x3FF);
        uint storedBlock = whichBlock / 8;
		uint storedOffset = storedBlock * 8;

        // Get the stored block
        uint4 temp[8];
        #pragma unroll
        for( unsigned int k = 0; k < 8; k++ )
            temp[k] = V[storedOffset + k];

        // Salsa extra that we may need
        uint extra = (whichBlock & 7);
        for( unsigned int k = 0; k < extra; k++ )
            salsa(temp);

        #pragma unroll
		for (unsigned int k = 0; k < 8; k++)
            X[k] ^= temp[k];
		salsa(X);
	}
    */
    Swap(X);
}

void ScryptCoreB(uint4* bp)
{
    int i;
    uint4 X[8];
    uint4 Base[8];

    #pragma unroll
	for (i = 0; i < 8; i++)
        Base[i] = X[i] = ByteReverse(bp[i]);

	for ( i = 0; i < 1024; i++) 
    {
        salsa(X);
    }
    
	for (i = 0; i < 1024; i++) 
    {        
		uint j = (X[4].x & 0x3FF);
        
        uint4 block[8];
        #pragma unroll
        for( unsigned int k = 0; k < 8; k++ )
            block[k] = Base[k];

        for( unsigned int k = 0; k < j; k++ )
            salsa(block);

        #pragma unroll
		for (unsigned int k = 0; k < 8; k++)
            X[k] ^= block[k];
		salsa(X);
	}

    #pragma unroll
    for (i = 0; i < 8; i++)
        bp[i] = ByteReverse(X[i]);
}

__kernel void ScryptHash(__global const uint4 *inputA, volatile __global uint*restrict output, __global uint4* VBuffer, const uint target) 
{
	uint gid = get_global_id(0);
    uint gsize = get_global_size(0);

    const uint4 staticHash0 = (uint4)(0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a);
    const uint4 staticHash1 = (uint4)(0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19);
    uint4 midstate[2];
    sha256Block(midstate, staticHash0, staticHash1, inputA[0], inputA[1], inputA[2], inputA[3]);
        
    uint4 nonced = inputA[4];
    nonced.w = gid;
    
    uint4 inner[2];
	uint4 outer[2];    
	sha256Block(inner, midstate[0], midstate[1], nonced, inputA[5], inputA[6], inputA[7]);   
    
    const uint4 c5c = (uint4)(0x5c5c5c5c,0x5c5c5c5c,0x5c5c5c5c,0x5c5c5c5c);    
    sha256Block(outer, staticHash0, staticHash1, inner[0] ^ c5c, inner[1] ^ c5c, c5c, c5c);

    const uint4 c36 = (uint4)(0x36363636,0x36363636,0x36363636,0x36363636);    
    sha256Block(inner, staticHash0, staticHash1, inner[0] ^ c36, inner[1] ^ c36, c36, c36);
    
    uint4 salted[2];
    sha256Block(salted, inner[0], inner[1], inputA[0], inputA[1], inputA[2], inputA[3]);
    
    uint4 tempHash[2];
    uint4 bp[8];
    #pragma unroll
    for( int i = 0; i < 4; i++ )
    {
        sha256Block(tempHash, salted[0], salted[1], nonced, (uint4)(i + 1, 0x80000000, 0, 0), (uint4)(0, 0, 0, 0), (uint4)(0, 0, 0, 0x000004A0));
        sha256Block(&bp[i * 2], outer[0], outer[1], tempHash[0], tempHash[1], (uint4)(0x80000000, 0, 0, 0), (uint4)(0, 0, 0, 0x00000300));
    }
    	
    //ScryptCore(bp, VBuffer + ((gid % gsize) * 8192));
    //ScryptCoreHalf(bp, VBuffer + ((gid % gsize) * 4096));
    //ScryptCoreQuarter(bp, VBuffer + ((gid % gsize) * 2048));
    ScryptCoreEighth(bp, VBuffer + ((gid % gsize) * 1024));
    
    sha256Block(salted, inner[0], inner[1], bp[0], bp[1], bp[2], bp[3]);
    sha256Block(salted, salted[0], salted[1], bp[4], bp[5], bp[6], bp[7]);
    
    sha256Block(tempHash, salted[0], salted[1], (uint4)(0x00000001, 0x80000000, 0, 0), (uint4)(0, 0, 0, 0), (uint4)(0, 0, 0, 0), (uint4)(0, 0, 0, 0x00000620));
    sha256Block(inner, outer[0], outer[1], tempHash[0], tempHash[1], (uint4)(0x80000000, 0, 0, 0), (uint4)(0, 0, 0, 0x00000300)); 
    
    
    uint4 test = ByteReverse(inner[1]);
    uint less = test.w <= target;
    output[gid % gsize] = less * nonced.w;
}