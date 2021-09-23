\ some words to debug multitasking

#if undefined .word
#include lib/crash.fs
#endif

: .task
    dup ." Task:" .word-off
    dup 1 cells + @ ." State: " .
    dup boot-task <> if
    dup task-sp find-beef ." S:" .
    dup task-rp find-beef ." R:" .
    then
    dup 3 cells + @ ?dup if ." Err:" .word then
    dup 4 cells + @ ?dup if ." Check:" .word then
    cr drop
;

: tasks ( -- ) \ Show tasks currently in round-robin list
  \ The list may change, so first take a snapshot
  base @ hex
  eint? dint >r
  0
  up @ begin ?dup while dup @ repeat
  irq-task @ begin ?dup while dup @ repeat
  r> if eint then

  cr begin
    ( 0 tasks… )
    ?dup while
    .task
    ( 0 one-fewer-tasks… )
  repeat

  base !
;

#ok depth 0=
