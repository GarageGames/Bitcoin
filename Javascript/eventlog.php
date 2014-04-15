<?php  
  include('config.php');
  
  $sqlQuery = "select * from events";
  $events = mysql_query($sqlQuery);
?>
<head>
	<style type="text/css">
        /* tables */
        table.tablesorter {
	        font-family:arial;
	        background-color: #CDCDCD;
	        margin:10px 0pt 15px;
	        font-size: 8pt;
	        width: 100%;
	        text-align: left;
        }
        table.tablesorter thead tr th, table.tablesorter tfoot tr th {
	        background-color: #e6EEEE;
	        border: 1px solid #FFF;
	        font-size: 8pt;
	        padding: 4px;
        }
        table.tablesorter thead tr .header {
	        background-image: url(bg.gif);
	        background-repeat: no-repeat;
	        background-position: center right;
	        cursor: pointer;
        }
        table.tablesorter tbody td {
	        color: #3D3D3D;
	        padding: 4px;
	        background-color: #FFF;
	        vertical-align: top;
        }
        table.tablesorter tbody tr.odd td {
	        background-color:#F0F0F6;
        }
        table.tablesorter thead tr .headerSortUp {
	        background-image: url(asc.gif);
        }
        table.tablesorter thead tr .headerSortDown {
	        background-image: url(desc.gif);
        }
        table.tablesorter thead tr .headerSortDown, table.tablesorter thead tr .headerSortUp {
        background-color: #8dbdd8;
        }
    </style>

    <script type="application/javascript" src="jquery-2.1.0.min.js"></script>
    <script type="application/javascript" src="jquery.tablesorter.js"></script>
</head>
<body>
	<table id="clientTable" class="tablesorter"> 
        <thead> 
            <tr> 
                <th>Time</th> 
                <th>Type</th> 
                <th>Event</th>
            </tr> 
        </thead> 
        <tbody> 
			<?php
				while( $row = mysql_fetch_array($events) )
				{
					Print "<tr>";
					Print "<td>".$row['timestamp']."</td>";
					Print "<td>".$row['type']."</td>";
					Print "<td>".$row['event']."</td>";
					Print "</tr>";
				}
			?>
        </tbody> 
    </table> 
</body>