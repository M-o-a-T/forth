\ timeout handling.

forth definitions only

voc: time

task also

%queue object: queue


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

0 variable last-check

: insert1 ( task time pos -- task 0 | 1 )
  >r r@ %cls timeout @  ( task tasktime postime )
  2dup u< if \ the new task needs less time
    \ take the difference off the existing task
    over - r@ %cls timeout !
    \ store the current delta in our task
    over %cls timeout !
    \ insert the new task before this one
    %cls link .. r> %cls link insert
    1
  else
    -
    rdrop
    0
  then
;

: insert ( task -- )
\ add to the queue, at its correct position
  dup %cls timeout @ ( task time,f )
  queue each: insert1
  0= if \ task time
    \ store the remaining delta in our task
    over %cls timeout !
    \ and append it
    queue insert
  then
;

: remove ( task -- )
\ remove from the queue.
  >r
  r@ %cls next .. \ ptr to link
  r@ %cls link remove
  \ =idle r@ %cls state !
  ( next |R: task )

  dup queue .. = if \ last element
    drop rdrop
  else ( old next )
    r> %cls timeout @  ( next time )
    swap %cls task-link @ timeout +!
  then
;

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
