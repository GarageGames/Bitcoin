var hashStart;
var hashCount;
var midstate;
var data64;
var hash1;
var target;
var workResult;



function ror32(word, shift)
{
	var val = (word >>> shift) | ((word << (32 - shift)) & 0xFFFFFFFF);
	return val;
}

function Ch(x, y, z)
{
	var val = z ^ (x & (y ^ z));
	return val;
}

function Maj(x, y, z)
{
	var val = (x & y) | (z & (x | y));
	return val;
}

function e0(x)
{
	var val = (ror32(x, 2) ^ ror32(x, 13) ^ ror32(x, 22));
	return val;
}

function e1(x)
{
	var val = (ror32(x, 6) ^ ror32(x, 11) ^ ror32(x, 25));
	return val;
}

function s0(x)
{
	var val = (ror32(x, 7) ^ ror32(x, 18) ^ (x >>> 3));
	return val;
}

function s1(x)
{
	var val = (ror32(x, 17) ^ ror32(x, 19) ^ (x >>> 10));
	return val;
}

function shaTransform(output, state, input)
{
	var W = new Array(64);
	for (var i = 0; i < 16; i++)
		W[i] = input[i];
	for (var i = 16; i < 64; i++)
	{
		var a = s1(W[i-2]);
		var b = s0(W[i-15]);

		W[i] = a;
		W[i] += W[i - 7];
		W[i] += b;
		W[i] += W[i - 16];
		W[i] = (W[i] & 0xFFFFFFFF) >>> 0;
		//W[i] = s1(W[i - 2]) + W[i - 7] + s0(W[i - 15]) + W[i - 16];
	}

	var t1;
	var t2;

	// load the state into our registers
	var a = state[0];
	var b = state[1];
	var c = state[2]; 
	var d = state[3];
	var e = state[4];
	var f = state[5];
	var g = state[6]; 
	var h = state[7];

	// now iterate
	t1 = h + e1(e) + Ch(e, f, g) + 0x428a2f98 + W[0];
	t2 = e0(a) + Maj(a, b, c); d += t1; h = t1 + t2;
	t1 = g + e1(d) + Ch(d, e, f) + 0x71374491 + W[1];
	t2 = e0(h) + Maj(h, a, b); c += t1; g = t1 + t2;
	t1 = f + e1(c) + Ch(c, d, e) + 0xb5c0fbcf + W[2];
	t2 = e0(g) + Maj(g, h, a); b += t1; f = t1 + t2;
	t1 = e + e1(b) + Ch(b, c, d) + 0xe9b5dba5 + W[3];
	t2 = e0(f) + Maj(f, g, h); a += t1; e = t1 + t2;
	t1 = d + e1(a) + Ch(a, b, c) + 0x3956c25b + W[4];
	t2 = e0(e) + Maj(e, f, g); h += t1; d = t1 + t2;
	t1 = c + e1(h) + Ch(h, a, b) + 0x59f111f1 + W[5];
	t2 = e0(d) + Maj(d, e, f); g += t1; c = t1 + t2;
	t1 = b + e1(g) + Ch(g, h, a) + 0x923f82a4 + W[6];
	t2 = e0(c) + Maj(c, d, e); f += t1; b = t1 + t2;
	t1 = a + e1(f) + Ch(f, g, h) + 0xab1c5ed5 + W[7];
	t2 = e0(b) + Maj(b, c, d); e += t1; a = t1 + t2;

	t1 = h + e1(e) + Ch(e, f, g) + 0xd807aa98 + W[8];
	t2 = e0(a) + Maj(a, b, c); d += t1; h = t1 + t2;
	t1 = g + e1(d) + Ch(d, e, f) + 0x12835b01 + W[9];
	t2 = e0(h) + Maj(h, a, b); c += t1; g = t1 + t2;
	t1 = f + e1(c) + Ch(c, d, e) + 0x243185be + W[10];
	t2 = e0(g) + Maj(g, h, a); b += t1; f = t1 + t2;
	t1 = e + e1(b) + Ch(b, c, d) + 0x550c7dc3 + W[11];
	t2 = e0(f) + Maj(f, g, h); a += t1; e = t1 + t2;
	t1 = d + e1(a) + Ch(a, b, c) + 0x72be5d74 + W[12];
	t2 = e0(e) + Maj(e, f, g); h += t1; d = t1 + t2;
	t1 = c + e1(h) + Ch(h, a, b) + 0x80deb1fe + W[13];
	t2 = e0(d) + Maj(d, e, f); g += t1; c = t1 + t2;
	t1 = b + e1(g) + Ch(g, h, a) + 0x9bdc06a7 + W[14];
	t2 = e0(c) + Maj(c, d, e); f += t1; b = t1 + t2;
	t1 = a + e1(f) + Ch(f, g, h) + 0xc19bf174 + W[15];
	t2 = e0(b) + Maj(b, c, d); e += t1; a = t1 + t2;

	t1 = h + e1(e) + Ch(e, f, g) + 0xe49b69c1 + W[16];
	t2 = e0(a) + Maj(a, b, c); d += t1; h = t1 + t2;
	t1 = g + e1(d) + Ch(d, e, f) + 0xefbe4786 + W[17];
	t2 = e0(h) + Maj(h, a, b); c += t1; g = t1 + t2;
	t1 = f + e1(c) + Ch(c, d, e) + 0x0fc19dc6 + W[18];
	t2 = e0(g) + Maj(g, h, a); b += t1; f = t1 + t2;
	t1 = e + e1(b) + Ch(b, c, d) + 0x240ca1cc + W[19];
	t2 = e0(f) + Maj(f, g, h); a += t1; e = t1 + t2;
	t1 = d + e1(a) + Ch(a, b, c) + 0x2de92c6f + W[20];
	t2 = e0(e) + Maj(e, f, g); h += t1; d = t1 + t2;
	t1 = c + e1(h) + Ch(h, a, b) + 0x4a7484aa + W[21];
	t2 = e0(d) + Maj(d, e, f); g += t1; c = t1 + t2;
	t1 = b + e1(g) + Ch(g, h, a) + 0x5cb0a9dc + W[22];
	t2 = e0(c) + Maj(c, d, e); f += t1; b = t1 + t2;
	t1 = a + e1(f) + Ch(f, g, h) + 0x76f988da + W[23];
	t2 = e0(b) + Maj(b, c, d); e += t1; a = t1 + t2;

	t1 = h + e1(e) + Ch(e, f, g) + 0x983e5152 + W[24];
	t2 = e0(a) + Maj(a, b, c); d += t1; h = t1 + t2;
	t1 = g + e1(d) + Ch(d, e, f) + 0xa831c66d + W[25];
	t2 = e0(h) + Maj(h, a, b); c += t1; g = t1 + t2;
	t1 = f + e1(c) + Ch(c, d, e) + 0xb00327c8 + W[26];
	t2 = e0(g) + Maj(g, h, a); b += t1; f = t1 + t2;
	t1 = e + e1(b) + Ch(b, c, d) + 0xbf597fc7 + W[27];
	t2 = e0(f) + Maj(f, g, h); a += t1; e = t1 + t2;
	t1 = d + e1(a) + Ch(a, b, c) + 0xc6e00bf3 + W[28];
	t2 = e0(e) + Maj(e, f, g); h += t1; d = t1 + t2;
	t1 = c + e1(h) + Ch(h, a, b) + 0xd5a79147 + W[29];
	t2 = e0(d) + Maj(d, e, f); g += t1; c = t1 + t2;
	t1 = b + e1(g) + Ch(g, h, a) + 0x06ca6351 + W[30];
	t2 = e0(c) + Maj(c, d, e); f += t1; b = t1 + t2;
	t1 = a + e1(f) + Ch(f, g, h) + 0x14292967 + W[31];
	t2 = e0(b) + Maj(b, c, d); e += t1; a = t1 + t2;

	t1 = h + e1(e) + Ch(e, f, g) + 0x27b70a85 + W[32];
	t2 = e0(a) + Maj(a, b, c); d += t1; h = t1 + t2;
	t1 = g + e1(d) + Ch(d, e, f) + 0x2e1b2138 + W[33];
	t2 = e0(h) + Maj(h, a, b); c += t1; g = t1 + t2;
	t1 = f + e1(c) + Ch(c, d, e) + 0x4d2c6dfc + W[34];
	t2 = e0(g) + Maj(g, h, a); b += t1; f = t1 + t2;
	t1 = e + e1(b) + Ch(b, c, d) + 0x53380d13 + W[35];
	t2 = e0(f) + Maj(f, g, h); a += t1; e = t1 + t2;
	t1 = d + e1(a) + Ch(a, b, c) + 0x650a7354 + W[36];
	t2 = e0(e) + Maj(e, f, g); h += t1; d = t1 + t2;
	t1 = c + e1(h) + Ch(h, a, b) + 0x766a0abb + W[37];
	t2 = e0(d) + Maj(d, e, f); g += t1; c = t1 + t2;
	t1 = b + e1(g) + Ch(g, h, a) + 0x81c2c92e + W[38];
	t2 = e0(c) + Maj(c, d, e); f += t1; b = t1 + t2;
	t1 = a + e1(f) + Ch(f, g, h) + 0x92722c85 + W[39];
	t2 = e0(b) + Maj(b, c, d); e += t1; a = t1 + t2;

	t1 = h + e1(e) + Ch(e, f, g) + 0xa2bfe8a1 + W[40];
	t2 = e0(a) + Maj(a, b, c); d += t1; h = t1 + t2;
	t1 = g + e1(d) + Ch(d, e, f) + 0xa81a664b + W[41];
	t2 = e0(h) + Maj(h, a, b); c += t1; g = t1 + t2;
	t1 = f + e1(c) + Ch(c, d, e) + 0xc24b8b70 + W[42];
	t2 = e0(g) + Maj(g, h, a); b += t1; f = t1 + t2;
	t1 = e + e1(b) + Ch(b, c, d) + 0xc76c51a3 + W[43];
	t2 = e0(f) + Maj(f, g, h); a += t1; e = t1 + t2;
	t1 = d + e1(a) + Ch(a, b, c) + 0xd192e819 + W[44];
	t2 = e0(e) + Maj(e, f, g); h += t1; d = t1 + t2;
	t1 = c + e1(h) + Ch(h, a, b) + 0xd6990624 + W[45];
	t2 = e0(d) + Maj(d, e, f); g += t1; c = t1 + t2;
	t1 = b + e1(g) + Ch(g, h, a) + 0xf40e3585 + W[46];
	t2 = e0(c) + Maj(c, d, e); f += t1; b = t1 + t2;
	t1 = a + e1(f) + Ch(f, g, h) + 0x106aa070 + W[47];
	t2 = e0(b) + Maj(b, c, d); e += t1; a = t1 + t2;

	t1 = h + e1(e) + Ch(e, f, g) + 0x19a4c116 + W[48];
	t2 = e0(a) + Maj(a, b, c); d += t1; h = t1 + t2;
	t1 = g + e1(d) + Ch(d, e, f) + 0x1e376c08 + W[49];
	t2 = e0(h) + Maj(h, a, b); c += t1; g = t1 + t2;
	t1 = f + e1(c) + Ch(c, d, e) + 0x2748774c + W[50];
	t2 = e0(g) + Maj(g, h, a); b += t1; f = t1 + t2;
	t1 = e + e1(b) + Ch(b, c, d) + 0x34b0bcb5 + W[51];
	t2 = e0(f) + Maj(f, g, h); a += t1; e = t1 + t2;
	t1 = d + e1(a) + Ch(a, b, c) + 0x391c0cb3 + W[52];
	t2 = e0(e) + Maj(e, f, g); h += t1; d = t1 + t2;
	t1 = c + e1(h) + Ch(h, a, b) + 0x4ed8aa4a + W[53];
	t2 = e0(d) + Maj(d, e, f); g += t1; c = t1 + t2;
	t1 = b + e1(g) + Ch(g, h, a) + 0x5b9cca4f + W[54];
	t2 = e0(c) + Maj(c, d, e); f += t1; b = t1 + t2;
	t1 = a + e1(f) + Ch(f, g, h) + 0x682e6ff3 + W[55];
	t2 = e0(b) + Maj(b, c, d); e += t1; a = t1 + t2;

	t1 = h + e1(e) + Ch(e, f, g) + 0x748f82ee + W[56];
	t2 = e0(a) + Maj(a, b, c); d += t1; h = t1 + t2;
	t1 = g + e1(d) + Ch(d, e, f) + 0x78a5636f + W[57];
	t2 = e0(h) + Maj(h, a, b); c += t1; g = t1 + t2;
	t1 = f + e1(c) + Ch(c, d, e) + 0x84c87814 + W[58];
	t2 = e0(g) + Maj(g, h, a); b += t1; f = t1 + t2;
	t1 = e + e1(b) + Ch(b, c, d) + 0x8cc70208 + W[59];
	t2 = e0(f) + Maj(f, g, h); a += t1; e = t1 + t2;
	t1 = d + e1(a) + Ch(a, b, c) + 0x90befffa + W[60];
	t2 = e0(e) + Maj(e, f, g); h += t1; d = t1 + t2;
	t1 = c + e1(h) + Ch(h, a, b) + 0xa4506ceb + W[61];
	t2 = e0(d) + Maj(d, e, f); g += t1; c = t1 + t2;
	t1 = b + e1(g) + Ch(g, h, a) + 0xbef9a3f7 + W[62];
	t2 = e0(c) + Maj(c, d, e); f += t1; b = t1 + t2;
	t1 = a + e1(f) + Ch(f, g, h) + 0xc67178f2 + W[63];
	t2 = e0(b) + Maj(b, c, d); e += t1; a = t1 + t2;

	//state[0] += a; state[1] += b; state[2] += c; state[3] += d;
	//state[4] += e; state[5] += f; state[6] += g; state[7] += h;
	//var alpha = [a, b, c, d, e, f, g, h];

	//for (var i = 0; i < 8; i++)
	//{
		//state[i] = ((state[i] & 0xFFFFFFFF) >>> 0);
	//	output[i] = (((state[i] + alpha[i]) & 0xFFFFFFFF) >>> 0);
	//}
	output[0] = (((state[0] + a) & 0xFFFFFFFF) >>> 0);
	output[1] = (((state[1] + b) & 0xFFFFFFFF) >>> 0);
	output[2] = (((state[2] + c) & 0xFFFFFFFF) >>> 0);
	output[3] = (((state[3] + d) & 0xFFFFFFFF) >>> 0);
	output[4] = (((state[4] + e) & 0xFFFFFFFF) >>> 0);
	output[5] = (((state[5] + f) & 0xFFFFFFFF) >>> 0);
	output[6] = (((state[6] + g) & 0xFFFFFFFF) >>> 0);
	output[7] = (((state[7] + h) & 0xFFFFFFFF) >>> 0);
}

