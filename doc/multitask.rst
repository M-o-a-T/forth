============
Multitasking
============

This library implements cooperative multitasking.

There is no provision (yet) for more than one CPU.

++++++++++++++++
Task declaration
++++++++++++++++

Declaring a (simple) task is very easy.
Just write ``:task:`` instead of ``:``::

    0 variable: runaway
    :task: foo
      begin
        runaway @ 1+ runaway !
        pause
      again ;

Your task will initially be "idle" state, i.e. it won't not run until
you tell it to.

+++++
Words
+++++

Affecting other tasks
=====================

start ( task -- )
+++++++++++++++++

Starting a task is easy::

    foo start

Alternately::

    :init
      foo start
    ;

will also remember to start the task after a reset, if you're compiling
into Flash.

continue ( task -- )
++++++++++++++++++++

CONTINUE does the same thing as START, except to a task that has already
been running.

The difference is strictly from a PoV of code correctness. It never makes
sense to start-or-continue a task; you either know it's new and (re)start
it, or you know it is running, then you continue it.

\kill ( task -- )
+++++++++++++++++

This immediately kills a task. It is now dead and will no longer be scheduled.

This is an internal method. There's a reason: it is almost never a good idea to
stop a task "cold": the task may contain a ``catch`` that clean up some
resource or peripheral that the task uses. You really should use SIGNAL
instead.

signal ( num task -- )
++++++++++++++++++++++

Tell the task to THROW this number when it's next scheduled.

Signal numbers are a matter of convention, though negative values are
reserved for the system. Using a signal zero is not allowed and will in
fact abandon a pending signal.

Affecting the current task
==========================

task yield
++++++++++

Interrupt the current task so others can get a turn.

Call this periodically if you're doing a nontrivial calculation.

task stop ( -- )
++++++++++++++++

Halt the current task until it is continued.

quit ( -- does not return )
+++++++++++++++++++++++++++

In subtasks, throw a marker value, which signals to terminate the task
without emitting an error message.

In the main task, fall back to the original QUIT handler.

task \die ( -- does not return )
++++++++++++++++++++++++++++++++

Immediately stop the current task.

This is meant for emergencies, like "I realized I have overrun my stack".

Do not otherwise call this if at all possible.

task caught ( -- )
++++++++++++++++++

If there's a pending signal, the task handler will re-throw it every time
your task is scheduled. Your ``catch`` code needs to clear it when it has
handled the signal or error.

+++++++++++++
Task creation
+++++++++++++

``:task:`` does basically this, behind the scenes::

    task \int sub definitions
    task also

    subtask class: NAME
    : \main 
      \ your code here
    ;
    back-to-your-vocabulary definitions
    task \int sub NAME object: NAME

You're free to do the same thing yourself, e.g. when you need per-task variables.

You can also prefix ``:task:`` with your own class, subclassing the ``task
subtask`` class. This is relevant if you need a task with larger stacks::

    subtask class: big-task
    50 constant psize
    50 constant rsize
    ;class

    big-task :task: 
      begin
        do-something-very-involved
      again
    ;

NB: Smaller stacks are generally not recommended. In debug mode you can
check a task's maximum stack using ``TASK ?``.


+++++++++++
Task states
+++++++++++

=new
====

The task has not been started. You can start it with ``NAME start``.

=dead
=====

The task has ended. You can restart it with ``NAME start``.

=idle
=====

The task has been started but is not doing anything. It can be continued
with ``NAME go``. This is intentionally not the same word as above.

=sched
======

The task is on the list of running tasks. You can check whether your code
is currently executing the task in question with ``NAME .. task this =``.

You can check for a pending signal with ``NAME abortcode @``.

=check
======

The task is idle, but the idle task will periodically run a check function
to query whether to restart it.

=irq
====

The task is idle. It may be made runnable by an interrupt.

A check function is still required; it ensures that the interrupt is not
yet pending, to prevent deadlocks.

The difference between ``=check`` and ``irq`` is that if there is no
running task and all check words return ``=irq``, the system may enter
some sleep state.

Your interrupt handler should continue the task. If that is difficult to
achieve, however, it is sufficient (though slower) to disable the interrupt
source and then defer the actual task start to your check word.

=wait
=====

The task has been added to a wait queue.

+++++++
Waiting
+++++++

A task can wait for something; when it does, it's important to not waste
time switching to that task's context unnecessarily.

One basic principle of this library is to avoid busy waiting, i.e.
tasks that loop calling ``pause`` until some condition is satisfied.
This approach wastes power and slows down your system due to unnecessary
context switches.

Thus we need to consider different reasons why a task might want to
continue its work.

Wait queues
===========

Examples:

* Task A is finished producing a result B is waiting for

* Task C writes to a buffer which is full / task D reads from a buffer
  that's empty

To handle this case, we use wait queues. They can be used independently, or
as members of another data structure::

    class: ring
    __data
      …
      task %queue field: waiters
    __seal
    : setup
      dup __ waiters >setup
      …
    ;

