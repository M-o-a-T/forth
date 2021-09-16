\ -----------------------------------------------------------
\   Cooperative Multitasking
\ -----------------------------------------------------------

\ This is a mod of the original Mecrips-Stellaris multitasking code.
\ It is has more task switch overhead than the original if all your
\ tasks are running, but it's faster for systems with many tasks
\ when most are idle. Also it supports a "check" taskqueue which 
\ can be used for tasks that wait for something that's not directly
\ triggered by an interrupt.

\ Configuration:

\ Internal stucture of task memory:
\ 0: Pointer to next task
\ 1: Task currently active ? flags, 1:runnable
\ 2: Saved stack pointer
\ 3: Handler for Catch/Throw
\ 4: scheduling check.
   \ Task is on the stack and must stay there. Checker pushes a run flag:
   \ 0 do nothing
   \ 1 re-check
   \ 2 run
\ Parameter stack space: @stackspace cells
\ Return stack space: @stackspace cells

forth definitions only  decimal

#if defined throw
#error conflicts with single-tasked catch/throw
#endif

#if undefined eint
\ happens when running on Linux …
: eint inline ;
: dint inline ;
: eint? true 0-foldable ;
#endif

#if undefined .word
\ not debugging
: .word drop inline ;
#endif

#if undefined var>
#include lib/vars.fs
#endif

voc: \multi
var> also

class: task
__ivar
  0 
__seal

5 constant taskvars

' false 0 0 1 0  taskvars  nvariable boot-task
\ Boot task is active, without handler, and has no extra stackspace.

boot-task variable up  \ User Pointer
: this-task  ( -- task )    up @ inline ;
: task-state ( -- state )   up @ 1 cells + inline ;
: stack-ptr  ( -- save )    up @ 2 cells + inline ;
: handler    ( -- handler ) up @ 3 cells + inline ;
: checker    ( -- checker ) up @ 4 cells + inline ;

boot-task variable last-task

forth definitions
: in-boot-task? up @ boot-task = ; 

\multi definitions

0 variable irq-task  \ list of waiting tasks

\ these point to the word *after* a task's stack
: task-sp    ( task -- param-stack-top ) taskvars  stackspace    + cells + ;
: task-rp    ( task -- param-stack-top ) taskvars  stackspace 2* + cells + ;

\ decrement a stack pointer and store data there.
: sp+! ( data sp -- sp-1 )
  \ ." save " over hex.
  1 cells -
  \ ." to " dup hex. cr
  tuck ! inline ;

\ task state:
\ 0 idle, not scheduled
\ 1 scheduled on main queue
\ 2 is on run-check queue
\ 3 has ended: cannot be rescheduled

: =idle  0 0-foldable ;  \ wait
: =sched 1 0-foldable ;  \ schedule
: =check 2 0-foldable ;  \ check
: =dead  3 0-foldable ;  \ oww

forth definitions
: stop ( -- ) =idle task-state ! pause ;
\ Stop current task (can be woken up later)

\ -----------------
\ Create a new task
\ -----------------

: task: ( "name" -- )  stackspace 2* taskvars + cells  buffer: ;
\ two stacks

\multi definitions

0 variable abortmsg

\ Helpers for task end and error handling

: task-end ( -- does-not-return )
\ call "quit" if we're the boot task
  boot-task this-task = if [ hook-quit @ call, ] then
  =dead task-state !
  \ protect against single task
  [ hook-pause @ literal, ] hook-pause @ = if [ hook-quit @ call, ] then
  begin pause again
;

: (dead) ( -- does-not-return )
\ the current task is dead. Don't schedule again.
\ DO NOT call this directly. Use (die).
  cr ." END:" this-task .word cr
  task-end
;

: (die) ( errcode -- does-not-return )
\ End task. If in boot task, then quit, else stop.
  cr
  ." ERROR in " up @ .word
  abortmsg @ ?dup if 
    ." : " ctype
    0 abortmsg !
    dup -1 = if drop 0 then  \ ignore the ABORT" errcode
  then
  ?dup if ." , code=" . then
  cr
  task-end
;

forth definitions

: catch ( x1 .. xn xt -- y1 .. yn throwcode / z1 .. zm 0 )
    [ $B430 h, ]  \ push { r4  r5 } to save I and I'
    sp@ >r handler @ >r rp@ handler !
    ( xt | R: sp oldhandler )
    execute
    \ restore state
    r> handler !  rdrop  unloop 
    0  \ no error
;

\multi definitions

: (cont) r> ;

: (go) ( *params? a-addr -- does-not-return )
  \ cr ." RUN: " dup dup hex. .word cr
  execute (dead)
\ catch ?dup if (die) else (dead) then
;
: (task)  ( -- a-continue )
\ Main code for tasks
  (cont) (go)
;

: sfill ( addr cells -- )
\ fills a stack, i.e. from the bottom
  0 ?do
    1 cells -
    $deadbeef over !
  loop
  drop
