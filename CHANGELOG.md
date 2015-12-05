Features:

- LinkSession: added support for instant disconnection recovery

Bugfixes:

- fixed the incorrect event equivalence test problem in handler chain building
- fixed the bug that do-nothing nested coroutines never return
- added additional safeguard for handler exception

Improvements:

- reduced hash conflict rate in the event handler map

## 0.7.2 (2015-11-03)

Features:

- Event: added support for guarded hub posting with the using directive
- added support for nested coroutines
- LinkSession: added support for external context object
- added support for property type 'list(float32)'
- added support for property type 'list(datetime)'

Bugfixes:

- *TcpServer: fixed unhandled exception on close without listening

## 0.7.1 (2015-10-12)

Bugfixes:

- TcpClient: fixed the unhandled exception on connection failure
- *TcpSession: fixed the problem that a LinkSessionDisconnected event is not posted on active close

## 0.7.0 (2015-10-05)

Features:

- added Config class and its properties, supporting application configuration loading
- added periodic global heartbeat events
- added new internal property, _WaitHandle, to the Event class, to automate coroutine response matching
- renamed a few methods: Startup, Shutdown, Setup, Teardown

Bugfixes:

- fixed the growing send buffer bug

## 0.6.3 (2015-09-25)

- added initial non-reliable UDP socket links
- fixed ServerLink.Diagnostics connection counting
- added support for using(new Hub.Flows().StartUp()) block

## 0.6.2 (2015-09-22)

- Links/Sockets/ServerLink: fixed the bug that Diag.ConnectionCount was not updated.
- Links/Sockets/AbstractTcpClient: duplicate connect attempt now throws InvalidOperationException

## 0.6.1 (2015-09-21)

- Initial NuGet packages

## 0.6.0 (2015-09-18)

- enhanced overall performance, especially for default socket links. re-wrote
  the default socket links for performance and extensibility
- to avoid pinned memory fragmentation, Buffer now works on array segments of 
  large buffers, instead of individual byte arrays
- unified library namespaces as x2
- renamed built-in thread-based flow classes
- re-structured built-in events and arranged the event type identifiers
