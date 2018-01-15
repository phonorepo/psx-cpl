# psx-cpl

This combines a DNS Server, 2 instances of web server, TCP Connection Client.

WARNING:
It was more or less a proof of concept to have one in all.
This means this code is not polished, follows no coding standard and should not be used for any serious work as it could be harmful even if it was never intended! 
So only use it at your own risk and if you know what you do.

You might need to configure / disable your firewall because the program is listening on the ports 53 (dns), 80 (http), 5350 (elfloader http).

As I have not enough time to finish it, there is the need for many improvements like:
- code cleanup
- ui design and missing controls
- stabilization
- proper logging (log4net)
- move to dotnet core to be able to use it with linux
- add proxy

Thanks goes to all of the developers and contributors like CTurt,Hitodama,Specter,flatz,idc,fail0verflow,...


Original source for DNS library is:
https://github.com/kapetan/dns (Mirza Kapetanovic)

Original source http server is:
https://gist.github.com/flq/369432 (Frank Quednau)

thanks to all contributing at https://stackoverflow.com/
