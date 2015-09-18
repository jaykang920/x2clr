## 0.6.0 (2015-09-18)

- enhanced overall performance, especially for default socket links. re-wrote
  the default socket links for performance and extensibility
- to avoid pinned memory fragmentation, Buffer now works on array segments of 
  large buffers, instead of individual byte arrays
- unified namespaces as x2
- renamed the built-in thread-based flow classes
- re-structured built-in events and arranged the event type identifiers
