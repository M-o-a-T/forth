=================
Flags and options
=================

Core flags
==========

This code base uses several "standard" flags to control what gets sent to
the device in question, or to the emulated Mecrisp under test.

arch=ARCH
+++++++++

CPU (sub)architecture.

Architecture-specific register files can be included from ``./svd/fs/core/{arch}/``.

real=VENDOR
+++++++++++

This code is running on "real" hardware, as opposed to an emulator.

mcu=VARIANT
+++++++++++

The real hardware is of this type.

MCU-specific register files can be included from ``./svd/fs/soc/{real}/{mcu}/``.

clock=HZ
++++++++

The frequency of your decice's system clock crystal. Used for baud rate and
systick calculation.

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

debug-disasm
++++++++++++

Include the disassembly words (``see``, ``seec``) in your Flash.

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

yieldstack
++++++++++

This mod for multitasking adds a separate stack to use when the task
switcher needs to change a task's state.

This may be beneficial on systems with a large number of tasks (provided
they also use a separate interrupt stack) because that way you need to
reserve fewer cells on your tasks' stacks for the task switcher.
