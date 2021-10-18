=====================
Forth subsystem tests
=====================

Forth is not an easy language to code to. If something goes wrong you're
likely to realize much later.

For this reason the Forth code in MoaT shall be tested reasonably rigorously.

Test script
===========

Automated testing is done by ``scripts/test_fs``. It calls the test suite
in ``test/main.fs`` with every possible combination of these flags:

* ram

  The code exercising the modules shall live in RAM. 

* forget

  Exercising code will be dropped after most tests.

* debug

  Extra debugging code will be included.

* multi

  Multitasking code will be included.

Test environment
================

This system runs tests in these environments:

Linux
+++++

We need the ``qemu-user-static`` package.

The test program is simply ``scripts/test``.

Blue Pill
+++++++++

You need a generic UART which connects to a Blue Pill. It *must* connect
RTS to its RESET pin. You also want to connect DTR to BOOT0 for easier
flashing. Pins PA2 and PA3 must be bridged, as one of the tests uses UART 2
for loopback.

The test program is ``scripts/test_pill``; it requires the UART to talk to
(e.g. ``/dev/ttyUSB0``).

Don't run this script too often; it erases your flash a couple of times.