The code to read an item from this structure might then be written like this::

    : @ ( ring -- item )
      begin
        dup __ empty?
      while
        dup __ waiters wait
      repeat
      \ now get the actual data
    ;

while writing to it might look somewhat like this::

    : ! ( item ring -- )
      \ write the actual data
      ( ring ) 
      __ one \ wake up one reader
    ;

You always need to loop on the condition because it could be false again by
the time the scheduler gets around to your task.

You might need to do the same thing in reverse for the "buffer full"
condition.

Words
+++++

one ( queue -- )
----------------

Wake up one task from the queue, if there is one.

Currently this is the first task, but you should not depend on that.

all ( queue -- )
----------------

Wake up all tasks from the queue, emptying it.

This basically calls ``pop`` until the queue is empty. New tasks arriving
during execution of this word, perhaps due to an interrupt, are also
(re)started.

wait ( queue -- )
-----------------

Insert the current task into the queue.

add ( task queue -- )
---------------------

Insert some other task into the queue.

External signals, no interrupt
==============================

This situation looks like busy waiting. However, it uses a separate check
function to monitor the signal which doesn't require a separate task switch.

To do this, you register a check word. That word is periodically run by the
idle task and will re-enable your task when it is ready.

A simple example::

    : xkey? drop key? ;
    :task: echo
      begin
        0 task wait: xkey?
        key emit
      again

    :init
      echo start
    ;

The check word must consume the argument (zero, in this example) and return
a flag whether to schedule the task.

Check functions must be short and to the point. They must never call
``throw`` and cannot themselves wait for anything. However, we pass the
address of "their" task to them, thus they may change its state
themselves if necessary::

    task also
    : deadpoll ( task arg -- task flag )
      drop
      42  over %cls signal
      0
    ;

This is helpful e.g. when the check function reads a status register. it
can decide whether to proceed or abort its task based on the register's
error flags.

Whenever a check function is active, the system will not be allowed to
sleep. If possible, you should register an IRQ function instead.


Words
+++++

wait: ( arg "name" )
--------------------

Sleep until calling the named word (with the argument on the stack) results
in non-zero.

The signature of the word NAME must be ``( taskptr arg -- taskptr flag )``.

NAME is called with interrupts enabled.

(wait) ( arg xt )
-----------------

As ``wait:``, but expects the word's execution token on the stack instead
of searching for it.


External signals, interrupts
============================

This is the ideal situation for handling external signals because the
system is able to halt the processor if no other work is going on.

Interrupt handling is a multi-step process. It somewhat differs depending
on whether the CPU has level- or edge-triggered interrupts.

Level-triggered means, basically, that if you interrupt handler doesn't
disable the interrupt somehow it will be called again immediately
thereafter. If your handler doesn't disable the interrupt, your system will
become unresponsive.

Edge-triggered interrupts, on the other hand, only fire once. If your
handler doesn't disable the interrupt, your system will not recognize it
again and will become unresponsive, albeit only with respect to this
particular event instead of in general.

Interrupt handling
++++++++++++++++++

First, install an interrupt handler. Consult your CPU manual on which
interrupt to use, set the corresponding ``irq-*`` variable to your handler,
then set up your hardware to produce interrupts.

Second, write a check word. This is particularly important for
edge-triggered interrupts, even more so when they may have multiple
sources.

Interrupt check words are called with interrupts disabled. Their job is
to ensure that no interrupt is missed (the interrupt handler is not called)
and your task continues (the interrupt handler executed already).

Then, your task should enable the device's interrupt and wait.

Interrupt handling is fraught with race conditions.
Consider this situation:

* You install an interrupt handler.

* You enable the interrupt.

* The condition is met instantly, the handler runs. It does whatever needs
  doing and disables your interrupt.

* Your main code tells the system to wait for the interrupt. As that already
  happened, your code is not scheduled.

We mitigate this by teaching the check word to also return ``true`` if the
interrupt already happened.

Words
+++++

irq: ( arg "name" -- flag)
--------------------------

Sleep until calling the named word (with the argument on the stack) returns
a non-zero.

The signature of the word NAME must be ``( taskptr arg -- taskptr flag )``.

NAME is called with interrupts disabled.


(irq) ( arg xt )
-----------------

As ``irq:``, but expects the word's execution token on the stack instead
of searching for it.


++++++++++++++++++
Differences to F83
++++++++++++++++++

F83 has a ``task:`` word that establishes a memory range for stacks plus
user area, and an inline ``activate`` that returns from within a word but
starts a task for running the rest of it.

That's a sub-optimal idea for a couple of reasons.

* the return stack size is fixed, which wastes memory.

* jumping out of a possibly-complex word is dangerous.

* why would you want different words to refer to a task vs. the way you
  start it?

* structured per-task storage would be nice.

* what happens when your word aborts, or runs off the end? Answer: Your
  program crashes. Forcing every task's main word to handle that by itself
  ends up being buggy and wastes memory.

* you want to introspect which tasks are doing what.
