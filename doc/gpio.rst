====
GPIO
====

or "General Purpose In/Output". Basically, single pins you can control
directly.

The problem is that MCUs have wildly different ideas about what "general
purpose" means. Does a level change trigger an interrupt? Are there pull-up
resistors? Are there pull-down resistors? Can you atomically change multiple
pins' values? Just on/off, or both? Can you atomically change the direction
of multiple pins in one step (inut vs. output)? Can you atomically switch
from a pull-up input to an open-drain output? Or from pull-down to
open-source? How do you control whether the pin is controlled by your code
or one of the MCU's built-in peripherals (UART, I²C, …)? The list goes on.

For this reason there's no really-generic GPIO library in MoaT Forth.

++++++++++
STM 32F1xx
++++++++++

The ``GPIO`` vocabulary is located in ``soc/armcm3/STM32F103xx/io.fs``.

Words
=====

pin: ( port pin "name" -- )
+++++++++++++++++++++++++++

Declare a word that defines a single GPIO pin.

Thus::

    gpio also
    1 3 pin: PB3
    PB3 +!
    PB3 mode PP !

sets pin PB3 to HIGH.


base: ( port "name" -- )
++++++++++++++++++++++++

Declare a word that defines a set of GPIO registers.

Thus::

    gpio also
    1 base: PB
    PB BSRR

leaves the address of port B's "Port bit set/reset register" on the stack,
which allows you to set *and* clear multiple bits in one instruction.

I/O
===

These words affect single pins; they are interrupt safe (except for ``^!``).

@ ( pin -- flag )
+++++++++++++++++

Read the input bit. Returns 0 or -1.

+! ( pin -- )
+++++++++++++

Set the output bit / switch to pull-up.

-! ( pin -- )
+++++++++++++

Clear the output bit / switch to pull-down.

^! ( pin -- )
+++++++++++++

Invert the output bit. This is not interrupt safe.

! ( flag pin -- )
+++++++++++++++++

Sets or clears the output depending on the flag.

Modes
=====

You declare the pin mode with ``‹pin› mode ‹mode› !``.

The following pin modes are defined:

Input
+++++

ADC
---

Analog mode.

FLOAT
-----

Floating input. This is the reset state.

PULL
----

Input with pull-up/down. The direction is controlled by the output data
register.

Output
++++++

PP
--

Push/Pull.

OD
--

Open Drain

Output Modifiers
++++++++++++++++

Do not use both ``slow`` and ``fast``.

SLOW
----

Max output speed is 2 MHz, not 10.

FAST
----

Max output speed is 50 MHz, not 10.

AF
--

Alternate Function.

Registers
=========


