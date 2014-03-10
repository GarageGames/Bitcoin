var websocket;

function HashWorkerDone(evt)
{
	if (evt.data.done)
	{
		WorkDone(evt.data.done[0], evt.data.done[1]);
	}
}

var hashWorker = [];
var w = new Worker("./sha.js");
hashWorker[0] = w;
w = new Worker("./sha.js");
hashWorker[1] = w;
hashWorker[0].onmessage = HashWorkerDone;
hashWorker[1].onmessage = HashWorkerDone;

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
		console.log("Work Complete: " + result + " : " + hashCount);
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
	var ar = new ArrayBuffer(6);
	var data = new DataView(ar, 0);
	data.setInt8(0, 1);
	data.setInt8(1, 0);
	data.setUint32(2, 500000);

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