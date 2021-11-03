\ timeout handling. Multitask only.

forth definitions only

#require d-list-item lib/linked-list.fs

\ Theory of operation:
\
\ We maintain a single queue of tasks waiting for a timeout.
\ The queue is sorted by time remaining after the task before it.
\
\ This makes removal quick, just add my time to the task after me,
\ but insertion is a linear search.
\ There's one usecase when that is not ok, which is adding a ten-second
\ error timeout which you then remove after a millisecond, only to
\ repeat the same thing another millisecond later.
\ The workaround for this is to use progressively larger delays.

#if defined time
#if time defined queue
#end
#endif
#endif

#if undefined time
voc: time
#else
time definitions also
#endif

d-list-head object: queue

class: %timer
__data
  d-list-item field: link
  var> uint field: timeout
  var> uint field: code \ go here when the timeout triggers
__seal

#if-flag debug
: setup
  __ link >setup
;
#endif

: _ins ( timer time pos -- task 0 | 1 )
  >r r@ %timer timeout @  ( task tasktime postime )
  2dup u< if \ the new task needs less time
    \ take the difference off the existing task
    over - r@ %timer timeout !
    \ store the current delta in our task
    over %timer timeout !
    \ insert the new task before this one
    %timer link .. r> %timer link insert
    1
  else
    -
    rdrop
    0
  then
;

: add ( timeout timer -- )
\ add to the queue, at its correct position
  swap ( timer time )
  ['] _ins queue each
  0= if ( timer time )
    \ store the remaining delta in our timer
    over %timer timeout !
    \ and append it
    queue insert
  then
;

: remove ( timer -- )
\ remove from the queue.
  >r
  r@ %timer link next @ .. \ ptr to link
  r@ %timer link remove
  \ =idle r@ %timer state !
  ( next |R: task )

  dup queue .. = if \ last element
    drop rdrop
  else ( next )
    r> %timer timeout @  ( next time )
    swap %timer timeout +!
  then
;

;class

\ The timer check code has two (and a half) functions.
\ * call all expired timers
\ * return how much time needs to pass until the next timeout
\ * return how much more time needs to pass until the timeout *after* that
\ 
\ Why return two values?
\ For some "real" hardware we want both.
\ The reason is that we don't want to update the actual tick
\ counter if we can possibly help it, because that introduces an
\ inaccurracy. Instead, we want to update the value the counter is
\ reloaded with.

\ The second return value is required for hardware timers that have
\ a built-in reload timer.
\
\ Thus initially we pass in "delta 0". check1 compares the delta to the
\ current timer remaining time; if the latter is smaller/equal (case A),
\ it is executed immediately and "delta" is decremented. Otherwise we leave
\ the timer's remaining delay on the stack ( delay ).
\ The following timer may have a zero incremental timeout. They are skipped (case B).
\ The timeout of the timer after that then stays on the stack and the scan ends (case C).

: check1 ( time1 0? timer  -- time2 time 0|1 )
  >r
  ?dup if  ( r1 )
    r> %timer timeout @ ( r1 r2|0 )  \ case c|b
  else ( t1 )
    r@ %timer timeout @ 2dup u< if \ needs more time. Bow out.
      swap - ( t1-r1 )
      dup r> %timer timeout !  \ case b
    else
      - ( r1-t1 )
      r> dup %timer code @ execute
      0 \ case a, continue with "delay 0". The delay may be zero; doesn't matter.
    then
    0
  then
;

: (check) ( elapsed -- r2 r1 | 0 )
  0  ['] check1 queue each.x
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

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
