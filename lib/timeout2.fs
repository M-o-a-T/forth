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

: now-hq now ;

#else

\ include the real MCU's tick configuration here

#if-flag !real
#error On virtual hardware but no syscall? doesn't work
#endif

#include soc/{arch}/tick.fs
time also
task also
bits tick also definitions

#endif

:init
  time now-hq  last-check !
;

\ Theory of operation:
\ 
\ For "real" hardware we want two return values from our time check:
\ how much time shall pass until the next task is ready, plus the delay
\ required after that. Why? we don't want to update the actual tick
\ counter if we can possibly help it, because that introduces an
\ inaccurracy. Instead, we want to update the value the counter is
\ reloaded with.
\ 
\ Thus initially we pass in "delta 0". check1 compares that to the
\ current task's remaining time; if the latter is smaller/equal (case A),
\ it is \ executed immediately and "delta" is decremented. Otherwise we leave
\ the initial delay on the stack (no zero).
\ The following task(s) may have a zero (incremental) timeout. They are skipped (case B).
\ The timeout of the task after that then stays on the stack and the scan ends (case C).

: check1 ( time1 0? task  -- time2 time 0|1 )
  >r
  ?dup if  ( r1 )
    r> %cls timeout @ ( r1 r2|0 )  \ case c|b
  else ( t1 )
    r@ %cls timeout @ 2dup u< if \ task needs more time. Bow out.
      swap - ( t1-r1 )
      dup r> %cls timeout !  \ case b
    else
      - ( r1-t1 )
      r> %cls continue
      0 \ case a, continue
    then
    0
  then
;

: (check) ( elapsed -- r2 r1 | 0 )
  0  queue each: check1
  \ Three cases.
  ?dup if \ (c) we see r1 r2.
    swap
  else
    \ the scan ran out of elements.
    \ The stack is now either "t1 0" (case a) or "t1" (case b)
    ?dup if \ case b: first wait for t1, then infinite
      0 swap
    else \ case a: no more timeouts, wait infinite
      drop 0 queue empty? 0= if 1 then
      \ except that if the queue isn't empty after all,
      \ we need to repeat this dance. Better safe than sorry.
    then
  then
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
\ maybe yield. Call every time through your loop.
  \yield @ ?dup if
    1- \yield ! drop
  else
    yield
    \yield ! 
  then
;


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
