======================
So, where do we start?
======================

++++++++++++
Give it a go
++++++++++++

Get Mecrisp Stellaris. You can download the latest release (including
pre-built binaries) it from Sourceforge, or clone MoaT's archive from `its
Github copy <git@github.com:M-o-a-T/mecrisp-stellaris.git>`_ and build it
yourself.

Install the ``qemu-user-static`` package.

Run ``scripts/mf-cmd -x test/main.fs -F multi -F debug  -D -g ok.
../mecrisp-stellaris/linux-ra/mecrisp-stellaris-linux``. This starts a
Linux version of Mecrisp Stellaris, force-feeds it a lot fo additional
Forth code, and then reports "END OF TEST. SUCCESS.". At this point you
can experiment with all the festures described in the rest of this
document.

For running our code on Real Hardware you need a Mecrisp
Stellaris-compatible controller with at least 64k of Flash, and a serial
adapter (if your development board doesn't already include it).

Our Flash script expects RTS to be connected to the controller's ``RESET``
pin and DTR to ``Boot0``. You can do that manually (i.e. remove the Boot
jumper and/or press the Reset button) if necessary.

Run ``env
MECRISP=../mecrisp-stellaris/stm32f103-ra/mecrisp-stellaris-stm32f103.bin
scripts/flash /dev/ttyUSB0 -x test/main.fs -F real  -D -g ok.``. With any
luck, this should reset your board to Flash mode, flash Mecrisp, then feed
a non-debug and non-multitask version of the core our code to it.
(Multitask support is on the TODO list; the multitask mainloop can't
poll the serial port fast enough.)

You can send additional files by typing Control-T followed by Control-U.

++++++++++++++++++
Included Batteries
++++++++++++++++++

This section provides an overview on what's in this archive, and why.

We'll start from the bottom up, because that's how you build a Forth
system. Ours is no different. You might want to skim over the stuff you
think you don't need, but chances are that you'll need it anyway. 😛

Actually, we skip the bottom two rungs of the ladder.

It will not teach you how to write Forth programs in general. An old but
still very good primer is `Starting FORTH
<https://www.forth.com/starting-forth/>`_, by Leo Brodie.

It also won't wax eloquently you about `Mecrisp Stellaris
<https://mecrisp.sourceforge.net/>`_, the Forth variant this
library is (mostly) based on. You can read more there, including how to
tell your favorite STM-32 microcontroller to run it.

Instead, let's talk about what we're building on top of that foundation,
and why.

Vocabularies
++++++++++++

Let's face it, Forth is good at abstracting things but abysmal at
separating different abstractions from each other, because it only has one
flat vocabulary.

Thus we split that up, mostly by using somewhat-mangled code by Manfred
Mahlow (clarification: this project somewhat-mangles Manfred's code) that's
included in the Mecrisp Stellaris distribution.

`Check here </doc/voc.rst>`_ how to use it.

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

Error handling
++++++++++++++

Programs fail. You want code that fails (say, your 1wire bus has a short in it)
not to take out the rest of the system (the 1wire handler loops / crashes
the system and you can't talk to it to figure out what's wrong / doesn't
free the buffer you need to do that).

Thus we include a couple of words that help with `non-local error handling
</doc/errors.rst>`_. Yes, they work when you're multitasking.

Hardware Access
+++++++++++++++

Talking to the bits and bytes of real hardware is, umm, somewhat annoying,
especially when you need to manually convert random register addresses and
bit offsets to Forth header files.

Check out our `support for SVD files </doc/bits.rst>`_ which is designed to
help with this.

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

Talking about flags: our code uses some of them, to control what actually
gets sent. `Check this list </doc/flags.rst` for details.

+++++++++
Stop here
+++++++++

… unless you want to read boring stuff like which `license
</doc/license.rst>`_ this code is released under, what to keep in mind
if/when you decide to `contribute </doc/meta/coding_style.rst>`_ to this
project (including things nobody ever does, like `testing your code
</doc/testing.rst>`_), or this author's opinion about some
`mostly-irrelevant stuff <doc/meta/opinion.rst>`_.

