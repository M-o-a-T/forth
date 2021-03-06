\ -----------------------------------------------------------
\   Cooperative Multitasking
\ -----------------------------------------------------------

\ This is a mod of the original Mecrips-Stellaris multitasking code.
\ It is double-linked for faster addition to / removal from the 
\ job queue.
\ 
forth definitions only  decimal

#if undefined task
\ skip ahead, there's a possibly-in-RAM part below

#if defined throw
#error conflicts with single-tasked catch/throw
#endif

#if undefined .word
#if-flag debug
#include debug/crash.fs
#else
: .word drop inline ;
#endif
#endif

#echo duh
#require class: lib/class.fs
#require d-list-item lib/linked-list.fs
#if-flag debug
#require .word debug/crash.fs
#endif

#include lib/timeout.fs

voc: task
voc: \int

: yield-dummy -29 abort ;
' yield-dummy variable yield-hook

#if-flag debug
0 variable yield-trace
#endif

voc: sub
task definitions
sub ignore

0 constant =new  \ new (not linked)
1 constant =dead  \ dead (not linked)
2 constant =idle  \ wait (not linked)
3 constant =nsched \ newly scheduled (do not abort)
4 constant =sched \ scheduled (in task list)
5 constant =check \ checking (in IRQ list)
6 constant =irq \ checking, IRQ on (in IRQ list)
7 constant =wait \ in some other queue
8 constant =timer \ in the timer queue
\ there might be more later
\ if more than 9, adapt the "state>q" table below

\ ********************
\         task
\ ********************

\ base class for tasks, may or may not have its own stack, thus internal


task definitions

d-list-head class: %queue
\ Some task queue
;class

task \int definitions

var> int class: waitq-link
%queue item
: @ ( d-list-adr -- task )
\ the entry stores a pointer to a waitq
  __ @ inline ;

;class

task definitions

sized class: %cls
\ the TASK class
__data
  var> cint field: pstack  \ param stack size, cells
  var> cint field: rstack  \ return stack size, cells
  aligned

\ Intermission: get from the link field back to the task.

time %timer class: task-link
dup constant \link-off
%cls item
: @ ( d-list-adr -- task )
\ pretend that the list item stores a link to the task it's in
  __ \link-off - inline ;

;class

\ back to our class variables
  %cls definitions

  task-link field: q
  var> int field: stackptr
  var> int field: abortptr
  var> int field: abortcode
  var> int field: checkfn \ redundant?
  var> int field: checkarg
  var> cint field: state
  var> cint field: newstate
  aligned
__seal

\ var> int item
\ : checkfn __ q code .. ;

\int waitq-link item
: waitq __ checkarg .. ;

\ xx constant psize  \ parameter stack size
\ xx constant rsize  \ return stack size
\ we want a guard cell above and below; the return stack also holds the
\ context register(s).
: psize@ s" psize" voc-eval dup if 2 + then ;
: rsize@ s" rsize" voc-eval dup if 2 + #ctx + then ;
: size __ size __ psize@ __ rsize@ + cells + ;

: task-ps ( task -- addr )
\ base address of parameter stack.
\ Points to the last (guard) cell.
  dup __ \offset @ over ( task off task )
  dup __ pstack @ 1- ( task off task ps )
  swap __ rstack @ + ( task off ps+rs )
  cells + +
;

: task-rs ( task -- addr )
\ base address of parameter stack.
\ Points to the last (guard) cell.
  dup __ \offset @ over __ rstack @ 1- cells + +
;

\ prev+next return whatever the link points to.
\ We don't auto-convert this to a taskptr because if the task is not
\ scheduled it may point to a d-list-head instead.
task-link item
: prev __ q link prev @ .. ;
task-link item
: next __ q link next @ .. ;

: setup ( object -- )
  dup __ q >setup
  __ psize@ over __ pstack !
  __ rsize@ over __ rstack !
  =new swap __ state !
;


;class  \ %cls

task definitions

: yield \int yield-hook @ execute ;

var> int class: %var
%cls item
: @ __ @ inline ;
;class


\ ********************
\      main task
\ ********************
\
\ This is a singleton class for the main Forth task.
\ 
\ Tasks for separate CPUs need their own UP. Not implemented yet!

task \int definitions

