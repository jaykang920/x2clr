Features:

- added support for partial serialziation: now a nested cell object is serialized exactly as it should be, even if it is actually an object of derived (bigger) type.

## 0.9.1 (2016-11-01)

Improvements:

- overriding `Setup()` or `Teardown()` method in `Flow` or `Link` subclasses,
now you don't have to chain into `base.Setup()` or `base.Teardown()` if you don't
need it explicitly

## 0.9.0 (2016-10-31)

Improvements:

- changed the coroutine programming interface.
 - support for syntax such as `yield return coroutine.WaitForSeconds(10);`
 - `Coroutine.Context` property has been renamed as `Coroutine.Result`

## 0.8.5 (2016-10-25)

Features:

- now `EventBasedFlow` subclasses works with ConcurrentQueue in .NET 4 or higher framework by default.
- `EventQueue` may be subclassed to meet custom needs and be used with existing flow implmentations

Bugfixes:

- fixed possible duplicate connect attempt exception in AbstractTcpClient.ConnectAndSend

## 0.8.4 (2016-10-21)

Features:

- initiated framework version multi-targeting (net35 and net40)
- added .NET 4 ConcurrentQueue-based flow. Contributed by @keedongpark

## 0.8.3 (2016-09-08)

Improvements:

- Unity3D: suppressed the use of reflection to support IL2CPP iOS build
- Unity3D: now .NET 2.0 subset is supported

## 0.8.2 (2016-04-05)

- renamed a few methods

## 0.8.1 (2016-04-01)

Features:

- property type datetime is now encoded as milliseconds, not microseconds

## 0.8.0 (2016-03-14)

Features:

- enhanced the TCP session recovery from instant disconnection
- added connect-on-demand support for TCP clients.
- changed the XML attribute name for super class: from "extends" to "base"
- added handler routine helper class, Scope, to support guarded response posting and handler rebinding

## 0.7.5 (2016-02-02)

Features:

- added ContainsName/ContainsValue to ConstsInfo, to support validity check

Bugfixes:

- fixed the problem in continuing no-yield coroutines
- SegmentPool: fixed synchronization issue

## 0.7.4 (2015-12-20)

Bugfixes:

- fixed the problem that equivalence test doesn't include type check in handler chain building

## 0.7.3 (2015-12-05)

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
