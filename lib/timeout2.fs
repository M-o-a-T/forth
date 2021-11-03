\ timeout handling, part 2.

forth definitions only

#if time undefined now
#include lib/time.fs
#endif

#if time defined hourtask
forth only definitions
#end
#endif

time also definitions

0 variable last-check

:init
  now-hq last-check !
;

: check ( -- delay2 delay1 )
\ start all tasks ready since our last call
  \ get µs since last call
  now-hq  last-check @  over last-check !  -
  (check)
;

\ ****************
\ long-term delays
\ ****************

#if-flag multi

0 variable next-hour
task %queue object: hourq

:task: hourtask
  now next-hour !
  begin
    3600 1000000 * next-hour +!
    next-hour @ now -  task sleep 
    hourq all
  again
;

:init hourtask start ; 
    
time definitions

: micros ( µsec -- )
  task sleep ;

: millis ( msec -- )
  1000 *  task sleep ;

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

\ The following implements a way to yield every n'th pass through a loop,
\ or something.

time definitions

0 variable \yield

: nyield-reset ( -- )
\ reset. Call after finishing your work.
  0 \yield !
;
: ?nyield ( n -- )
\ maybe yield. Call in your loop to yield after N passes.
  \yield @ ?dup if
    1- \yield ! drop
  else
    task yield
    \yield ! 
  then
;

#endif


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
