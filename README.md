# populated-ports
Warns if two applications are using the same port (e.g. Docker and Visual Studio Code.) A simple console application that beeps when two or more ports are in use. Ports can be configured by editing the source code.

This application solves the problem of accidently running two copies of an app, and running one but not the other. For example, starting a Docker image of an Angular app, and then also running it in Visual Studio Code and wondering why changing the code isn't changing anything; the Docker image started first so it takes priority (and vice-versa.) Some apps do not report a port conflict when starting, which is why this app can be useful.

If a port conflict is found, it will beep three times to get your attention before asking you to resolve the conflict. If you do not resolve the conflict within `60` seconds, it will re-check if the conflict exists and then beep three times again.

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

## Roadmap

This app is relatively new, so watch out for bugs. Feel free to make a PR or issue for any feature requests or bugs that you find.

Here's what I would like to support in the future:

- linux compatibility (currently `populated-ports` is configured to use Windows's Docker socket port)

- macOS compatibility

- better UI/integration (i.e. a balloon notification instead of a terminal; ability to manage multiple processes at once)

- ability to gracefully terminate console applications (e.g. via a `Ctrl+C` event) instead of issuing them a window close notification, which doesn't do anything

- if possible, ability to switch apps between the in-use port (i.e. Docker and Visual Studio are using port `5000`; I want to click a button to make Docker use it, then click another one to make Visual Studio use it without shutting down the apps.)

- better integration with editors (i.e. Visual Studio Code would show a message when starting an app which uses the same port as another one and I could close the app there natively with a popup.)

- ability to give priority to other processes (e.g. when Docker starts, quit Visual Studio's process if the ports overlap)
