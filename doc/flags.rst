=================
Flags and options
=================

Core flags
==========

This code base uses several "standard" flags to control what gets sent to
the device in question, or to the emulated Mecrisp under test.

real=TYPE
+++++++++

This code is running on "real" hardware, as opposed to an emulator.

You might want to use this value to construct a filename for including
hardware definitions.

hardware=VARIANT
++++++++++++++++

The real hardware is of this type.

You might want to use this value to construct a filename for including
hardware definitions.

machid
++++++

Set this flag to a 32-bit number on Linux-emulated systems.

This value defaults to a random number if not set.

Don#t use oon "real" hardware, as that has a built-in random ID.

See ``lib/mach-id.fs`` for details.

debug
+++++

Include various files for introspection, including disassembly.

Using this flag on real hardware is not recommended, as the Flash image may
easily get larger than 64k.

debug-boot
++++++++++

Make the startup INIT more verbose.

Use this only if your startup code goes splat.

ram
+++

Compile test code into RAM. Typically used with the ``forget`` flag to flush
test code after running it.

Using this flag on real hardware tests without ``forget`` is unlikely to
succeed, as RAM is limited.

forget
++++++

Include a call to ``forgetram`` between tests. Using this flag without
``ram`` should still make a difference, as ``forgetram`` will also
re-initialize all objects.

multi
+++++

Include the multi-tasking code base.

TODO:
The system will switch to multitasking if a UART IRQ handler is defined.
Going back to single-task mode without restoring the old I/O hooks
is unlikely to work.

DONE:
Without real hardware, multitasking will use ``epoll`` as a UART
replacement. The same caveat applies.
