============
System calls
============

On an MCU there obviously is no system to call into, so you might wonder
what this section of the documentation is good for.

The answer is that Mecrisp Stellaris is able to run under Linux (32-bit
ARM), which thanks to the magic of ``qemu`` means "on any current Linux
whatsoever". Thus you can use this feature to iteratively test things
without time-consuming (and ultimately flash-destroying) re-flashing of
test code, to a memory-constrained MCU that may not have enough space for
debug words.

++++++++++++
Vocabularies
++++++++++++

The syscall library is split into several sub-vocabularies. We always elide
common prefixes from named constants, thus it's ``sys pf inet`` instead of
``AF_INET``.

sys
===

Base vocabulary for all things related to sytem calls.

call0 … call7 ( args… callno -- result )
++++++++++++++++++++++++++++++++++++++++

System call wrappers for 0 to 7 arguments.

?err ( result -- result )
+++++++++++++++++++++++++

Check if the result is an error, i.e. between -1 and -1023 inclusive.
If so, abort.

?-err ( result -- )
+++++++++++++++++++

As ``?err`` but drops the result.


block ( fd -- )
+++++++++++++++

Set a file descriptor to blocking.

nonblock ( fd -- )
++++++++++++++++++

Set a file descriptor to nonblocking.

sys timespec
============

Class for timespec-related system calls.

get ( clock timespec -- )
+++++++++++++++++++++++++

clock_gettime(2)

sys monotonic
=============

A subclass of ``timespec``.

get ( timespec -- )
+++++++++++++++++++

clock_gettime(CLOCK_MONOTONIC)

sys realtime
============

A subclass of ``timespec``.

get ( timespec -- )
+++++++++++++++++++

clock_gettime(CLOCK_REALTIME)

sys epoll_event
===============

A copy of the C data type for events related to ``epoll``.

Used by ``epoll wait``, below. You typically don't use this directly.

sys epoll
=========

System calls related to ``epoll``.

create ( -- fd )
++++++++++++++++

epoll_create(2)

wait ( fd event ts -- result )
++++++++++++++++++++++++++++++

epoll_wait(2)

``event`` is a single event record.

Called by ``epcb poll``, below. You typically don't call this directly.

sys epoll event
===============

``EPOLL*`` event masks (without prefix). See the ``epoll_ctl(2)`` manpage.

sys epoll epcb
==============

Control structure to manage an epoll instance. On a multitasking system,
this includes a queue for the tasks waiting on it.

Note that the epoll system doesn't support multiple records per file
descriptor. If you need to wait for read *and* write on the same file /
socket, one side should clone it with ``dupfd``.

On multitasking systems, you typically don't allocate this
structure, as there's a global ``poll`` instance.

wait-read ( fd epcb -- )
++++++++++++++++++++++++

Wait for this file descriptor to become readable. On a multitasking system,
this suspends the caller until it is.

wait-write ( fd epcb -- )
+++++++++++++++++++++++++

Wait for this file descriptor to become writeable. On a multitasking system,
this suspends the caller until it is.

poll ( µs epcb -- work? )
+++++++++++++++++++++++++

epoll_wait(2)

This word returns -1 if there is no work. Otherwise, on a multitasking
system it continues the waiting tasks and returns 0. When singletasking,
it returns the (first) file descriptor that is ready.

Unlike the system call, zero µs means "infinite". To return immediately, use
one microsecond.

We use ``epoll`` for convenience, not speed, and thus don't support
returning multiple events / file descriptors.

sys err
=======

Error numbers. Positive constants, extracted from C header files. The
leading E has not been elided because it's only a single letter and people
are used to reading ENOENT instead of NOENT.

sys ipproto
===========

ipproto_* constants.

sys af, sys pf
==============

address and protocol family constants. (They are identical.)

sys sig
=======

Signal names. ``SIG_IGN`` and ``SIG_DFL`` are also defined here, without
the ``SIG_`` prefix of course.

sys sig bus_
============

Constants for SIGBUS signals.

sys sig segv_
=============

Constants for SIGSEGV signals.

sys sig ill_
============

Constants for SIGILL signals.

sys sig sa_
===========

``SA_*`` flag bits (masks, actually).

sys sig info
============

A data structure that mirrors ``struct siginfo_t``.

sys sig action
==============

A data structure that mirrors ``struct sigaction``.

sys sig altstack
================

A data structure that mirrors ``struct stack_t``.

sys sock
========

``SOCK_*`` constants.

sys F_
======

``F_*`` flags (for ``fcntl``).

sys S_
======

``S_*`` flags (for file modes).

sys O_
======

``O_*`` flags (for ``open``).


sys call 
========

Vocabulary for various system call wrappers.

Feel free to add your own if you need them.

exit ( code -- end )
++++++++++++++++++++

exit(2)

read ( fd ptr len -- len )
++++++++++++++++++++++++++

read(2)

write ( fd ptr len -- len )
+++++++++++++++++++++++++++

write(2)

open ( ptr len flags mode -- fd )
+++++++++++++++++++++++++++++++++

open(2)

close ( fd -- )
+++++++++++++++

close(2)

creat ( ptr len mode -- fd )
++++++++++++++++++++++++++++

creat(2)

dupfd ( fd -- fd2 )
+++++++++++++++++++

dup(2); not called ``dup`` for obvious reasons

fcntl ( fd cmd arg -- result )
++++++++++++++++++++++++++++++

fcntl(2)

pipe ( -- fd fd2 )
++++++++++++++++++

pipe(2)

socket ( domain type protocol -- fd )
+++++++++++++++++++++++++++++++++++++

socket(2)

bind ( fd addr len -- )
+++++++++++++++++++++++

bind(2)

connect ( fd addr len -- )
++++++++++++++++++++++++++

connect(2)

getpid ( -- pid )
+++++++++++++++++

getpid(2)

kill ( pid signal -- )
++++++++++++++++++++++

kill(2)

signal ( xt signum )
++++++++++++++++++++

signal(2); actually this calls sigaction(2) with the ``SA_SIGINFO`` flag set.

The XT **must** be a word that solely consists of ``sigenter realcode sigexit``.
``realcode`` is called with three arguments, as in C's ``sa_sigaction`` type.

Also, ``sigpsp`` must point to the bottom of an area for a temporary
parameter stack, as in::

    64 buffer: \sigstack
    \sigstack 60 + sigpsp !

See ``debug/crash2.fs`` for an example how to set this up.

This word only exists when Mecrisp is compiled with signal support.


forth poll
==========

On multitasking systems, this is the main ``epoll`` instance, used by the
task dispatcher to discover which tasks are ready.

You probably should use that instead of creating your own.


