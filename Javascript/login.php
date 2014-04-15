<?php
	include("config.php");

	session_start();
	if($_SERVER["REQUEST_METHOD"] == "POST")
	{
		// username and password sent from Form 
		$myusername=addslashes($_POST['username']); 
		$mypassword=addslashes($_POST['password']); 

		$sql="SELECT * FROM members WHERE member_name='$myusername' AND password='$mypassword'";
		$result=mysql_query($sql);
		$count=mysql_num_rows($result);


		// If result matched $myusername and $mypassword, table row must be 1 row
		if($count==1)
		{
		
			$row=mysql_fetch_array($result);
			$active=$row['index'];
			$_SESSION['login_user']=$myusername;
			$_SESSION['login_id']=$active;
			$_SESSION['admin']=$row['admin'];
			header("location: welcome.php");
		}
		else 
		{
			$error="Your Login Name or Password is invalid";
			Print $error;
		}
	}
?>

<form action="" method="post">
	<label>UserName :</label>
	<input type="text" name="username" required="required"/><br />
	<label>Password :</label>
	<input type="password" name="password" required="required"/><br/>
	<input type="submit" value=" Submit "/><br />
</form>