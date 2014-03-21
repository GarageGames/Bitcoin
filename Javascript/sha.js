var hashStart;
var hashCount;
var midstate;
var data64;
var hash1;
var target;
var dataFull;
var workResult;


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

function sha256_block(output, state, input)
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

var staticHash = [0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19];

function DoHashes(n, count, midstate, data, target)
{
	var hash1 = [0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000100];	
	var state = new Uint32Array(16);
	var end = n + count;
	for (n; n < end; n++)
	{
		data[3] = swap32(n);
		sha256_block(hash1, midstate, data);
		sha256_block(state, staticHash, hash1);

		if (state[7] == 0)
		{
			//console.log("state: " + state[0] + ", " + state[1] + ", " + state[2] + ", " + state[3] + ", " + state[4] + ", " + state[5] + ", " + state[6] + ", " + state[7]);
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


function xor_salsa8(B, Bx)
{
	var x00,x01,x02,x03,x04,x05,x06,x07,x08,x09,x10,x11,x12,x13,x14,x15;
	var i;

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
		x04 ^= ROTATE(x00 + x12,  7);  x09 ^= ROTATE(x05 + x01,  7);
		x14 ^= ROTATE(x10 + x06,  7);  x03 ^= ROTATE(x15 + x11,  7);

		x08 ^= ROTATE(x04 + x00,  9);  x13 ^= ROTATE(x09 + x05,  9);
		x02 ^= ROTATE(x14 + x10,  9);  x07 ^= ROTATE(x03 + x15,  9);

		x12 ^= ROTATE(x08 + x04, 13);  x01 ^= ROTATE(x13 + x09, 13);
		x06 ^= ROTATE(x02 + x14, 13);  x11 ^= ROTATE(x07 + x03, 13);

		x00 ^= ROTATE(x12 + x08, 18);  x05 ^= ROTATE(x01 + x13, 18);
		x10 ^= ROTATE(x06 + x02, 18);  x15 ^= ROTATE(x11 + x07, 18);

		/* Operate on rows. */
		x01 ^= ROTATE(x00 + x03,  7);  x06 ^= ROTATE(x05 + x04,  7);
		x11 ^= ROTATE(x10 + x09,  7);  x12 ^= ROTATE(x15 + x14,  7);

		x02 ^= ROTATE(x01 + x00,  9);  x07 ^= ROTATE(x06 + x05,  9);
		x08 ^= ROTATE(x11 + x10,  9);  x13 ^= ROTATE(x12 + x15,  9);

		x03 ^= ROTATE(x02 + x01, 13);  x04 ^= ROTATE(x07 + x06, 13);
		x09 ^= ROTATE(x08 + x11, 13);  x14 ^= ROTATE(x13 + x12, 13);

		x00 ^= ROTATE(x03 + x02, 18);  x05 ^= ROTATE(x04 + x07, 18);
		x10 ^= ROTATE(x09 + x08, 18);  x15 ^= ROTATE(x14 + x13, 18);
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

function XorScramble(bp)
{
	var Xbuff = new ArrayBuffer(128);
	var X = new Uint32Array(Xbuff, 0);
	var Xb = new Uint32Array(Xbuff, 64);
	var V = new ArrayBuffer(131072);
	var VI = new Uint32Array(V);

	for (var i = 0; i < 32; i++)
        X[i] = swap32(bp[i]);

	for (var i = 0; i < 1024; i++) 
	{
		var vview = new Uint32Array(V, i * 128);
		for( var j = 0; j < 32; j++ )
			vview[j] = X[j];

		xor_salsa8(X, Xb);
		xor_salsa8(Xb, X);
	}
	for (var i = 0; i < 1024; i++) 
	{
		var j = 32 * (X[16] & 1023);
		for (var k = 0; k < 32; k++)
		{
			var v = ((j + k) & 0xFFFFFFFF) >>> 0;
			X[k] ^= VI[v];
		}
		xor_salsa8(X, Xb);
		xor_salsa8(Xb, X);
	}

    for (var i = 0; i < 32; i++)
        bp[i] = swap32(X[i]);
}

var pad36 = [0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x36363636, 0x36363636, 0x36363636, 0x36363636, 0x36363636, 0x36363636, 0x36363636, 0x36363636];	
var pad5c = [0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c, 0x5c5c5c5c];
var dataBuffer2 = [0x00000001, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000620];	
var tempHash = [0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000300];
function scrypt(inputA, inputB, inputB2, output)
{
	var inner = new Uint32Array(8);
    var outer = new Uint32Array(8);
    sha256_block(inner, staticHash, inputA);
    sha256_block(inner, inner, inputB);
    	
    for( var i = 0; i < 8; i++ )
    {
        pad36[i] = inner[i] ^ 0x36363636;
        pad5c[i] = inner[i] ^ 0x5c5c5c5c;
    }
    sha256_block(inner, staticHash, pad36);
    sha256_block(outer, staticHash, pad5c);

    var salted = new Uint32Array(8);
    sha256_block(salted, inner, inputA);

    var bpArr = new ArrayBuffer(128);
    var bp = new Uint32Array(bpArr);
    for( var i = 0; i < 4; i++ )
    {
        inputB2[4] = (i + 1);
		var bpx = new Uint32Array(bpArr, i * 32);

        sha256_block(tempHash, salted, inputB2);
        sha256_block(bpx, outer, tempHash);
    }
	
	XorScramble(bp);
    
    sha256_block(salted, inner, bp);
	bp = new Uint32Array(bpArr, 64);
    sha256_block(salted, salted, bp);

    sha256_block(tempHash, salted, dataBuffer2);
    sha256_block(output, outer, tempHash);
}

function TestScrypt()
{
	var inputA = [0x00000002, 0x2a34cf18, 0xf3e954a3, 0x76d84bfb, 0x9665b4f9, 0x602e7fc0, 0xa90a934b, 0x9aa31b1a, 0xbf7b9d63, 0xade27f84, 0xf9d067e7, 0x0428d2e7, 0x2d58405f, 0xdb42626f, 0xe166e0d2, 0x45efaae4];
	var inputB = [0x6390dbd9, 0x532A27ED, 0x1c011b0d, 0x6BB20500, 0x00000080, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x80020000];
	var inputB2 = [0x6390dbd9, 0x532A27ED, 0x1c011b0d, 0x6BB20500, 0x00000080, 0x00000080, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0xA0040000];
	var target =  [0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x000d1b01, 0x00000000];
	var output = new Uint32Array(8);

	for (var i = 0; i < 16; i++)
	{
		inputA[i] = swap32(inputA[i]);
		inputB[i] = swap32(inputB[i]);
		inputB2[i] = swap32(inputB2[i]);

		if( i < 8 )
			target[i] = swap32(target[i]);
	}
	
	console.log("working\n");
	var start = new Date().getTime();
	var startSeed = 373355;
	var nonce = startSeed;

	while (true)
	{
		inputB[3] = nonce;
		inputB2[3] = nonce;

		scrypt(inputA, inputB, inputB2, output);

		if (output[7] == 0)
		{
			for (var i = 6; i >= 0; i++)
			{
				var swapped = swap32(output[i]);
				if (swapped > target[i])
					break;
				if (swapped < target[i])
				{
					var end = new Date().getTime();
					var seconds = ((end - start) / 1000);
					var hashes = nonce - startSeed;
					var hashesPerSecond = hashes / seconds;
					console.log("hashes: " + hashes + ", seconds: " + seconds + ", hashrate: " + Math.floor(hashesPerSecond).toLocaleString());
					return;
				}
			}
		}

		nonce++;
	}
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
	//console.log(numHashes + " hashes took " + (end - start) + " milliseconds");
	//console.log(hashesPerSecond + "hashes per second");

	var secondsTotal = 4294967296 / hashesPerSecond;
	//console.log(secondsTotal + " seconds for the entire hash space");
}

function DoScrypt(start, count, dataA, dataB, tgt)
{
	var inputB2 = [0x6390dbd9, 0x532A27ED, 0x1c011b0d, 0x6BB20500, 0x00000080, 0x80000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x000004A0];
	inputB2[0] = dataB[0];
	inputB2[1] = dataB[1];
	inputB2[2] = dataB[2];
	inputB2[3] = dataB[3];
	var output = new Uint32Array(8);

	var end = start + count;
	for( var i = start; i < end; i++ )
	{
		dataB[3] = i;
		inputB2[3] = i;

		scrypt(dataA, dataB, inputB2, output);
		if (output[7] == 0)
		{
			console.log("found zero @ " + i);
			for (var j = 6; j >= 0; i++)
			{
				var swapped = swap32(output[j]);
				if (swapped > tgt[j])
					break;
				if (swapped < tgt[j])
				{
					hashCount = ((i + 1) - start);
					console.log("Soultion found: " + i + " hashes: " + hashCount);
					return i;
				}
			}
		}
	} 
	return -1;
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
		dataFull = event.data.work[5];
		var currency = event.data.work[6];

		switch( currency )
		{
			case 0:	
				workResult = DoHashes(hashStart, hashCount, midstate, data64, target);
				break;
			default:
				workResult = DoScrypt(hashStart, hashCount, dataFull, data64, target);
				break;
		}

		postMessage({ "done": [workResult, hashCount] });
	}

};