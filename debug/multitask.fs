\ debug multitasking: add `?` to tasks

#require .word debug/crash.fs
#if \voc \d-list undefined ?
#include debug/linked-list.fs
#endif

#if task %cls undefined ?

#include debug/linked-list.fs

task %cls definitions also
task also

: find-beef ( max stack-end -- n )
  dup @ poisoned <> if 2drop -4 exit then
  over cells -
  over 1 do
    cell+
    dup @ poisoned <> if drop
      i if i - else drop -2 then unloop exit
    then
  loop
  2drop -3  \ should never happen
;

: .state ( state -- )
  case
    =new   of ." new"  endof
    =dead  of ." dead" endof
    =idle  of ." idle" endof
    =sched of ." run"  endof
    =check of ." chk"  endof
    =irq   of ." irq"  endof
    =wait  of ." wait" endof
    =timer of ." time" endof
    ." ?" . 0
  endcase
;

: ? ( task -- )
  dup \ ." Task:$" dup hex.
  .word-off \ prints hex addr if not declared
  space 
  dup __ state @ ." State:" .state
  dup __ newstate @ ?dup if
    over __ state @
    over <> if
      ." >" .state
    else 
      drop
    then
  then space
  dup __ pstack @ if
    dup __ pstack @  over __ task-ps find-beef ." S:" .
  then
  dup __ rstack @ if
    dup __ rstack @  over __ task-rs find-beef ." R:" .
  then
  space
  dup __ state @ =idle > if
    ." Link:" dup __ link ?
  then
  dup __ state @ =timer = if
    dup __ timeout @ ." Timer:" .
  else
  \ dup __ abortptr @ ?dup if ." Abort:" hex. then
    dup __ abortcode @ ?dup if ." Sig:" . then
    dup __ checkfn @ ?dup if ." Check:" .word  dup __ checkarg @ hex.  then
  then
  \ 'checkarg' may also be a queue
  drop
;

#endif

forth definitions

#if defined check-tasks

: nop0 0 ;
: tasks ( -- ) \ Show tasks currently in round-robin list
  \ The list may change, so first take a snapshot
  eint? dint >r
  0 this .. ( 0 task )
  begin
    dup %cls next @ ..
    dup this .. =
  until drop
  check-tasks each: nop0 drop \ just leave them on the stack
  irq-tasks each: nop0 drop \ just leave them on the stack
  r> if eint then
  ( 0 tasks… )

  cr begin
    ( 0 tasks… )
  ?dup while
    task %cls ? cr
    ( 0 one-fewer-tasks… )
  repeat
;
#endif

forth only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
