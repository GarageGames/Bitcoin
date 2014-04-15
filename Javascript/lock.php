<?php
include('config.php');
session_start();
$user_check=$_SESSION['login_user'];

$ses_sql=mysql_query("select member_name from members where member_name='$user_check' ");

$row=mysql_fetch_array($ses_sql);

$login_session=$row['member_name'];

if(!isset($login_session))
{
header("Location: login.php");
}
?>