\ The main task doesn't store stacks
%cls class: \main
0 constant psize
0 constant rsize

: setup
  \ this is the initial+running task, so link to itself
  dup __ q link .. d-list-head >setup

  =sched swap __ state !
;

;class  \ main task

#ok depth 0=


\ ********************
\      global vars
\ ********************
\

\ our main task
\main object: main

\ Back to defining TASK
task definitions

\ pointer to the current task. Some Forths call this UP (User Pointer)
main .. variable this-task

: >this ( task -- )
\ Set the current task (also for future multi-CPU)
  this-task !
;

task also
task definitions

%cls item
: this ( -- task )
\ Get the current task (to reduce typing, but also for future multi-CPU)
  this-task @ ..
  inline
;

#if-flag debug
main .. variable dbg-this-task
%cls item
: dbg-this ( -- task )
\ Get the current task (to reduce typing, but also for future multi-CPU)
  this-task @ ..
  inline
;
: this>dbg
  this-task @ dbg-this-task !
;
#endif


: in-main?  ( -- flag )
\ are we in the main task?
  \ ." M:" this-task @ hex. this .. hex. main .. hex. cr
  this .. main .. =
;

#if-flag debug
: dbg-in-main?  ( -- flag )
\ are we in the main task?
  \ ." M:" this-task @ hex. this .. hex. main .. hex. cr
  dbg-this .. main .. =
;
#endif

forth definitions

#if-flag debug
: depth
  dbg-in-main? if depth else
    dbg-this task-ps sp@ - 1 cells / 1-
  then
;

: rdepth
  dbg-in-main? if rdepth else
    dbg-this task-rs rp@ - 1 cells / 1-
  then
;

#else

: depth
  in-main? if depth else
    this task-ps sp@ - 1 cells / 1-
  then
;

: rdepth
  in-main? if rdepth else
    this task-rs rp@ - 1 cells / 1-
  then
;

#endif

\ Override standard words so they work with tasks

\voc sticky
: .s
  \ can't patch our "depth" into the core, so ???
  depth
  ." Stack: [" dup . ." ] "
  ?dup if 
    1-
    begin
    ?dup while
      dup 1+ pick .
      1-
    repeat
    ." TOS: " dup . 
  then
  ." *>" cr
;

\voc sticky
: h.s
  \ can't patch our "depth" into the core, so ???
  depth
  ." Stack: [" dup . ." ] "
  ?dup if 
    1-
    begin
    ?dup while
      dup 1+ pick hex.
      1-
    repeat
    ." TOS: " dup hex.
  then
  ." *>" cr
;

%cls definitions
: running?  ( task -- flag )
\ is this task currently executing?
  this .. =
;

\ ********************
\        abort
\ ********************

\ Now that the task structure is present,
\ we can define multitask-capable abort handling


\voc definitions also

#ok depth 0=
#ok undefined aborthandler

\ per-task abort handling
: aborthandler this abortptr .. ;

#include sys/abort.fs

\voc ignore


\ ********************
\     Task queues
\ ********************

task %queue definitions also
task also

: insert ( task q -- )
\ insert a task.
  swap %cls q link .. swap  \ address of the link
  __ insert
;

\
\ a few state changing words will be defined later



: (adj) ( xt link -- xt )
\ call xt with the task that contains "link"
  %cls task-link @ .. \ adjust link so that it addresses the task

  \ park the original xt on the stack during its execution
  \ for transparency
  swap >r 
  r@ execute
  r> swap
;

