<?php
$mysql_hostname = "ronstestmachine.cloudapp.net";
$mysql_user = "web";
$mysql_password = "password";
$mysql_database = "minerdata";
$bd = mysql_connect($mysql_hostname, $mysql_user, $mysql_password) 
or die("Opps some thing went wrong");
mysql_select_db($mysql_database, $bd) or die("Opps some thing went wrongB  " . mysql_error());
?>