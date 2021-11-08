#if-flag !multi
#end
#endif

#include test/reset.fs

compiletoflash
#require task sys/mult.fs
compiletoram

\ --------------------------------------------------
\  Examples
\ --------------------------------------------------

0 variable seconds
:task: time&
  begin
    1 seconds +!
    seconds @ . cr
    seconds @ 10 mod 0= if  task \int main continue then
    task stop
  again
;

: \st task !single ;
: \mt task !single ;

time& start
#if-flag debug
tasks
#endif

#if defined irq-systick

: tick ( -- ) timetask wake ;

 ' tick irq-systick !
 800000 $E000E014 ! \ How many ticks between interrupts ? This is 1/10th second
\     7 $E000E010 ! \ Enable the systick interrupt.

#endif

0 variable kicked
:task: kick
  begin
    1 kicked +!
    task yield
  again
;

task also
kick start
kick end
yield
#ok 0 kicked @ =

kick start
yield
kick end
yield
#ok 1 kicked @ =

kick start
kick end
yield
yield
#ok 1 kicked @ =

kick start
yield
yield
kick end
yield
yield
#ok 3 kicked @ =

task !multi

#if defined irq-systick
#delay 13
stop \ Idle the boot task
#delay 1
\st
0 $E000E010 ! \ Disable systick
 
#endif
\ multitask

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
