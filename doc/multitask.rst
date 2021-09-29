============
Multitasking
============

This library implements cooperative multitasking.

There is no provision (yet) for more than one CPU.

Task declaration
================

Declaring a (simple) task is very easy.
Just write ``task:`` instead of ``:``::

    0 variable: runaway
    task: foo
      begin
        runaway @ 1+ runaway !
        pause
      again ;

Your task will initially be "idle" state, i.e. it won't not run until
you tell it to.

Affecting other tasks
=====================

start ( task -- )
+++++++++++++++++

Starting a task is easy::

    foo start

Alternately::

    init:
      foo start ;

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

Task creation
=============

``task:`` does basically this, behind the scenes::

    task \int sub definitions
    task also

    subtask class: NAME
    : \main 
      \ your code here
    ;
    back-to-your-vocabulary definitions
    task \int sub NAME object: NAME

You're free to do the same thing yourself, e.g. when you need per-task variables.

You can also prefix ``task:`` with your own class, subclassing the ``task
subtask`` class. This is relevant if you need a task with larger stacks::

    subtask class: big-task
    50 constant psize
    50 constant rsize
    ;class

    big-task task: 
      begin
        do-something-very-involved
      again
    ;

NB: Smaller stacks are generally not recommended. In debug mode you can
check a task's maximum stack using ``TASK ?``.


Task states
===========

=new
++++

The task has not been started. You can start it with ``NAME start``.

=dead
+++++

The task has ended. You can restart it with ``NAME start``.

=idle
+++++

The task has been started but is not doing anything. It can be continued
with ``NAME go``. This is intentionally not the same word as above.

=sched
======

The task is on the list of running tasks. You can check whether your code
is currently executing the task in question with ``NAME .. task this =``.

You can check for a pending signal with ``NAME abortcode @``.

=check
++++++

The task is idle, but the idle task will periodically run a check function
to query whether to restart it.

=irq
++++

The task is idle. It may be made runnable by an interrupt.

A check function is still required; it ensures that the interrupt is not
yet pending, to prevent deadlocks.

The difference between ``=check`` and ``irq`` is that if there is no
running task and all check words return ``=irq``, the system may enter
some sleep state.

Your interrupt handler should continue the task. If that is difficult to
achieve, however, it is sufficient (though slower) to disable the interrupt
source and then defer the actual task start to your check word.


Waiting
=======

A task can wait for something; when it does, it's important to not waste
time switching to that task's context unnecessarily.

This is afforded by using a check function. That function is run by the idle
task and will re-enable your task when it is ready.

A simple example::

    : xkey? key? if task =sched else task =check then ;
    task: echo
      begin
        task wait: xkey?
        key emit
      again

    init:
      echo start
    ;

This word must return the new task state. It will see your task structure
on the stack, but it must leave it there.

Check functions might run with interrupts disabled. They must be short and to
the point. They must never call ``throw`` and cannot wait for anything.

If the check function returns ``=dead``, the task will be ``signal``\led.

The check function is executed immediately. If it returns ``=sched`` the
task is not suspended and ``wait:`` returns immediately.

