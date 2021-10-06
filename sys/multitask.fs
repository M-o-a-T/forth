\ -----------------------------------------------------------
\   Cooperative Multitasking
\ -----------------------------------------------------------

\ This is a mod of the original Mecrips-Stellaris multitasking code.
\ It is double-linked for faster addition to / removal from the 
\ job queue.
\ 
forth definitions only  decimal

#if defined task
\ repeated
#end
#endif

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
#if-flag debug
#include lib/crash.fs
#else
: .word drop inline ;
#endif
#endif

#echo duh
#require class: lib/class.fs
#require d-list-item lib/linked-list.fs
#if-flag debug
#require .word lib/crash.fs
#endif

voc: task
voc: \int

: yield-dummy -29 abort ;
' yield-dummy variable yield-hook

voc: sub
task definitions
sub ignore

0 constant =new  \ new (not linked)
1 constant =dead  \ dead (not linked)
2 constant =idle  \ wait (not linked)
3 constant =sched \ scheduled (in task list)
4 constant =check \ checking (in IRQ list)
5 constant =irq \ checking, IRQ on (in IRQ list)
6 constant =wait \ in some other queue
7 constant =timer \ in some other queue
\ there might be more later
8 constant #states

\ ********************
\         task
\ ********************

\ base class for tasks, may or may not have its own stack, thus internal


task definitions

d-list-head class: %queue
\ Some task queue
: empty?  ( q -- flag )
  dup __ next @ .. swap =
;
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

d-list-item class: task-link
dup constant \link-off
%cls item
: @ ( d-list-adr -- task )
\ pretend that the list item stores a link to the task it's in
  __ \link-off - inline ;

;class

\ back to our class variables
  %cls definitions

  task-link field: link
  var> int field: stackptr
  var> int field: abortptr
  var> int field: abortcode
  var> int field: checkfn

  2dup
  var> int field: checkarg
  2swap
  2dup
  var> uint field: timeout \ for waitq
  2swap
  \int waitq-link field: waitq
#ok 2over d= 
#ok 2over d= 
  var> cint field: state
  var> cint field: newstate
  aligned
__seal

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
: prev __ link prev @ .. ;
task-link item
: next __ link next @ .. ;

: setup ( object -- )
  dup __ link >setup
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
  dup __ link .. d-list-head >setup

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

: in-main?  ( -- flag )
\ are we in the main task?
  \ ." M:" this-task @ hex. this .. hex. main .. hex. cr
  this .. main .. =
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
  swap %cls link .. swap  \ address of the link
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
  __ each
  ( xt flag )
  nip
;

\ duplicate from lib/linked-list.fs, unfortunately
: each: ( head "name" -- )
\ run NAME with each item
  postpone .. ' literal, postpone swap postpone each  
  immediate
;

;class

\ There are other lists. Specifically we need one for tasks that are waiting.
%queue object: irq-tasks  \ tasks in =irq
%queue object: check-tasks  \ tasks in =check



#if-flag debug
#include debug/multitask.fs
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

\ *** state transition: away from …
: _table: <builds does> + c@ ;

0 constant _err
1 constant _no
2 constant _run
3 constant _chk
4 constant _irq
5 constant _enq
6 constant _deq
7 constant _ent

_table: state>q
_no  c, \ =new
_err c, \ =dead
_no  c, \ =idle
_run c, \ =sched
_chk c, \ =check
_irq c, \ =irq
_enq c, \ =wait
_ent c, \ =timer

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
        over time remove
        endof
      drop over __ link remove  \ task is queued, so dequeue
      0
    endcase
;
: \new  ( state task new_q -- state task )
    case
      _run of
        dup __ link .. this link insert
        endof
      _irq of
        dup irq-tasks insert
        endof
      _chk of
        dup check-tasks insert
        endof
      _ent of
        dup time insert
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
    __ newstate !
    exit
  then
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
  __ state !
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

forth definitions
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

\ Override standard words so they work with tasks

\voc sticky
: .s
  \ can't patch our "depth" into the core, so …
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

task definitions

: yield ( -- )
\ Switch to the next task.
\ Hooked to "pause" when multitasking.

\ At this point we might be tempted to check whether only one task is
\ running, and return early. However, that is folly because it can only
\ happen if we're the idle task, which does this test itself.

#if-flag debug
  eint? not abort" yield while DINT"
#endif

  dint
  ctx>r rp@ sp@ this stackptr !
  \ save current return and stack pointer. Don't go below sp@

  ( oldtask )
  this .. dup newstate @ if  \ state change? if so, do some work
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
  this stackptr @ sp! rp! r>ctx \ restore pointers and registers
  eint

  \ check new task for pending abort
  this abortcode @ ?dup if throw then
#if-flag debug
  \ ." >TASK:" this .. .word depth . rdepth . cr
  this pstack @ ?dup if  depth  5 + < if
    ." >TASK:" this .. .word
    ." Param stack tight" -3 abort then then
  this rstack @ ?dup if rdepth 10 + < if
    ." >TASK:" this .. .word
    ." Return stack tight" -5 abort then then
#endif
  \ The abort handler is responsible for clearing this, if warranted
  \ otherwise the abort will be re-thrown next time

  \ ." RET to " r@ hex. ."  RP=" rp@ hex. ."  SP=" sp@ hex. cr
  \ r@ 1 bic disasm-$ ! seec
  \ return to caller
;