function ROTATE(a,n)     
{
	var val = (((a)<<(n))|(((a)&0xffffffff)>>>(32-(n))));
	return val;
}

function Sigma0(x)
{
	var val = (ROTATE((x),30) ^ ROTATE((x),19) ^ ROTATE((x),10));
	return val;
}

function Sigma1(x)
{
	var val = (ROTATE((x),26) ^ ROTATE((x),21) ^ ROTATE((x),7));
	return val;
}

function sigma0(x)
{
	var val = (ROTATE((x),25) ^ ROTATE((x),14) ^ ((x)>>>3));
	return val;
}

function sigma1(x)
{
	var val = (ROTATE((x),15) ^ ROTATE((x),13) ^ ((x)>>>10));
	return val;
}

function Ch(x,y,z)
{
	var val = (((x) & (y)) ^ ((~(x)) & (z)));
	return val;
}

function Maj(x,y,z)
{
	var val = (((x) & (y)) ^ ((x) & (z)) ^ ((y) & (z)));
	return val;
}

var K256 = [ 0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5, 0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5, 0xd807aa98,0x12835b01,0x243185be,0x550c7dc3, 0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174, 0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc, 0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da, 0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7, 0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967, 0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13, 0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85, 0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3, 0xd192e819,0xd6990624,0xf40e3585,0x106aa070, 0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5, 0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3, 0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208, 0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2 ];

