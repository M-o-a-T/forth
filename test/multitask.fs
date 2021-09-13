
\ --------------------------------------------------
\  Examples
\ --------------------------------------------------

0 variable seconds
task: timetask

: time& ( -- )
  begin
    1 seconds +!
    seconds @ . cr
    seconds @ 10 mod 0= if boot-task wake then
    stop
    yield  \ debugging
  again
;

: \st singletask ;

timetask run time&
#if defined irq-systick
lowpower-task run lowpower&
#endif
tasks

\st
\ we overrun the serial buffer otherwise

#if defined irq-systick

: tick ( -- ) timetask wake ;

 ' tick irq-systick !
 800000 $E000E014 ! \ How many ticks between interrupts ? This is 1/10th second
\     7 $E000E010 ! \ Enable the systick interrupt.

#endif
#end

\ multitask

#if defined irq-systick
#delay 1.3
stop \ Idle the boot task
#delay 1
\st
0 $E000E010 ! \ Disable systick
 
#endif
\ multitask