;

forth definitions

: preparetask ( *args N task a-addr -- )
\ Prepare stacks.
\ The param stack contains
\ - the top of the return stack.
\ The return stack contains
\ - dummy values for R4 and R5
\ - the code to return to
\ Initially (task) is called, so the param stack also has initial args.
  swap >r ( *args N a-addr R: task )

  0         r@           ! \ nextptr
  0         r@ 1 cells + ! \ Flag
  \ 2: stack: below
  0         r@ 3 cells + ! \ No abort
  ['] false r@ 4 cells + ! \ No handler

  r@ task-sp stackspace sfill
  r@ task-rp stackspace sfill

  cr
  r@ task-sp ( *args N a-addr SP )

  \ Copy the arguments to the new stack
  2 pick
  begin  ( *args N a-addr SP N )
    ?dup
  while
    1- dup 4 + pick  ( *args N a-addr SP N-1 args[N-1] )
    rot sp+! swap
  repeat  ( *args N a-addr SP )

  \ … and drop them
  >r >r 0 ?do drop loop r> r>  ( a-addr SP )

  \ push a-addr onto SP. This (almost) finishes the stack setup for (task)
  sp+!  ( SP )

  r@ task-rp  ( SP-1 RP )
  (task) swap sp+! \ Store entry address at top of the task's return stack
  2 cells - \ Adjust RP for saved loop index+limit
  swap sp+! ( SP ) \ Save the adjusted RP to the param stack

  \ ." save " dup hex.
  r> 2 cells +
  \ ." to " dup hex. cr
  !  \ and finally save SP to task
;

\ --------------------------------------------------
\  Multitasking insight
\ --------------------------------------------------

\multi definitions

: find-beef ( stack-end -- n )
  stackspace cells -
  stackspace 0 do
    dup @ $deadbeef <> if drop
      i if stackspace i - else -1 then unloop exit
    then
    cell+
  loop
  drop 0
;

\ --------------------------------------------------
\  Exception handling
\ --------------------------------------------------

forth definitions

: throw ( throwcode -- )
\ Throw an error code.
\ This is a no-op if the throwcode is zero.
  ?dup if
    handler @ ?dup if
      \ restore previous state to jump to
      rp! r> handler ! r> swap >r sp! drop r>
      unloop  exit
    else
      (die) \ unhandled error: stop task
    then
  then
;

: abort ( -- )
\ Unconditionally throws a -1 error.
  -1 throw
;

\multi definitions

: (abort) ( flag cstr )
\ runtime for abort"
  swap ?dup if
    swap abortmsg ! throw
  else drop then
;

forth definitions

: abort" postpone c" ['] (abort) call, immediate ;

\multi definitions

: (chk-unqueued) ( task -- )
\ check that task is not queued
  >r

  this-task begin
    dup r@ = abort" Task already queued"
    dup @ while
    @
  repeat
  \ also check that last-task is correct
  last-task @ <> abort" last-task bad"

  irq-task @ begin
    ?dup while
    dup r@ = abort" Task on check list"
    @
  repeat

  r> drop
;

: (wake) ( task -- )
\ enqueues the task. MUST NOT be the current task. MUST NOT be already queued.
  \ ." W:" dup .word   \ debug only, not when multitasking
  dup (chk-unqueued)
  \ add to back of task list
  1 over 1 cells + !
  0 over !
  dup last-task @ ! last-task !
;

: (check) ( task -- )
\ enqueues the task to the IRQ list. MUST NOT be the current task. MUST NOT be already queued.
  \ ." W:" dup .word   \ debug only, not when multitasking
  dup (chk-unqueued)
  \ add to front of IRQ list
  2 over 1 cells + !
  irq-task @ over ! irq-task !
;

: run-checks
\ walks through the irq-task list
\ NOT interrupt safe: a task may either be on the list
\                     or started by an interrupt, never both
  irq-task @  0 irq-task !

  begin ( task )
  ?dup while
    dup @ swap  ( next-task task )
    dup 4 cells + @  ( next-task task checker )
    execute  ( next-task task flag )
    case
      =idle of  \ ignore. Clear flag.
        =idle swap 1 cells + !
        endof
      =sched of  \ yes, run
        (wake)
        endof
      =check of  \ no, but check again
        ." C2 " (check)
        endof
      drop  \ error / dead, oh well
    endcase  ( next-task )
  repeat
;

: (idle) ( task -- )
\ dequeues a task. MUST NOT be the current task. No-op if already dequeued.
  this-task begin  ( task chkptr )
    dup @ while
    2dup @ = if \ does @chkptr point to task?
      \ if task is last, point last-task to chkptr instead
      over last-task @ = if dup last-task ! then
      swap @ swap !  \ fix ptr
      exit
    then
    @  \ check next task
  repeat

\ \ Huh. Maybe it's on the check list?
\ irq-task begin  ( task chkptr )
\ dup @ while
\   2dup @ = if  \ does @chkptr point to task?
\     swap @ swap !  \ fix ptr
\     exit
\   then
\   @
\ repeat

  \ None of the above. To be sure, set checker to idle.
  ['] =idle 4 cells + !
;

forth definitions

: yield   ( stacks may fly around )
\ hooked to "pause" when multitasking
  this-task @ 0= if
    task-state @ =sched = if
      \ ." T:same:" this-task .word   \ debug only, not when multitasking
      exit  \ simply return if we're the only runnable task
    then

    \ Uh oh. We don't run, and neither does anybody else.
    \ You should have created an idle task!
    -3 abort" No running task"
  then

  dint
  [ $B430 h, ]        \ push { r4  r5 } to save I and I'
  rp@ sp@ stack-ptr !  \ save current return and stack pointer

  \ get current state bits
  task-state @
  this-task dup @  ( flag oldtask newtask )
  \ ." T:" dup .word   \ debug only, not when multitasking
  up !  ( flag oldtask )
  \ "up" now contains the new task, so "(wake)" on the old task will enqueue it
  swap  ( oldtask flag )

  \ Check flags to decide on the old task's fate.
  case ( oldtask flag -- )
  =sched of (wake) endof
  =check of ." C1 " (check) endof
  drop
  endcase

  \ now return to the now-current task
  stack-ptr @ sp! rp!  \ restore pointers
  unloop  \ pop { r4  r5 } to restore I and I'
  eint
  \ ." RET to " r@ hex. ."  RP=" rp@ hex. ."  SP=" sp@ hex. cr
  \ r@ 1 bic disasm-$ ! seec
;

: wake ( task -- )
\ Wake a random task up (IRQ safe)
  eint? dint swap  ( iflag task )
    dup 1 cells + @ case ( iflag task state -- iflag )
    =idle of (wake) endof
    =check of .s  ['] =sched swap 4 cells + ! endof  \ start it
    drop
    endcase  ( iflag )
  if eint then
;

: idle ( task -- )
\ Idle a random task. IRQ safe.
  eint? dint swap ( iflag task )
    dup 1 cells + @ case
    \ =idle: do nothing
    =sched of (idle) endof
    =check of 
      ['] =idle over 4 cells + ! ( iflag statevar task )
      endof
    endcase
    \ remove from runqueue if not current task and not already off

    over 1 swap bit@ if
      dup up @ <> if (idle) else drop then
    then
    1 cells + ( flag statevar )
    1 swap bic! ( flag )
  if eint then
;

: run ( task "name" -- )
\ shortcut for: 0 TASK ' name preparetask  TASK wake
  0 over  ' preparetask  wake
;

: *run ( *args n task "name" -- )
  dup >r ' preparetask r> wake
;

\multi definitions

: (*run) ( *args n task a-addr )
  over >r  preparetask  r> wake
;

: (run) ( task a-addr )
  0 -rot (*run)
;

forth definitions

: [run]
  ' literal, ['] (run) call,
immediate ;

: [*run]
  ' literal, ['] (*run) call,
immediate ;

: singletask ( -- ) [ hook-pause @ literal, ]  hook-pause ! ;
: multitask  ( -- ) ['] yield hook-pause ! ;

\ Override standard words so they work with tasks

: depth
  in-boot-task? if depth else
    sp@ this-task task-sp - 1 cells /
  then
;

: rdepth
  in-boot-task? if rdepth else
    rp@ this-task task-rp - 1 cells /
  then
;

: .s 
  \ can't patch our "depth" into the core, so …
  ." Stack: [" depth . ." ] "
  depth if 
    depth 1-
    begin ?dup while
      dup 1+ pick .
      1-
    repeat
    ." TOS: "dup . 
  then
  ." *>" cr
;

\ -------------
\ the idle task
\ -------------

\ This task checks whether anything else in the system is running, or wants
\ to run. Otherwise it sleeps, to conserve (some) power.

\multi definitions

#if defined irq-systick
: sleep ( c -- c ) [ $BF30 h, ] inline ; \ WFI Opcode, Wait For Interrupt, enters sleep mode
#else
: sleep ( c -- c ) ;  \ no-op, for now
#endif

task: idle-task

: up-alone? ( -- ? ) \ Checks if all other tasks are currently in idle state
  this-task @ 0=
inline ;

: idle& ( -- )
  begin
    run-checks
    up-alone? if
      dint run-checks
      up-alone? if
        sleep
      then
      eint
    then
    yield
  again
;

\ setup

: task-init
  ['] task-end hook-quit !
  idle-task [run] idle
  multitask
;

forth definitions

: init init task-init ;

task-init singletask

only
\ Going back to singletask is temporary: we need to either
\ get a serial IRQ with input buffer, or teach the terminal
\ to wait for the echo.

