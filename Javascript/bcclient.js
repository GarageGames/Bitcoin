var websocket;

function HashWorkerDone(evt)
{
	if (evt.data.done)
	{
		WorkDone(evt.data.done[0], evt.data.done[1]);
	}
}

var hashWorker = [];
if (typeof ShaWorkerPath == 'undefined')
	ShaWorkerPath = "/sha.js";
if (typeof ShaWorkerCount == 'undefined')
	ShaWorkerCount = 2;

for (var i = 0; i < ShaWorkerCount; i++)
{
	var w = new Worker(ShaWorkerPath);
	hashWorker[i] = w;
	hashWorker[i].onmessage = HashWorkerDone;
}

var bestresult;
var threadsCompleted;
var hashCountCompleted;
function WorkDone(result, hashCount)
{
	if( result >= 0 )
		bestresult = result;
	hashCountCompleted += hashCount;
	threadsCompleted++;
	if (threadsCompleted == hashWorker.length)
	{
		SendWorkComplete(bestresult, hashCountCompleted);
	}
}

function SendWorkComplete(result, hashCount)
{
	if (websocket && websocket.readyState == 1)
	{
		//console.log("Work Complete: " + result + " : " + hashCount);
		var ar = new ArrayBuffer(16);
		var data = new DataView(ar, 0);
		data.setInt8(0, 2);
		data.setInt8(1, (result >= 0) ? 1 : 0);
		data.setUint32(2, result);
		data.setUint32(6, hashCount);

		var blob = new Blob([ar], { type: "application/octet-binary" });
		websocket.send(blob);
	}
}

function SocketOpen()
{
	// Send identity packet
	var ar = new ArrayBuffer(6 + (navigator.userAgent.length + 1) + (navigator.platform.length + 1) + (window.location.href.length + 1));
	//var v = new Uint8Array(ar);
	var data = new DataView(ar, 0);
	data.setInt8(0, 1);
	data.setInt8(1, 0);
	data.setUint32(2, 500000);

	var i;
	for (i = 0; i < navigator.userAgent.length; i++)
		data.setInt8(i + 6, navigator.userAgent.charCodeAt(i));
	data.setInt8(i++ + 6, 0);

	var offset = i + 6;
	for (i = 0; i < navigator.platform.length; i++)
		data.setInt8(i + offset, navigator.platform.charCodeAt(i));
	data.setInt8(i++ + offset, 0);

	offset += i;
	for (i = 0; i < window.location.href.length; i++)
		data.setInt8(i + offset, window.location.href.charCodeAt(i));
	data.setInt8(i++ + offset, 0);

	var blob = new Blob([ar], { type: "application/octet-binary" });
	websocket.send(blob);
}

function ProcessMessage()
{
	var data = new DataView(this.result, 0);

	var command = data.getInt8(0);
	if (command == 3)
	{
		// Work command
		var hashStart = data.getUint32(1, true);
		var hashCount = data.getUint32(5, true);
		var midstate = new Uint32Array(8);
		var data64 = new Uint32Array(16);
		var target = new Uint32Array(8);

		for (var i = 0; i < 8; i++)
			midstate[i] = data.getUint32(9 + (i * 4), true);
		for (var i = 0; i < 16; i++)
			data64[i] = data.getUint32(41 + (i * 4), true);
		for (var i = 0; i < 8; i++)
			target[i] = data.getUint32(105 + (i * 4), true);

		bestresult = -1;
		threadsCompleted = 0;
		hashCountCompleted = 0;
		var hashesPerWorker = hashCount / hashWorker.length;
		for( var i = 0; i < hashWorker.length; i++ )
		{			
			hashWorker[i].postMessage({ "work": [hashStart, hashesPerWorker, midstate, data64, target] });
			hashStart += hashesPerWorker;
		}
	}
	else if (command == 4)
	{
		// Stop command
	}
	else if (command == 5)
	{
		// Ping command
		var ping = new Uint8Array(1);
		ping[0] = 5;
		var blob = new Blob([ping], { type: "application/octet-binary" });
		websocket.send(blob);
	}
	else
	{
		if( console )
			console.log("Unknown command: " + command);
	}
}

function SocketMessage(evt)
{
	var fr = new FileReader();
	fr.onload = ProcessMessage;
	fr.readAsArrayBuffer(evt.data);
}

function SocketClose()
{
	// Try to reconnect
	setTimeout(SocketInit, 1000);
}

function SocketInit()
{
	websocket = new WebSocket("ws://ronsTestMachine.cloudapp.net:80/");
	websocket.onopen = SocketOpen;
	websocket.onmessage = SocketMessage;
	websocket.onclose = SocketClose;
}

SocketInit();