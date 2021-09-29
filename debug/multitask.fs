\ some words to debug multitasking

#require .word fs/lib/crash.fs
#if \voc \d-list undefined ?
#include debug/linked-list.fs
#endif

get-current

task \int \task definitions also

: find-beef ( max stack-end -- n )
  dup @ poisoned <> if 2drop -4 exit then
  over cells -
  over 1 do
    cell+
    dup @ poisoned <> if drop
      i if i - else -2 then unloop exit
    then
  loop
  drop -3  \ should never happen
;

: .state ( state -- )
  case
    =new   of ." new"  endof
    =dead  of ." dead" endof
    =idle  of ." idle" endof
    =sched of ." run"  endof
    =check of ." chk"  endof
    =irq   of ." irq"  endof
    ." ?" . 0
  endcase
;

: ? ( task -- )
  dup ." Task:$" hex.  \ should be declared somewhere
  dup __ state @ ." State:" .state space
  dup __ pstack @ if
    dup __ pstack @  over __ task-ps find-beef ." S:" .
  then
  dup rstack @ if
    dup __ rstack @  over __ task-rs find-beef ." R:" .
  then
  space
  dup __ state @ =idle > if
    dup __ link ?
  then cr
  dup __ abortptr @ ?dup if ." Abort:" hex. then
  dup __ abortcode @ ?dup if ." Sig:" hex. then
  dup __ checkfn @ ?dup if ." Check:" .word  dup __ checkarg @ hex.  then
  cr drop
;

set-current

forth definitions
: tasks ( -- ) \ Show tasks currently in round-robin list
  \ The list may change, so first take a snapshot
  eint? dint >r
  0 this .. ( 0 task )
  begin
    dup \task next @ ..
    dup this .. =
  until
  ['] nop irq-tasks (run)  \ just leave them on the stack
  r> if eint then
  ( 0 tasks… )

  cr begin
    ( 0 tasks… )
  ?dup while
    task \int \task ?
    ( 0 one-fewer-tasks… )
  repeat
;

#ok depth 0=
