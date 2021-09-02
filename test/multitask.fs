
\ --------------------------------------------------
\  Lowpower mode
\ --------------------------------------------------

#if defined eint
: up-alone? ( -- ? ) \ Checks if all other tasks are currently in idle state
  next-task @ \ Current task is in UP. Start with the next one.
  begin
    dup up @ <> \ Scan the whole round-robin list until back to current task.
  while
    dup 1 cells + @ if drop false exit then \ Check state of this task and exit if it is active
    @ \ Next task in list
  repeat
  drop true
;

: sleep ( -- ) [ $BF30 h, ] inline ; \ WFI Opcode, Wait For Interrupt, enters sleep mode

task: lowpower-task

: lowpower& ( -- )
  lowpower-task activate
    begin
      eint? if \ Only enter sleep mode if interrupts have been enabled
        dint up-alone? if ."  Sleep " sleep then eint
      then
      pause
    again
;
#endif

\ --------------------------------------------------
\  Examples
\ --------------------------------------------------

compiletoram
#if defined eint
  eint
#endif
  multitask

0 variable seconds
task: timetask

: time& ( -- )
  timetask background
    begin
      1 seconds +!
      seconds @ . cr
      seconds @ 10 mod 0= if boot-task wake then
      stop
    again
;

time&
#if defined eint
  lowpower&
#endif
  tasks

#if defined irq-systick
: tick ( -- ) timetask wake ;

 ' tick irq-systick !
 8000000 $E000E014 ! \ How many ticks between interrupts ?
       7 $E000E010 ! \ Enable the systick interrupt.

#delay 12
stop \ Idle the boot task
#delay 1
 
#endif