: each ( xt queue -- flag )
\ call xt with every job
  ['] (adj) swap
  __ each.x
  ( xt flag )
  nip
;

;class

\ There are other lists. Specifically we need one for tasks that are waiting.
%queue object: irq-tasks  \ tasks in =irq
%queue object: check-tasks  \ tasks in =check



#if-flag debug
#include debug/multitask.fs
#include debug/linked-list.fs
#endif

forth definitions only
#if undefined time
#include lib/timeout.fs
forth definitions only
task also
#endif



\ ********************
\    State Machine
\ ********************


task \int definitions also

\ *** state transition: away from ???
: _table: <builds does> @ swap 3 * rshift 7 and ;

0 constant _err
1 constant _no
2 constant _run
3 constant _chk
4 constant _irq
5 constant _enq
6 constant _deq
7 constant _ent

_table: state>q
_ent                  \ =timer
_enq swap 3 lshift or \ =wait
_irq swap 3 lshift or \ =irq
_chk swap 3 lshift or \ =check
_run swap 3 lshift or \ =sched
_run swap 3 lshift or \ =nsched
_no  swap 3 lshift or \ =idle
_err swap 3 lshift or \ =dead
_no  swap 3 lshift or \ =new
,

task also
%cls definitions also
\int also

: \old  ( state task new_q old_q -- state task new_q )
    case
      _no of
        endof
      _err of
        -22 abort" Task dead"
        endof
      ( state task new_q )
      _ent of
        over __ q remove
#if-flag debug
        over 0 swap __ q timeout !
#endif
        endof
      drop over __ q link remove  \ task is queued, so dequeue
      0
    endcase
;
: \new  ( state task new_q -- state task )
    case
      _run of
        dup __ q link ..  this q link insert
        endof
      _irq of
        dup irq-tasks insert
        endof
      _chk of
        dup check-tasks insert
        endof
      _ent of
        dup __ q timeout @
#if-flag debug
        dup 0= abort" Timer zero"
        over __ q link >setup
#endif
        over __ q add
        endof
      _enq of
#if-flag debug
        dup waitq @ .. 0= abort" wait without queue"
#endif
        dup dup __ waitq @ insert
#if-flag debug
        0 over __ waitq !
#endif
        endof
    endcase
;

: >state ( state task -- )
\ set task state
\ may be postponed until YIELD

  dup __ running? if
#if-flag debug
\ yield-trace @ if 2dup __ newstate ! ." StQ:" dup __ ? then
#endif
    __ newstate !
    exit
  then
#if-flag debug
\ yield-trace @ if 2dup __ newstate ! ." StO:" dup __ ? then
#endif
  over state>q  ( state task new_q )
  over __ state @ state>q
  \ Special case: the task is in a queue and the state change sends it to 
  \ some other queue, possibly, we need to handle that
  dup _enq = if drop _deq then
  ( state task new_q old_q )
  2dup <> if
    \old
    \new
  else
    2drop
  then
  ( state task )
  \ the above compares state effects, not states. States may still be
  \ different. But the additional tests are not worth the effort.
#if-flag debug
  tuck
#endif
  __ state !
#if-flag debug
  yield-trace @ if
    ." StN:" __ ?
  else drop then
#endif
;


\ more queue

%queue definitions also

: pop ( q -- item|0 )
\ get the first element
  dup __ next @ .. tuck ( 1st q 1st )
  = if
    drop 0
  else
    dup d-list-item remove
    %cls task-link @ ..
    =idle over %cls state !
  then
;
: one ( q -- )
\ start one task
  __ pop
  ?dup if
    =sched swap  %cls >state
  then 
;

: all ( q -- )
\ start all tasks
\ does not yield
  begin
    dup __ pop
  dup while
    =sched swap %cls >state
  repeat
  2drop
;

: add ( task q -- )
\ insert some task.
  over %cls waitq !
  ( task )
  =wait swap %cls >state
;

: wait ( q -- )
\ insert the current task.
  this .. swap __ add
  yield
;

%queue ignore

\ ********************
\        yield 
\ ********************

#if-flag yieldstack
task \int definitions
%var object: yield-task
#endif

task definitions

: yield ( -- )
\ Switch to the next task.
\ Hooked to "pause" when multitasking.

\ At this point we might be tempted to check whether only one task is
\ running, and return early. However, that is folly because it can only
\ happen if we're the idle task, which does this test itself.

  dint
  \ save current state.
  ctx>r rp@ sp@ this stackptr !
  \ We must not modify the existing stack after this point,
  \ so pretend that there's nothing on it.

  this ..  ( oldtask )
#if-flag debug
  dup %cls abortcode @ 0= if
   dup %cls newstate @ 0= if
    dup %cls pstack @ ?dup if  depth 10 + < if
      ." >TASK:" dup .word
      ." Param stack " depth .
      -3 over %cls abortcode ! then then then
  then
  dup %cls abortcode @ 0= if
   dup %cls newstate @ 0= if
    dup %cls rstack @ ?dup if rdepth 10 + < if
      ." >TASK:" dup .word
      ." Return stack " rdepth .
      -5 over %cls abortcode ! then then then
  then
#endif

  dup newstate @ if  \ state change? if so, do some work
#if-flag yieldstack
    yield-task @ .. ?dup if
      dup >this
      %cls stackptr @ ( old spnew )
      sp+! sp!
      \ on the temp stack we now find ( rp old ). We must not modify rp.
      over rp!
    then ( old )
#endif
    dup %cls next @ ..
    ( oldtask newtask )
    \ ." T:" dup .word   \ debug only, not when multitasking
    dup >this swap ( newtask oldtask )
    \ "this" now contains the new task, so we can safely change the old
    dup %cls newstate @ ( newtask oldtask state )
    over 0 swap %cls newstate ! \ clear new-state flag
    swap %cls >state
  else
    \ no state change, be quick
    %cls next @ .. >this
  then
  ( newtask )

  \ now return to the now-current task
#if-flag debug
  this>dbg
#endif
  this stackptr @ sp! rp! r>ctx \ restore pointers and registers
  eint

#if-flag debug
  yield-trace @ if
    ." >TASK:" this .. .word depth . rdepth . cr
  then
#endif

  \ check new task for pending abort
  this abortcode @ ?dup if
#if-flag debug
    ." aborted" dup . cr
#endif
    0 this abortcode !  throw
  then
  \ ABORT may print, which may yield, which would crash

  \ The abort handler is responsible for clearing this, if warranted
  \ otherwise the abort will be re-thrown next time

  \ ." RET to " r@ hex. ."  RP=" rp@ hex. ."  SP=" sp@ hex. cr
  \ r@ 1 bic disasm-$ ! seec
  \ return to caller
;

:init
  ['] yield  task \int yield-hook !
;

%cls definitions

: >state.i ( state task -- )
\ change task state, disabling interrupts
  eint? dint -rot  ( iflag state task )
  %cls >state
  if eint then
;

: signal ( num task -- )
  dup __ state @ =nsched = if
    dup __ q remove
    =dead over state !
  then  \ no abort handler yet
  dup __ state @ =dead = if 2drop exit then   \ already killed
  tuck __ abortcode !
  =sched swap __ >state.i
;

: end ( task -- )
  -56 swap signal
;


: stop ( task -- )
\ Stop this task (can be woken up later)
  =idle swap __ >state.i
  inline
;

task definitions

: stop ( -- )
\ Stop the current task (if it is marked as running)
  this newstate @  =sched <= if
    =idle this >state.i
  then
  yield
;

: end
  -56 throw
;

: \die
  begin
    =dead this >state.i
    yield
  again
;


\ ********************
\       subtask
\ ********************

\ superclass for tasks with stacks
\ 
task \int also definitions
task also
%cls class: \stk
: setup
  dup __ task-ps over __ pstack @ stackfill
  dup __ task-rs over __ rstack @ stackfill
;
;class

\ superclass for internal non-tasks that need a stack
\stk class: inttask
: setup ( ptr -- )
  \ save the RSP to the PSP
  dup __ task-rs
  over __ task-ps
  sp+!
  \ and the PSP to the task struct
  swap __ stackptr !
;
;class


\ class for everything but the main task

task definitions
\stk class: subtask

200 constant psize  \ this is a "safe" default
200 constant rsize  \ feel free to reduce this *after* testing.
: main@ s" \main" voc-lfa \voc lfa>xt ;


\ Next: setting up the stack for starting a subtask.

\ We can't just push a word's address onto the return stack, that's not
\ portable. Thus we pop a continuation address instead, and rely on the
\ fact that a word consisting solely of a call to R> won't push anything
\ else onto the return stack.

: (cont) r> ;

: (go) ( xt -- does-not-return )
  =sched this state !
\ cr ." RUN:: " dup hex. dup .word  cr this ?
  catch
#if-flag debug
  cr ." END::"
  this .. .word cr
#endif
  dup 0= if drop -1 then
  .abort

  begin
    0 this abortcode !
    =dead this newstate !
    yield
  again
;

: (task)  ( -- go-continue )
\ Main code for tasks
  (cont) (go)
;
: task@ s" (task)" voc-eval ;

: prep ( XT task -- )
\ Prepare stacks so that:
\ The param stack contains
\ - the top of the return stack.
\ - the xt of our main code
\ The return stack contains
\ - dummy values for R4 and R5
\ - the code to "return" to
\ Initially (task) is called, so the param stack also has initial args.
\ This is executed from setup.
#if-flag debug
  ." TASK SETUP " dup .word over ." > " .word cr
#endif

  dup __ task-ps ( xt task SP )

  \ push a-addr onto SP. This (almost) finishes the stack setup for (task)

  over __ task-rs  ( xt task SP RSP )
  \ Store entry address to the top of RSP
  task@ swap sp+!
  \ reserve space for noon-stack CPU state
  #ctx begin  ( xt task SP RSP N )
  ?dup while
    poisoned rot sp+! swap
    1-
  repeat
  \ save xt (should be RUN) to SP
  >r rot swap sp+! r> ( task SP RSP )
  \ Save the RSP to the param stack (YIELD will restore it from there)
  swap sp+! ( task SP )

  \ and finally save SP to the task
  \ ." save " dup hex. ." to " r> hex. cr
  over __ stackptr !
  =new swap __ state !
;

: setup ( object -- )
  __ main@ swap __ prep
;

;class  \ subtask

subtask class: looped 
: (go) ( xt -- does-not-return )
  =sched this state !
  begin
    dup catch 
    ?dup if
      .abort
    then
    yield
  again
;
: (task)  ( -- go-continue )
\ Main code for tasks
  (cont) (go)
;
;class


task \int definitions also
task also

#if-flag yieldstack

inttask class: \ytc
\ one-off class for SWT
100 constant psize
100 constant rsize \ TODO check actual usage
: setup ( ptr -- )
  yield-task !
;
: \main ;
;class

\ This task holds return stack space for "yield" to use when it needs
\ to do some nontrivial work, but is otherwise unused. The task cannot be
\ started at all because the stack pointer is completely wrong
\ytc object: yield-temp

#endif

task %cls definitions

: (start) ( task ctx -- )
\ ." TS:" dup hex. dup .word over hex. over .word over __ state @ . cr
  >r
  dup __ state @ =dead > abort" Task running"
  dup __ state @ =dead = if
    \ the main task cannot be =dead. We hope.
#if-flag debug
    dup __ pstack @ 0= abort" Maintask dead??"
#endif
    \ save the argument, to pass a param to the task
    dup r@  \cls (>setup)
    =new over __ state !
  then
  rdrop
  =nsched swap __ >state.i
;

: start ( task -- )
\ Start a task. If the task is dead we need to re-animate it
\ which is why we save the voc context here.
  \voc voc-context @
  forth state @ if \ compile
    literal, postpone (start)
  else \ interactive
    (start)
  then
immediate ;

: un-idle ( task -- )
\ like continue but only if idle
  dup __ state @ =idle = if
    drop
  else
    =sched swap __ >state.i
  then
;

: continue ( task -- )
  dup __ state @ =dead <= abort" Task not running"
  =sched swap __ >state.i
;

: (wake) ( timer -- )
  %cls task-link @
  continue
;
  
: sleep ( ??s task -- )
  tuck __ q timeout !
  ['] (wake) over __ q code !
  =timer swap __ >state.i
;


\ *****************
\ Create a new task
\ *****************

\ This is a defining word. It creates a task subclass, adds the
\ following code as its "run" method, then creates an instance.

task \int definitions
0 variable \twid

: (task:) ( -- )
\ Part 2 of declaring a named task.
\ run the subclass to set its context
  \voc get-current  \twid @ \voc set-current ( subclass )
  dup \voc (dovoc  \ set temp context to the subclass
  \voc lfa>nfa
  count [with] object: \ make our object
  postpone previous \ take off the task vocabulary
\ \ object setup is in its "setup"
;

task %cls definitions
: :task: ( "name" code??? ; -- )
\ Declare a named task
  \voc get-current \twid !                       \ save our current voc
  [ task \int (' sub literal, ] \voc set-current \ voc for one-off subclasses
  token
  \voc _sop_ @ dup \voc context <> if
    @
  else
    drop [ task (' subtask literal, ]
  then
  \voc (dovoc
  [with] class:
  \ now the one-off subclass is current
  ['] (task:) \voc post-def !               \ second step:
  s" \main" [with] : \                      \ start defining its "\main" method
;

forth definitions
: :task: task %cls :task: ;

task definitions

: sleep ( ??s -- )
  this sleep
  yield
;


\ Supplant the QUIT hook

: quit ( -- does-not-return )
\ call "quit" if we're the main task
\ "task quit" is the same as "quit"
  in-main? if
    [ hook-quit @ call, ]
  then
#if-flag debug
  cr ." S END:" this .. .word
  ct
#endif

  \ subtasks always have a CATCH running
  -56 throw
;

\ *****************
\        wait
\ *****************

: (wait) ( arg xt )
\ wait until ``arg xt execute`` returns true
  this ..
  tuck %cls checkfn !
  tuck %cls checkarg !
  =check swap %cls >state
;

: wait: ( arg "name" )
  ' literal,  postpone (wait)
;


: (irq) ( arg xt )
\ wait until ``arg xt execute`` returns true
  this ..
  tuck %cls checkfn !
  tuck %cls checkarg !
  =irq swap %cls >state
;

: irq: ( arg "name" )
  ' literal,  postpone (irq)
;

#endif
\ part 1


forth definitions only

#if undefined time
#include lib/timeout.fs
#endif
#if time undefined poll
#include lib/timeout2.fs
#endif

forth only
task definitions also

#if undefined !single

: !single ( -- ) [ hook-pause @ literal, ]  hook-pause ! ;
: !multi  ( -- ) task ['] yield hook-pause ! ;
: ?multi  ( -- ) task ['] yield hook-pause @ = ;

task \int definitions also

: i-check ( task -- )
  dup %cls checkarg @
  over %cls checkfn @
#if-flag debug
  ?dup 0= if
    ." OWCH no checker in " dup hex. .word
    exit
  then
#endif
  execute ( task arg xt -- task flag )
  if
    =sched swap %cls >state.i
  else
    drop
  then
  0
;

\ *****************
\     idle task
\ *****************

: busy? ( -- flag )
\ Check if any other task is on our queue
  this .. dup %cls next @ .. <>
;

0 variable last-debug
0 variable loop-debug
0 variable loop-hook
: dbg
  loop-debug @ ?dup if
    1- dup loop-debug !
    0= if
      loop-hook @ ?dup if execute then
      ." DBG:" dup . cr
    then
  else
    last-debug @ over <> if
      dup last-debug !
      \ dup $30 + emit
    then
  then
  drop
;

: run-irqs ( this -- this )
\ walks through the check- and irq-task list

  \ scan the timers
  time check
  ( t2 t1 | 0 )

#[if] defined syscall
  \ don't allow other tasks to starve us
  begin 1 forth poll poll until
#endif

  \ walk the check list
  ['] i-check check-tasks each drop ( n )
  \ "busy" checkers or more work present? exit

  \ disable IRQs and walk the IRQ list
  dint
  ['] i-check irq-tasks each drop ( n )
  \ exit if checkers present or work found

  busy? if  if drop then  eint  2 dbg  exit then

  \ if there are any task checks left, we cannot sleep
  check-tasks empty? not if  if drop then  eint  3 dbg  exit then

#[if] defined syscall
  \ we don't need t2 for polling
  dup if  ( t2 t1 ) nip then ( t1 )
  \ if multitasking is currently off we can't sleep
  ?multi 0= if drop 1  5 dbg  then
  dup dbg
  forth poll poll  drop
#else
  \ the timer code will have arranged an interrupt
  bits tick update if  eint  4 dbg  exit then
  \ if multitasking is currently off we can't halt
  ?multi if
    6 dbg
    \halt
  else
    7 dbg 
  then
#endif
  eint
;

task \int definitions

looped :task: idle
  begin
    run-irqs
    busy? if
      \ do not yield if there's no other task anyway
      yield
    then
  again
;

forth only
task definitions also

\ This task checks whether anything else in the system is running, or wants
\ to run. Otherwise it sleeps, to conserve (some) power.

:init
  task ['] quit hook-quit !
  task \int idle start
  \ !multi
;

\ staying in singletask is temporary: we need to either
\ get a serial IRQ with input buffer, or teach the terminal
\ to wait for the echo.

#endif
\ part 2

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
