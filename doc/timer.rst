======
Timers
======

Tasks need to be able to wake up after some time. For instance, you might
want to hold a signal for a second, or raise an error when a signal doesn't
get released after some time.

There are thus two kinds of timer: those you can check yourself because you
have something to do anyway, and those that the system manages for you.

++++++++++++++++
Self-timed tasks
++++++++++++++++

The system maintains a "now" variable which, when read, returns a
microsecond tick value, which. yields timers of more than an hour. Simply
take the current value, subtract your starting value from it, and compare
the result to your target. (All in unsigned arithmetic of course.)

+++++++++++
Timed tasks
+++++++++++

If you want a task to sleep for some time, just say ``DELAY task sleep``.

Words
=====

task sleep ( µs -- )
---------------------

Suspends the current task for the given time.

TASK sleep ( µs task -- )
-------------------------

Suspends another task for the given time.

Unlike ``task sleep``, if you apply this to the current task (i.e. you
write ``task this sleep``,), your task will not be suspended until the next
time you call ``yield``.

now
---

Return the current system timer, in microseconds.

``-1`` is skipped.

+++++++++++++++
Longer timeouts
+++++++++++++++

time hour
---------

This queue is triggered by a background task every hour. Just wait on
it in a loop::

	:task: daily
	  begin
	    \ do whatever
	    24 0 do  time hour wait loop
	  again
	;

time next-hour
--------------

This variable contains the ``now`` value of the next time when all jobs in
the ``time hour`` queue will be started. You can use it to calculate how
long to delay, for more accurate long-term timing.

time hours
----------

This helper delays your task for this number of hours, exactly. Your system
is unlikely to run continuously for a quarter million years (OK, 245146,
but still), so there's no practical upper limit.

time minutes
------------

Same, but minutes. Don't use this for delays longer than 60 (OK, actually
71) minutes.

time seconds
------------

Same, but seconds. Don't use this for delays longer than 60 seconds. OK,
the real number is 4294, but let's pretend you didn't read that.

