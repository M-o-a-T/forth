The Forth interpreter
=====================

Traditionally, Forth has two interpreters. The "inner" interpreter, in
threaded Forth systems, steps from one Forth word to the next and runs the
code in each one. This chapter is not about that.

This chapter concerns the "outer" interpreter; specifically, how we talk to
it in a multi-tasked system, and (maybe even more important) how it talks
back.

This is handled by the code in ``lib/mt-term.fs``.

Input
+++++

The interpreter is running in the main task. Your ``:init`` words start a
couple of tasks, then the interpreter sits there and waits for data.

Incoming data passes through the ``inq`` ring. That ring is not particularly
large, since the interpreter should quickly take the data back out.

On "real" hardware, typically an interrupt handler on one of the UARTs
sends incoming characters to the queue.

On Linux, the ``inrecv`` task reads from standard input.

The input handler is responsible for passing the input stream through
``hook-packet``, if that is set. The hook is called with the byte to be
received and returns either the char plus ``True``, or ``False`` (with the
byte eaten).

Output
++++++

Output is challenging because there may be multiple tasks that want to send
data at the same time. We don't want to interleave output; that's not
particularly readable.

Thus there is a separate output task which coordinates giving each sender a
turn at sending. This task is also responsible for sending characters to the
UART (Blue Pill) or writing to standard output (Linux).

TODO: this needs to be adapted as soon as we have message-oriented output.

Debugging
+++++++++

Debugging is a nontrivial exercise from within a multi-tasked system. If the
task-switching machinery breaks down, there's no longer any output and (on
Linux) no input either.

Thus the multi-tasking deactivtion word switches the interpreter's I/O
hooks to the single-tasked version.

A third way, if debugging is enabled, is to use ``term emit-debug``. This
word changes console output to words which write directly to the UART,
without calling ``pause``. The rest of the system is not affected.

Debugging is somewhat easier on real hardware because we can hook into the
crash handler and produce a stack trace. On Linux, a segmentation fault
is more difficult to intercept; Mecrisp currently doesn't even try.