function sha256_block_data_order(output, state, input)
{
	var s0,s1;
	var X = new Uint32Array(16);
	var i;
		
	var a = state[0];
	var b = state[1];		
	var c = state[2];
	var d = state[3];
	var e = state[4];
	var f = state[5];
	var g = state[6];
	var h = state[7];

	for (i=0;i<16;i++)
	{ 
		var T1 = X[i] = input[i];
		T1 += h + Sigma1(e) + Ch(e, f, g) + K256[i];
		T1 = (T1 & 0xFFFFFFFF) >>> 0;
		var T2 = Sigma0(a) + Maj(a, b, c);
		T2 = (T2 & 0xFFFFFFFF) >>> 0;
		h = g; g = f; f = e; e = d + T1;
		e = (e & 0xFFFFFFFF) >>> 0;
		d = c; c = b; b = a; a = T1 + T2;
		a = (a & 0xFFFFFFFF) >>> 0;
	}

	for (;i<64;i++)
	{
	    var s0 = X[(i+1)&0x0f];	
		s0 = sigma0(s0);
	    var s1 = X[(i+14)&0x0f];	
		s1 = sigma1(s1);

		X[i & 0xf] += s0 + s1 + X[(i + 9) & 0xf];
		X[i & 0xf] = (X[i & 0xf] & 0xFFFFFFFF) >>> 0;
		var T1 = X[i & 0xf];
		T1 += h + Sigma1(e) + Ch(e, f, g) + K256[i];
		T1 = (T1 & 0xFFFFFFFF) >>> 0;

		var T2 = Sigma0(a) + Maj(a, b, c);
		T2 = (T2 & 0xFFFFFFFF) >>> 0;
		h = g; g = f; f = e; e = d + T1;
		e = (e & 0xFFFFFFFF) >>> 0;
		d = c; c = b; b = a; a = T1 + T2;
		a = (a & 0xFFFFFFFF) >>> 0;
	}

	output[0] = (((state[0] + a) & 0xFFFFFFFF) >>> 0);
	output[1] = (((state[1] + b) & 0xFFFFFFFF) >>> 0);
	output[2] = (((state[2] + c) & 0xFFFFFFFF) >>> 0);
	output[3] = (((state[3] + d) & 0xFFFFFFFF) >>> 0);
	output[4] = (((state[4] + e) & 0xFFFFFFFF) >>> 0);
	output[5] = (((state[5] + f) & 0xFFFFFFFF) >>> 0);
	output[6] = (((state[6] + g) & 0xFFFFFFFF) >>> 0);
	output[7] = (((state[7] + h) & 0xFFFFFFFF) >>> 0);
}