:init
  ['] yield  dup h.s drop  task \int yield-hook !
;

: caught ( -- )
\ clear the current task's abort code
  0 this abortcode !
;

%cls definitions

: >state.i ( state task -- )
\ change task state, disabling interrupts
  eint? dint -rot  ( iflag state task )
  %cls >state
  if eint then
;

: \kill ( task -- )
  =sched swap __ >state.i
  inline
;

: signal ( num task -- )
  dup __ state @ =dead = if 2drop exit then   \ already killed
  tuck __ abortcode !
  =sched swap __ >state.i
;

: stop ( task -- )
\ Stop this task (can be woken up later)
  =idle swap __ >state.i
  inline
;

task definitions

: stop ( -- )
\ Stop the current task (can be woken up later)
  =idle this >state.i
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

\ class for everything but the main task

task %cls class: subtask
task also
task \int also

100 constant psize  \ this is a "safe" default
100 constant rsize  \ feel free to reduce this *after* testing.
: main@ s" \main" voc-lfa \voc lfa>xt ;


\ Next: setting up the stack for starting a subtask.

\ We can't just push a word's address onto the return stack, that's not
\ portable. Thus we pop a continuation address instead, and rely on the
\ fact that a word consisting solely of a call to R> won't push anything
\ else onto the return stack.

: (cont) r> ;

#if-flag debug
: (go) ( xt -- does-not-return )
  begin
    dup
    cr ." RUN: " dup hex. dup .word  cr this ?
    catch
    0 this abortcode !
    cr ." END:"
    ?dup if dup . else -1 then
    this .. .word cr
    dup \voc abortcode !
    .abort
    this abortcode !
    =dead this newstate !
    yield
  again
;
#else
: (go) ( xt -- does-not-return )
  begin
    dup catch
    0 this abortcode !
    dup 0= if drop -1 then
    dup \voc abortcode !
    .abort
    this abortcode !
    =dead this newstate !
    yield
  again
;
#endif

: (task)  ( -- go-continue )
\ Main code for tasks
  (cont) (go)
;
: task@ s" (task)" voc-eval ;

\ Helper: decrement a stack pointer and store data there.
: sp+! ( data sp -- sp-1 )
  \ ." save " over hex.
  1 cells -
  \ ." to " dup hex. cr
  tuck ! inline ;

: sfill ( addr cells -- )
\ fill a stack from the bottom
\ this overwrites the cell which the address points to. That address is not
\ part of the stack; we do this for stack underrun protection
  0 ?do
    poisoned over !
    1 cells -
  loop
  drop
;

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
  swap __ stackptr !
;

: setup ( object -- )
  dup __ task-ps over __ pstack @ sfill
  dup __ task-rs over __ rstack @ sfill

  __ main@ swap __ prep
;

;class  \ subtask

subtask class: looped 
: (go) ( xt -- does-not-return )
  begin
    dup catch 
    ?dup if
      this abortcode !
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


task %cls definitions

: (start) ( task ctx -- )
  >r
  dup __ state @ =dead > abort" Task running"
  dup __ state @ =dead = if
    \ the main task cannot be =dead. We hope.
#if-flag debug
    dup __ pstack @ 0= abort" Maintask dead??"
#endif
    dup r@  \cls (>setup)
    =new over __ state !
  then
  rdrop
  =sched swap __ >state.i
;

: start
\ Start a task. If the task is dead we need to re-animate it
\ which is why we save the voc context here.
  \voc voc-context @
  forth state @ if \ compile
    literal, postpone (start)
  else \ interactive
    (start)
  then
immediate ;

: sleep ( µs task -- )
  tuck __ timeout !
  =timer swap __ >state.i
;

: continue ( task -- )
  dup __ state @ =dead <= abort" Task not running"
  =sched swap __ >state.i
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
  get-current  \twid @ set-current ( subclass )
  dup \voc (dovoc  \ set temp context to the subclass
  \voc lfa>nfa
  count [with] object: \ make our object
  postpone previous \ take off the task vocabulary
\ \ object setup is in its "setup"
;

task %cls definitions
: :task: ( "name" code… ; -- )
\ Declare a named task
  get-current \twid !                     \ save our current voc
  [ task \int (' sub literal, ] set-current \ voc for one-off subclasses
  token
  \voc _sop_ @ dup \voc context <> if
    @
  else
    drop [ task (' subtask literal, ]
  then
  \voc (dovoc
  [with] class:
  \ now the one-off subclass is current
  ['] (task:) \voc post-def !               \ second step
  s" \main" [with] : \                      \ … start defining its "\main" method
;

forth definitions
: :task: task %cls :task: ;


task definitions

: sleep ( µs -- )
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
#endif

  \ subtasks always have a CATCH running
  -56 throw
;

:init
  task ['] quit hook-quit !
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


forth definitions only
#if time undefined poll
#include lib/timeout2.fs
#endif

\ *****************
\     idle task
\ *****************

forth only
task definitions also

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

: busy? ( this -- this flag )
\ Checks if all other tasks are currently in idle state
  dup %cls next @ .. over <>
;

: run-irqs ( -- )
\ walks through the check- and irq-task list
#[if] defined syscall
  0 time poll drop
#endif

  this ..  \ for busy?
  \ walk the check list
  check-tasks each: i-check drop ( n )
  \ "busy" checkers or more work present? exit

  \ disable IRQs and walk the IRQ list
  dint
  irq-tasks each: i-check drop ( n )
  \ exit if checkers present or work found
  busy? if eint exit then
  check-tasks empty? not if eint exit then
#[if] defined syscall
  \ check timeouts and epoll
  ?multi time poll drop
#else
  \ the timer code will have arranged an interrupt
  \halt
#endif
  eint
;

task \int definitions

looped :task: idle
  this ..
  begin
    run-irqs
    busy? if
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

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
