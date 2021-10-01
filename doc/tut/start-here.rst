Let's start off with …
======================

… actually writing the documentation before we try to collect it into a
coherent whole, shall we?

There are a few TODOish holes in this file.

Sorry about that.

Anyway. I'll not teach you how to code in Forth in general. An old but
still very good primer is `Starting FORTH
<https://www.forth.com/starting-forth/>`_, by Leo Brodie.

I'll also not teach you about `Mecrisp Stellaris
<https://mecrisp.sourceforge.net/>`_, the Forth variant this
library is (mostly) based on. You can read more there.

Instead, let's talk about the stuff we're doing here, and why we're doing
it. I'll start from the ground up.

Vocabularies
++++++++++++

Let's face it, Forth is good at abstracting things but abysmal at
separating different abstractions from each other, because it only has one
flat vocabulary.

Thus we split that up, mostly by using somewhat-mangled code from Manfred
Mahlow that's included in the Mecrisp Stellaris distribution.

`Check here </doc/voc.rst>`_ how to use (our version of) vocabulary code.

Object-oriented Forth
---------------------
An extension to that vocabulary idea gives us something like classes and
objects. They're admittedly far from a perfect implementation of these
particular concepts, but this author thinks that our code work well enough
to get most of the benefits to be worth the (mostly-syntactic) trouble.
`Here </doc/classes.rst>`_ is how you use them.

Setup and INIT
--------------

Setting things up on a Forth system that uses Flash storage typically
involves writing an ``init`` word. The system startup code then finds the
latest incarnation of ``init`` and runs that.

This is sub-optimal; we can do better. See `System Initialization
</doc/init.rst>`_ for both the why and the how-to.

Multitasking
++++++++++++

The `MoaT Bus <https://github.com/M-o-a-T/moat-bus>`_ is about teaching
reasonably stupid small embedded computers, meaning small STM-32 CPUs,
to perform reasonably complicated jobs when connected to a reasonably fast
new wired bus.

These complex jobs typically have some state, which corresponds to external
devices (think I²C or 1wire bus slaves) we talk to. So we send a bus
message, the device talks to a bus, which (a) can go wrong and (b) during
which the thing can't do anything else.

That's bad. So we need some sort of either explicit state machines (implying
that we need to save their state somewhere) which we can interleave with
other state machines, or multitasking: then the state is implicitly stored
on the tasks' stacks.

Sorry to all state machine fans out there, but people are notoriously bad
at keeping multiple nested state machines straight, thus we do the
multitasking thing. Fortunately, Forth in general is very good at
multitasking: just switch to a different pair of stacks.

The devil is in the detail, but Forth is also very good at abstracting
details. See `Multitasking </doc/multitask.rst>`_ for, well, the details.

Waiting and Interrupts
----------------------

A state change on a wire triggers an interrupt, which should be processed
without disturbing other tasks. Worse, some changes can't cause an
interrupt, how do we handle these?

Answer: Reasonably well. We hope. The `Multitasking`_ documentation
contains the details for that, too.

Sometimes, though, the state is internal (as in "wait for a buffer to
contain something"). We use doubly-linked lists to manage waiting tasks
(and all the others). These lists can be re-used by your code; see `Linked
lists </doc/linked-list.rst>`_ for details.

Buffers
+++++++

A message arrives. You start to handle it; while you do that another
message trundles in, and while *that* happens the user types a command to
your serial input.

You can't create a new task for every incoming message (OK yes you could
but you'd run out of RAM, and they'd block each other anyway), much less
every incoming byte (tasks can finish out of order, so that would be bad),
so we give you a `ring buffer </doc/ring.rst>`_ to handle them.

The nitty-gritty
++++++++++++++++

All the little stuff that doesn't know where to go goes to our `Utilities
</doc/utils.rst>`_ file.

The Terminal
++++++++++++

Last but not least, there's the question of how all that code gets to the
microcontroller in the first place. The Forth core is flashed onto it, but
then?

The answer is a terminal program. One that is reasonably intelligent so
that you can teach it to assemble your main program just by a couple of
flags, or perhaps a configuration file. Learn about ours `here
</doc/terminal.rst>`_.

