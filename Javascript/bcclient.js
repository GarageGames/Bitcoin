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
if (typeof StatusClient == 'undefined')
	StatusClient = false;

function ResetThreads()
{
	// Kill all active workers
	for (var i = 0; i < hashWorker.length; i++)
		hashWorker[i].terminate();
	hashWorker = [];

	// Get ShaWorkerCount
	if (StatusClient)
	{
		var myThreads = document.getElementById("mti");
		if( myThreads )
			ShaWorkerCount = myThreads.value;
	}

	// kill socket so it has to reconnect and get more work
	if( websocket && websocket.readyState != 3 )
		websocket.close();

	// Start new workers
	for (var i = 0; i < ShaWorkerCount; i++)
	{
		var w = new Worker(ShaWorkerPath);
		hashWorker[i] = w;
		hashWorker[i].onmessage = HashWorkerDone;
	}
}
ResetThreads();

var bestresult;
var threadsCompleted;
var hashCountCompleted;
var workStartTime;
var ShaHashRate;
function WorkDone(result, hashCount)
{
	if( result >= 0 )
		bestresult = result;
	hashCountCompleted += hashCount;
	threadsCompleted++;
	if (threadsCompleted == hashWorker.length)
	{
		var end = new Date().getTime();
		ShaHashRate = hashCountCompleted / ((end - workStartTime) / 1000);
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

function RequestClientInfo()
{
	var infoRequest = new Uint8Array(1);
	infoRequest[0] = 7;
	var blob = new Blob([infoRequest], { type: "application/octet-binary" });
	websocket.send(blob);
}

function SocketOpen()
{
	// Send identity packet
	var ar = new ArrayBuffer(6 + (navigator.userAgent.length + 1) + (navigator.platform.length + 1) + (window.location.href.length + 1));
	//var v = new Uint8Array(ar);
	var data = new DataView(ar, 0);
	data.setInt8(0, 1);
	if (StatusClient)
		data.setInt8(1, 0x80);
	else
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
		workStartTime = new Date().getTime();
		for (var i = 0; i < hashWorker.length; i++)
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
	else if (command == 6)
	{
		// Status command
		if (StatusClient)
		{
			var clientCount = document.getElementById("clientCount");
			var hashRate = document.getElementById("hashRate");
			var myHashrate = document.getElementById("myHashrate");

			var count = data.getUint32(1, true);
			var rate = data.getFloat64(5, true);

			clientCount.innerHTML = "Clients: " + count;
			hashRate.innerHTML = "Hashrate: " + Math.floor(rate).toLocaleString() + " / second";

			myHashrate.innerHTML = "My Hashrate: " + Math.floor(ShaHashRate).toLocaleString() + " / second";
		}
	}
	else if (command == 8 )
	{
		if( StatusClient )
		{
			var chars = new Uint8Array(this.result, 1);
			var jsonstr = String.fromCharCode.apply(null, chars);
			var clientList = JSON.parse(jsonstr);
						
			var table = document.getElementById("clientTable");
			if( table )
			{
				// Clear clients in the table
				while( table.rows.length > 1 )
					table.deleteRow(1);

				// Add clients from the client list
				for( var i = 0; i < clientList.clients.length; i++ )
				{
					var row = table.insertRow(1);
					var client = clientList.clients[i];

					row.insertCell(0).innerHTML = client.ipaddress;
					row.insertCell(1).innerHTML = client.state;
					row.insertCell(2).innerHTML = client.type;
					row.insertCell(3).innerHTML = client.hashrate;
					row.insertCell(4).innerHTML = client.lastSeen;
					row.insertCell(5).innerHTML = client.agent;
					row.insertCell(6).innerHTML = client.platform;
					row.insertCell(7).innerHTML = client.location;
				}
			}
		}
	}
	else
	{
		if (console)
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