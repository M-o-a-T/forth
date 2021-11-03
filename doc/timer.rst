======
Timers
======

Tasks need to be able to suspend themselves for some time. For instance,
you might want to hold an output low for more-or-less-exactly one second,
or raise an error when an input doesn't get released after some time.

Words
=====

now-hq ( -- u )
---------------

Return the current system timer, in microseconds.

This value is an unsigned number which wraps around. You should only
compare differences between times, never times directly.

The value returned by ``now-hq`` is as accurate as possible given the
system tick's frequency.

now ( -- u )
------------

Like ``now-hq`` but with lower overhead and accuracy.

The returned value will lag behind ``now-hq`` by some random number of
microseconds that depends on system load and any other task that insists on
using ``now-hq``, but it will never go entirely out of sync.

It is guaranteed to be updated during ``yield``.

setclk ( freq -- )
------------------

Tell the timing system that ``freq`` is the (new) frequency of the system's 
input to the ``SysTick`` counter.

As there are a myriad of ways to configure the system clocks and their
external input(s), if any, this timer module does not even pretend to try
infering the actual clock from the system's registers.

\*delay ( n -- n*delay )
------------------------

Measure the round-trip through ``yield`` N times.

This word leaves N hopefully-small integers (in microseconds) on the stack,
for the caller to print / average / maximize / agonize about.

++++++
Timers
++++++

This system supports explicit timers.

With multitasking, the idle loop checks the timers on each pass.

Explicit timers
===============

The class ``time %timer`` has one publicly accessible fields:

code
++++

This field holds the XT of the word to execute when the timer expires.

Timeouts are queued to the system using this word:

callback ( timer -- )
---------------------

The word called when the timer expires. It receives the timer on the stack.

There is no way to pass additional parameters; if you need space to store
them, simply extend the class.


add ( timeout timer -- )
++++++++++++++++++++++++

This word adds the timer to the timeout queue. Its runtime depends on the
number of timers with shorter timeout.

``timeout`` is unsigned and in microsecond units.


remove ( timer -- )
+++++++++++++++++++

Unqueue the timer. The runtime is linear.

The remaining timeout is not returned. Use ``now`` to calculate it
yourself if required.


Tasks
=====

An implicit timer control block is embedded in each task; thus you can
delay a task by simply calling ``N millis`` or another delay word.

If you want to wait for a timer *or* some event, get an interrupt to
trigger on the event; its code could set a flag and then simply wake up
your task prematurely, with ``your-task continue``. This is interrupt safe.

time micros ( n -- )
++++++++++++++++++++

Wait for this number of microseconds to pass. In the interest of forward
compatibility, please pretend that it can't do more than 50000 µs (50 ms).

Don't confuse this word with Arduino; there, the function with this name
would simply return a microsecond counter. We don't do that.

Don't assume that this is in any way exact; your value is a lower bound.
Check the data from ``*delay``, or write your own delay estimator along
its lines, for the actual accurracy of ``micros``.

This is equivalent to ``task sleep``.

time millis ( n -- )
++++++++++++++++++++

Same, but milliseconds. In the interest of forward compatibility, please
pretend that it's no good for delays of more than a minute (60000).

Don't confuse this word with Arduino; there, the function with this name
would simply return a millisecond counter. We don't do that.

time seconds ( n -- )
+++++++++++++++++++++

Same, but seconds. Don't use this for delays (much) longer than 60 seconds.
The real upper bound is likely to be higher, but the details vary depending
on the implementation.

time minutes ( n -- )
+++++++++++++++++++++

Same, but minutes. Don't use this for delays (much) longer than 60 minutes.
Again, the actual limit may be higher.

time hours ( n -- )
+++++++++++++++++++

Same, but hours.

There is no reasonable upper bound for ``n``; at most, it is internally
converted to seconds, and your system will not run continuously for 136
years.

time days ( n -- )
++++++++++++++++++

Not implemented. I *told* you there's no upper limit for
``hours``, didn't I? Thus, you can write ``24 * time hours`` yourself,
should you need it.

+++++++++++++++++++++++
Suspending another task
+++++++++++++++++++++++

TASK sleep ( µs task -- )
-------------------------

Suspends another task for the given time.

Unlike ``task sleep``, if you apply this to the current task (i.e. you
write ``task this sleep``,), your task will not be suspended until the next
time you call ``yield``.

You commonly use this on a task that's already suspended, thus telling it
to start some time later. One possible use case for this is error handling
/ recovery.

Common patterns on how to do that will be described later.

+++++++++++++++++++
Non-sleeping timers
+++++++++++++++++++

You might want to time-limit some activity. There are two distinct
use cases for this:

* yield after some time (but not *too* often) to let other tasks have some
  CPU time

* work for some amount of time, then e.g. complain that nothing happened 😒

The first case is supported by ``nyield-reset`` and ``nyield?``, described below.

As for the second case, the main thing to remember is never to compare
specific timestamps (as returned by ``now``) directly. *Always* compare
intervals. Thus you might write::

	: work
	  now
	  begin
	    do-some-work
		30 nyield?
		now over - TIMEOUT >
	  until
	  drop nyield-reset
	;

A different way to do this would be::

	:task: worker
	  begin
	    do-some-work
		30 nyield?
	  again
	;
	:task: killer
	  TIMEOUT time micros \ or whateher
	  12345 worker signal \ or whatever
	;
	worker start  killer start

In this case, of course, using one task just to limit another task's run
time doesn't make sense. However, this strategy might be more appealing if
your worker has multiple points which call ``yield``, and/or cannot easily
be adapted to ``do-some-work``-ish chunks.

nyield-reset ( -- )
===================

Clear the *n-yield* counter. You should do this at the end of your work,
in order to not leave it in an inconsistent state.

?nyield ( n -- )
================

This word calls ``yield`` after every ``n``\th invocation. This is a
low-overhead way of ceding runtime to other tasks; repeatedly calling
``now-hq`` to determine out whether to yield control is hideously expensive
by comparison.

.. note::
	The second example above doesn't call ``nyield-reset`` because when the
	killer task runs, the worker task has yielded. As ``?nyield`` always
	leaves the counter at zero when it does yield, there's nothing to
	reset.

If you have a loop counter (or can put one onto the stack without too much
overhead), this style ::

    100000 0 do … i 50 mod 0= if yield then loop

is even less expensive. However, if you can't do it that way easily,
``?nyield`` is a good option.
