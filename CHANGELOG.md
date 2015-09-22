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
