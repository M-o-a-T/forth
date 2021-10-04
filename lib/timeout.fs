\ timeout handling.

forth definitions only

#if defined syscall
#if undefined sys
#include lib/syscall.fs
forth definitions only
#endif
#endif

voc: time

task also

%queue object: queue

: check ( elapsed.f -- )
\ start all tasks that are ready
;

: insert ( task -- )
\ add to the queue at its correct position
;

: remove ( task -- )
\ remove from this queue.
;

#if defined syscall
: poll ( f,|-1 )
  forth poll poll
  drop
;
#endif


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
