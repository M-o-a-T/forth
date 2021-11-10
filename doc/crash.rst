==============
Crash handling
==============

Let's face it: Forth is a pretty unforgiving system. You can be pretty sure
that not balancing your stack, esp. the return stack, will result in a
nasty crash or cause your code to run an (unintentional …) endless loop.

Well, we can fix that. Simply include ``debug/crash2.fs``.

On "real" hardware, this installs an interrupt handler to system interrupt
3 (the hard fault handler). Whenever the system crashes (or an NMI is
triggered), you get a parameter stack dump and a return stack call tree.

On Linux, the interrupt handler is hooked to the ``SIGSEGV``, ``SIGBUS``
and ``SIGILL`` interrupts instead.

How to trigger an NMI, to get out of an endless loop, depends on the SoC
you're using. On Linux, use ``kill -BUS ‹pid›``. TODO: The terminal program
has a button to do this.

+++++++++
Debugging
+++++++++

If you use multitasking, tasks get auto-suspended while some other task is
busy emitting characters. Otherwise the outputs would be interleaved and
you'd have fun decoding the resulting mess.

That's no good if a crash disables the tasking system, thus ``task
!single`` suspends background multitasking and restores direct output. It's
also no good if your code crashes immediately after printing something, as
the output is buffered. To circumvent this, ``term drain`` busy-waits until
all output has been sent.

Do not print anything from an interrupt word while multitasking is enabled.

You can use a hardware debugger. That's out of scope for this
documentation, though.

