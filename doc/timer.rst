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

\*delay ( n -- n*delay )
------------------------

Measure the round-trip through ``yield`` N times.

This word leaves N hopefully-small integers (in microseconds) on the stack,
for the caller to print / average / maximize / agonize about.

++++++
Timers
++++++

Timers are tied to tasks. Any task can be in the "wait for some time to
pass" state.

If you want to wait for a timer *or* some event, get an interrupt to
trigger on the event; its code should set a flag and then simply wake up
your task prematurely, with ``your-task continue``. This is interrupt safe.

time micros ( n -- )
--------------------

Wait for this number of microseconds to pass. In the interest of forward
compatibility, please pretend that it can't do more than 50000 µs (50 ms).

Don't confuse this word with Arduino; there, the function with this name
would simply return a microsecond counter. We don't do that.

Don't assume that this is in any way exact; your value is a lower bound.
Check the data from ``*delay``, or write your own delay estimator along
its lines, for the actual accurracy of ``micros``.

time millis ( n -- )
--------------------

Same, but milliseconds. In the interest of forward compatibility, please
pretend that it's no good for delays of more than a minute (60000).

Don't confuse this word with Arduino; there, the function with this name
would simply return a millisecond counter. We don't do that.

time seconds ( n -- )
---------------------

Same, but seconds. Don't use this for delays (much) longer than 60 seconds.
The real upper bound is likely to be higher, but the details vary depending
on the implementation.

time minutes ( n -- )
---------------------

Same, but minutes. Don't use this for delays (much) longer than 60 minutes.
Again, the actual limit may be higher.

time hours ( n -- )
-------------------

Same, but hours.

There is no reasonable upper bound for ``n``; at most, it is internally
converted to seconds, and your system will not run continuously for 136
years.

time days ( n -- )
------------------

Not implemented. Let's be real – I *told* you there's no upper limit for
``hours``, didn't I? Thus, you can write ``24 * time hours`` yourself,
should you need it.
