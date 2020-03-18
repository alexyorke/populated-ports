# populated-ports
Warns if two applications are using the same port (e.g. Docker and Visual Studio Code.) A simple console application that beeps when two or more ports are in use. Ports can be configured by editing the source code.

This application solves the problem of accidently running two copies of an app, and running one but not the other. For example, starting a Docker image of an Angular app, and then also running it in Visual Studio Code and wondering why changing the code isn't changing anything; the Docker image started first so it takes priority (and vice-versa.)

It will beep three times to get your attention.

Sample output:

```
Listening for overlapping ports on ports 8000, 4200, 5000, 5001...
Listening for overlapping ports on ports 8000, 4200, 5000, 5001...
Port conflict found:
====================
Port 5000:
python2.7 (TCPv4 port 5000 pid 6376)
python2.7 (TCPv4 port 5000 pid 25104)
Would you like to close all above processes? Unsaved data will be lost (y/n):
y
Should I close all docker containers that are using that port? (y/n):
n
Closing processes...
```

Docker containers will be gracefully stopped, and then aborted after seven seconds if they are still not stopped. Only the docker containers which use the ports specified will be terminated. User-space applications will be gracefully stopped, and then forcefully stopped after five seconds if they have not stopped.
