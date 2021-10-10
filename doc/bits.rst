==========================
Registers, bits, bitfields
==========================

Modern microcontrollers have a lot of modules (GPIO, SPI, I2C, CAN, USART;
the list goes on), each with many registers.

We provide a couple of features to make life easier, or at least code
easier to type.

++++++++++++++++++++++++
Generating register defs
++++++++++++++++++++++++

The supplied program ``scripts/mapgen`` creates Forth code to build
register maps. Its arguments are an SVD file and one or more group or
device names.

Its output will look like this (example: ``mapgen STM32F7x.svd RNG``)::

    \ Random number generator
    voc: _RNG
    \ CR : control register
      &rg voc: _CR
            &bi item \ 3
        $3 constant IE \ Interrupt enable
            &bi item \ 2
        $2 constant RNGEN \ Random number generator enable
            previous definitions
      _CR item
    $0 offset: CR
    
    \ SR : status register
    …
    
    \ DR : data register
      &rg item
    $8 offset: DR
    
    previous definitions
    
    $50060800 _RNG port: RNG
    
(``&rg`` and ``&bi`` are sub-vocabularies of ``bits``, which contain some
words for memory manipulation.)

All of this means that ``RNG CR IE @`` will retrieve one bit. Likewise, you can
set or clear this bit by writing ``0 RNG CR IE !`` or `` RNG CR IE +!``.

Bitfields works similarly. Also, you can prepare to modify multiple bits::

	RNG CR <% IE -% RNGEN +% %>!

This will mostly-atomically set RNGEN and clear IE.

Bitfields work similarly::

	TIM1 CR1 <%  2 CKD *%  ARPE -%  %>!  \\ manipulate TIM1.CR1

``ARPE`` is a single bit, ``CKD`` a bitfield. ``*%`` also works on single
bits.

Options
=======

``mapgen`` currently accepts these options:

-v / --voc NAME
+++++++++++++++

Put the peripherals in vocabulary ``NAME`` instead of ``forth``.
The vocabulary must already exist.

You can use a sub-vocabulary; just quote the space (for the shell).

SVD files
=========

The core ARM register definitions are available from CMSIS-4, in the
directory ``svd/core/Device/ARM/SVD/``. The SoC definitions are collected
in ``svd/soc/data/VENDOR/``. List these directories to discover for which
vendors and MCU families SVDs are available. (The answer *should* be "all of
them".)

Run ``git submodule update --init`` to fetch these directories.

Run ``make prep VENDOR=STMicro`` to prepare all register files for a
specific MCU vendor. This also builds all core-specific register files.
Be aware that the core-specific files are incomplete; `this Github issue
<https://github.com/ARM-software/CMSIS_5/issues/844>`_ tracks one attempt
to get this problem fixed, unfortunately without much success (as of late
2021).

We'd appreciate if you'd weigh in there; any help towards fixing this
problem would be very much appreciated. In the meantime, the ``soc/ARCH``
subdirectory contains a couple of hand-written and somewhat incomplete
workarounds.

The code for the NVIC ("Nested Vectored Interrupt Controller") contains
some specialized words. See `here </doc/arch/nvic.rst>`_ for details.

Words
=====

Registers
+++++++++

Register words push their address.

! ( value reg -- )
------------------

Stores the value in this register.

This is like Forth's normal ``!`` except that it works transparently with
half-word and character registers.

@ ( reg -- value )
------------------

Retrieve the value from this register.

This is like Forth's normal ``!`` except that it works transparently with
half-word and character registers.

? ( reg -- )
------------

Display this register's content. The default is to use a hex number of the
appropriate width, but auto-generated words that split the value into its
constituent bits are available if fields are defined in the SVD file.

Single bits
+++++++++++

Single-bit words push their bit position.

! ( value reg pos -- )
----------------------

Stores the bit value in this register. All other bits are unchanged.

The value is treated as a flag, i.e. not just the bottom bit.

@ ( reg pos -- value )
----------------------

Retrieve the value from this register.

Returns zero or one.

? ( reg pos -- )
----------------

Display this bit.

+% ( bits mask pos -- bits mask )
---------------------------------

Sets bit ``pos`` in both ``bits`` and ``mask``.

-% ( bits mask pos -- bits mask )
---------------------------------

Sets bit ``pos`` in ``mask``.

\*% ( bits mask value pos -- bits mask )
----------------------------------------

Calls ``-%`` or ``+%``, as appropriate, depending on whether ``value`` is
zero.

Bitfields
+++++++++

Bitfields push a composite word describing how large the field is (bits
11:6) and its offset (bits 5:0).

! ( value reg width|off -- )
----------------------------

Stores the bit value in this register. All other bits are unchanged.

The value is treated as a flag, i.e. not just the bottom bit.

@ ( reg width|off -- value )
----------------------------

Retrieve the value from this register.

Returns zero or one.
    
? ( reg width|off -- )
----------------------

Display this bitfield.

\*% ( bits mask value width|off -- bits mask )
----------------------------------------------

Mask and shift the value, then OR it into ``bits`` and the mask into
``mask``.

