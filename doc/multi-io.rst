Multi-threading and terminal I/O
================================

Output
++++++

Basically, threading and console output don't mix.

Forth's output loops until ``emit?`` is true before it calls ``emit``, for
every character. And ``emit?`` unconditionally calls ``pause``, which (if
multitasking is on) calls ``yield``.

Thus if more than one task prints something, it all ends up interleaved,
which may look pretty but is reasonably unreadable.

We can do better.

Thus ``lib/mt-term.fs`` contains alternate output code; it replaces
``emit`` (by setting ``hook-emit``) to write to a buffer. If another task
is writing, wait until it is done.

"Done" means, either it writes a line feed, wrote 200 bytes, or nothing
(for a couple of ``yield`` calls).

The tests for line feeds and 200 bytes are applied in debug mode only, as
otherwise a task that continually writes some lines could starve others.
During normal operation, serial output may be multiplexed between packet
and console output, and we won't want one to interfere with the other.

Also, on Unix we want to write more than one character at a time.

Input
+++++

Input doesn't have to be dispatched because because usually there's one
interpreter running (the one in the main loop). However, if any other part
of the system is busy you'll lose serial characters. Thus serial data, on
"real" hardware, needs to be read by an interrupt (TODO). On Unix, on the
other hand, we want to reda more than one character at a time because,
again, system calls are slow.

Another reason to hook into the input system is that MoaT wants to read
packetized data, possibly interleaved with console input. We supply a hook
variable for this usecase.

Testing
+++++++

Usurping Forth's basic input/output handling is tricky. 

