\ timeout handling, part 2.

forth definitions only
#if undefined time
#include lib/timeout.fs
forth definitions only
#endif

#if defined syscall
#if undefined sys
#include lib/syscall.fs
forth definitions only
#endif
#endif

time definitions also
task also

#if defined syscall
sys also

monotonic object: systime

: now
  systime get
  systime @  ( float.seconds )
  1000000, f* nip
;

#endif

:init
  now  last-check !
;


: check1 ( time task  -- time 0 | 1 )
  >r
  r@ %cls timeout @ 2dup u< if \ task needs more time. Bow out.
    swap - dup r> %cls timeout !
  else
    -
    r> %cls continue
    0
  then
;

: (check) ( elapsed -- delay )
  queue each: check1
  dup 0= if 2drop -1 then \ if we ran past the end, the time is still on-stack
;
: check ( -- delay )
\ start all tasks ready since our last call
  \ get µs since last call
  now  last-check @  over last-check !  -
  (check)
;

: poll ( timeout -- work? )
\ flag 0: check only
  check ( flg dly )
#[if] defined syscall
  umin
  forth poll poll
#else
  nip
#endif
;

\ ****************
\ long-term delays
\ ****************

0 variable next-hour
%queue object: hourq

:task: hourtask
  now next-hour !
  begin
    3600 1000000 * next-hour +!
    next-hour @ now -  task sleep 
    hourq all
  again
;
:init hourtask start ; 
    

: seconds ( sec -- )
  1000000 *  task sleep ;

: minutes ( min -- )
  60 * seconds ;

: hours ( hour -- )
\ wait multiple hours.
  ?dup if
    \ How far into the current hour is hourtask?
    3600 1000000 * next-hour @ now - -
    swap 1 ?do  hourq wait  loop
    \ … which is our additional delay
    task sleep
  then
;



forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
