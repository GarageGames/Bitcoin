<?php
	include('config.php');

	session_start();  
	$user_id=$_SESSION['login_id'];  
		
	if( $_GET )
	{
		$productID = $_GET['productId'];
		if( count($_GET) > 1 )
			$user_id=$_GET['memberId'];
	}
	else
	{
		$productID = $argv[1];
		if( $argc > 2 )
			$user_id=$argv[2];
	}

	$productsSql=mysql_query("select * from products where product_id='$productID' ");
	$row = mysql_fetch_array($productsSql);
	$productName= $row['product_name'];

	$sqlQuery = "select * from workdata where product_id='$productID' and member_id='$user_id'";
	$history = mysql_query($sqlQuery);
?>

<head>
	<script src="./jquery-2.1.0.min.js"></script>
	<script src="./RGraph/libraries/RGraph.common.core.js"></script>
	<script src="./RGraph/libraries/RGraph.line.js"/></script>
	<script>
		var productData = [<?php
							while( $row = mysql_fetch_array($history) )
							{
								$timestamp = strtotime($row['timestamp']);
								Print "{\"hr\":".$row['hashrate'].",\"t\":".$timestamp."},";
							}
						?>];
		var interval = (60 * 60);		// one hour
		var historyCount = 20;
		var now = Math.round(new Date().getTime() / 1000);

		function IntervalChange()
		{
			var sel = document.getElementById('intervalSel');

			interval = sel.options[sel.selectedIndex].value;
			DisplayData();
		}

		function DisplayData()
		{
			var canvas = document.getElementById('cvs2');
			canvas.width = canvas.width;
			var oldest = now - (interval * (historyCount + 1));

	        var first = 0;
	        for (var i = productData.length - 1; i >= 0; i--)
	        {
                if( productData[i].t < oldest )
				{
					first = i + 1;
					break;
				}
	        }
			
			var sums = [];
			var counts = [];
			for( var i = 0; i < historyCount; i++ )
			{
				sums[i] = 0;
				counts[i] = 0;
			}
			for( var i = first; i < productData.length; i++ )
			{
				var timeSince = now - productData[i].t;
				var index = Math.round(timeSince / interval);
				sums[index] += productData[i].hr;
				counts[index]++;
			}
			for( var i = 0; i < sums.length; i++ )
			{
				if( counts[i] > 0 )
					sums[i] /= counts[i];
			}

			var values = [];
			var idx = sums.length - 1;
			for( var i = 0; i < historyCount; i++, idx-- )
				values[i] = sums[idx];

			var line = new RGraph.Line('cvs2', values)
				.set('spline', false)
				.set('numxticks', 20)
				.set('numyticks', 5)
				.set('background.grid.autofit.numvlines', historyCount)
				.set('colors', ['red'])
				.set('linewidth', 5)
				.set('gutter.left', 80)
				.set('gutter.right', 15)
				//.set('labels',['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'])
				.set('shadow',true)
				.set('shadow.color','#aaa')
				.set('shadow.blur',5)
				.set('tickmarks',null)
				.draw();
		}

		window.onload = function ()		
		{
			IntervalChange();	
		}
	</script>
</head>
<body>
  <h1>Product: <?php echo $productName?></h1>  
  <div>
	<label>Interval</label>
	<select id="intervalSel" onChange=IntervalChange()>
		<option value=60>Minute</option>
		<option value=3600>Hour</option>
		<option value=86400>Day</option>
		<option value=604800>Week</option>
	</select>
	<br>
	<canvas id="cvs2" width="800" height="400">[No canvas support]</canvas>
  </div>
</body>