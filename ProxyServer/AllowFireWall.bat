set currpath=%~dp0

netsh advfirewall firewall add rule name="My ProxyServer" dir=in action=allow program="%currpath%ProxyServer.exe" enable=yes
pause