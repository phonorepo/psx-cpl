ProxyDump by phono

This program allows to capture http sessions and later to automatically respond the recorded responses again.


Short Notes:

1. Sessionfile
First you need a recorded session file (.saz) for your own client.
-add the urls for the session that you want to dump into responses\urlfilter.txt
-Connect to the internet
-run this program (ProxyDump.exe)
-press "j" for setting the program to DumpMode
-start whatever program that uses ProxyDump as Proxy and do your web session
-when finished press Q in this program to quit
-on your desktop should be a new file with ".saz"
-copy the .saz-file into the \responses folder
-you can delete all other .saz-files so it will be the default one

2. running offline
now after you have your own response file put into the responses folder, you can:
-disconnect completely from internet
-run this program (ProxyDump) - note the yellow line telling you that it uses you .saz-file!
-run your

now you should reach the login screen even if you are disconnected from internet


3. no dump created
If there is no .saz-file created on your desktop, your client may be connecting to a different URL.
You can edit the file responses\urlfilter.txt and add the URL your client is connecting to for loading. 