function swap32(val) 
{
	var v = ((val & 0xFF) << 24) | ((val & 0xFF00) << 8) | ((val >>> 8) & 0xFF00) | ((val >>> 24) & 0xFF);
	v = (v & 0xFFFFFFFF) >>> 0;
	return v;
}

function DoHashes(n, count, midstate, data, target)
{
	var hash1 = [0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000100];
	var staticHash = [0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19];
	var state = new Uint32Array(16);
	var end = n + count;
	for (n; n < end; n++)
	{
		data[3] = swap32(n);
		sha256_block_data_order(hash1, midstate, data);
		sha256_block_data_order(state, staticHash, hash1);

		if (state[7] == 0)
		{
			console.log("state: " + state[0] + ", " + state[1] + ", " + state[2] + ", " + state[3] + ", " + state[4] + ", " + state[5] + ", " + state[6] + ", " + state[7]);
			for (var i = 6; i >= 0; i--)
			{
				var hashval = swap32(state[i]);
				if (hashval > target[i])
					break;
				if (hashval < target[i])
				{
					return n;
				}
			}
		}
	}

	return -1;
}

function TestHashes()
{
	var midstate = [0xbc909a33, 0x6358bff0, 0x90ccac7d, 0x1e59caa8, 0xc3c8d8e9, 0x4f0103c8, 0x96b18736, 0x4719f91b];
	//var data = [0x4a5e1e4b, 0x495fab29, 0x1d00ffff, 0x7c2bac1d, 0x00000080, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80020000];
	var data = [0x4b1e5e4a, 0x29ab5f49, 0xffff001d, 0x1dac2b7c, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000280];
	var hash1 = [0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000100];
	var target = [0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0xffff0000, 0x00000000];
		
	var start = new Date().getTime();

	var numHashes = 1;
	var n = DoHashes(2083236893, numHashes, midstate, data, target);

	var end = new Date().getTime();
	var hashesPerSecond = numHashes / ((end - start) / 1000);
	console.log(numHashes + " hashes took " + (end - start) + " milliseconds");
	console.log(hashesPerSecond + "hashes per second");

	var secondsTotal = 4294967296 / hashesPerSecond;
	console.log(secondsTotal + " seconds for the entire hash space");
}

function DoWork()
{
	var start = new Date().getTime();

	workResult = DoHashes(hashStart, hashCount, midstate, data64, target);

	var end = new Date().getTime();
	var hashesPerSecond = hashCount / ((end - start) / 1000);
	//console.log(hashCount + " hashes took " + (end - start) + " milliseconds");
	//console.log(hashesPerSecond + "hashes per second");

}

onmessage = function (event)
{
	if (event.data.work)
	{
		hashStart = event.data.work[0];
		hashCount = event.data.work[1];
		midstate = event.data.work[2];
		data64 = event.data.work[3];
		target = event.data.work[4];
		console.log("DoWork: " + hashStart + ", " + hashCount);
		DoWork();
		postMessage({ "done": [workResult, hashCount] });
	}

};