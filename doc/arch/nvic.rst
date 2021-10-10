====
NVIC
====

The "Nested Vectored Interrupt Controller" is somewhat special because some
of its registers really don't map well to the code we otherwise
auto-generate from register descriptions (when we have them, which isn't
the case here anyway).

Thus the ``nvic.fs`` file contains some specialized words, which are
described here.

The SysTick block, which is notionally aprt of the NVIC, is not described
here because we provide high-level words for using it. See `here
</doc/timer.rst>`_ for details.

SoC Interrupts
==============

The following words access the ISER, ICER, ISPR, ICPR and ABR.

thei affects only the SOC-specific interrupts with numbers
startign at zero. The core or "system handler" interrupts, numbered -14 to
-1, are handled elsewhere.

active ? ( n -- flag)
+++++++++++++++++++++

Check whether this interrupt is active, i.e. either currently running or
pending and stacked.

enabled ? ( n -- flag )
+++++++++++++++++++++++

Check whether this interrupt is enabled, i.e. can possibly be triggered.

enabled +! ( n -- )
+++++++++++++++++++++++

Enable this interrupt.

enabled -! ( n -- )
+++++++++++++++++++++++

Disable this interrupt.

pending ? ( n -- flag )
+++++++++++++++++++++++

Check whether this interrupt is enabled, i.e. can possibly be triggered.

pending +! ( n -- )
+++++++++++++++++++

Pend this interrupt, i.e. cause it to fire.

The interrupt must be enabled for this to have any effect. This is a no-op
if the interrupt is already pending.

pending -! ( n -- )
+++++++++++++++++++

Clear the interrupt's "pending" state, i.e. no longer run the handler at
the next opportunity.

prio @ ( n -- prio )
++++++++++++++++++++

Read this interrupt's priority.

prio ! ( prio n -- )
++++++++++++++++++++

Set this interrupt's priority.


Core Interrupts
===============

These interrupts are handled via the SHCSR and SHPR ("System Handler Priority Registers").

The registers in this set are coded like any other hardware register, thus
``nvic shcsr ?`` lists all relevant bits, provided that you have loaded
``debug/bits.fs``.

Interrupt numbers
+++++++++++++++++

Core interrupts have negative numbers. They are provided in the ``nvic
irq`` vocabulary, as ``NMI``, ``HardFault``, ``MemMgr``, ``BusFault``,
``UsageFault``, ``SecureFault``, ``SVCall``, ``DebugMon``, ``PendSV``
and ``SysTick`` constants.

.. note::

    Yes, typing ``irq systick`` isn't quite as concise as ``-1``, but it
    documents what that -1 *means* – without an additional comment off to
    the side. The resulting code size is the same.

We don't describe the details here; check the Cortex-M3 Technical Reference
Manual if you need them.

Registers
+++++++++

This block contains other registers, which we didn't add words for because
we're very unlikely to ever need them. Feel free to submit a patch if your
usecase requires access to any of them.

aiarc
-----

App Interrupt And Reset Control.

This register contains the PRIGROUP field, which describes whether
interrupts can pre-empt each other. We set this field to 7, meaning that
they cannot, because interrupts need stack space and in a multi-tasked
system we need to not require too much of that.

shcsr
-----

System Handler Control and State Register.

icsr
----

Interrupt Control State Register. 

shpr
----

System Handler Priority Registers. Usage: like ``prio``, above.
You cannot change the priorities of ``NMI`` and ``HardFault``.

