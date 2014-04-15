<?php  
  include('config.php');
  
  session_start();  
  $user_name=$_SESSION['login_user'];
  $user_id=$_SESSION['login_id'];  
  $user_admin=$_SESSION['admin'];
  
  // Get all the workdata for this month
  $workdata = mysql_query("select product_id, member_id, AVG(hashrate) as hrav from workdata where MONTH(timestamp) = MONTH(CURDATE()) AND YEAR(timestamp) = YEAR(CURDATE()) GROUP BY product_id");
  $totalHashrate = 0;
  $productHashrates = array();
  $myTotalHashrate = 0;
  while( $row = mysql_fetch_array($workdata) )
  {
      $totalHashrate += $row['hrav'];
      $productHashrates[$row['product_id']] = $row['hrav'];
      if( $user_admin > 0 || $user_id == $row['member_id'] )
        $myTotalHashrate += $row['hrav'];
  }  
  
  if( $user_admin > 0 )
    $productsSql=mysql_query("select * from products");
  else
    $productsSql=mysql_query("select * from products where member='$user_id' ");
  
?>
<head>
  <style type="text/css">    
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
  <h1>Products for member: <?php echo $user_name?></h1>
  <?php
    if( $user_admin > 0 )
        Print "<a href=eventlog.php>Event Log</a><br><br>";
        
    Print "<label>My Hashrate: ".number_format($myTotalHashrate, 0)."</label><br>";
    Print "<label>Network Hashrate: ".number_format($totalHashrate, 0)."</label><br>";
    Print "<label>My Percentage: ".number_format(($myTotalHashrate / $totalHashrate) * 100, 2)."%</label><br><br><br>";
    ?>

    <table id="clientTable" class="tablesorter">
      <thead>
        <tr>
          <th>Product</th>
          <th>Hashrate</th>
          <th>Percentage</th>
        </tr>
      </thead>
      <tbody>
        <?php
				  while( $row = mysql_fetch_array($productsSql) )
				  {
            $id = $row['product_id'];
            if( array_key_exists($id, $productHashrates) )
            {
              if( $user_admin > 0 )
                $url = "<a href=product.php?productId=" . $row['product_id'] . "&memberId=" . $row['member']. ">" . $row['product_name'] . "</a><br>";
              else
                $url = "<a href=product.php?productId=" . $row['product_id'] . ">" . $row['product_name'] . "</a><br>";
        
					    Print "<tr>";
					    Print "<td>".$url."</td>";
					    Print "<td>".number_format($productHashrates[$id], 0)."</td>";
					    Print "<td>".number_format(($productHashrates[$id] / $totalHashrate) * 100, 2)."</td>";
					    Print "</tr>";
            }
				  }
			  ?>
      </tbody>
    </table>
  </body>