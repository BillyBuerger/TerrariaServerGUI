# TerrariaServerGUI
Graphical front-end for Terraria Server

Simple graphical front-end for for a Terraria Server.  The primary ideas were to report a timestamp when things happen and write the results to a log file.  In this way, as the server admin, you know when people joined and left and could review history of multiple sessions.  I also added a list of current users which is written out to an XML file.  I then reference that file through a website so people can see who is connected.

I used https://github.com/oiisamiio/TerrariaDedicatedServerGui for my initial reference but threw much of that away.  It was trying to do too much that I didn't need.  Also, the server program added additional features that made some of that stuff unnecessary.  I also used http://www.codeproject.com/Articles/170017/Solving-Problems-of-Monitoring-Standard-Output-and to handle some of the issues with input/output from the server process.  I tried implementing some MVVM concepts but the threading with the server process caused some issues making it hard to fully implement.
