=====================
Forth subsystem tests
=====================

Forth is not an easy language to code to. If something goes wrong you're
likely to realize much later.

For this reason the Forth code in MoaT shall be tested reasonably rigorously.

Test script
===========

Automated testing is done by ``scripts/test_fs``. It calls the test suite
in ``fs/test/main.fs`` with every possible combination of these flags:

* ram

  The code exercising the modules shall live in RAM. 

* forget

  Exercising code will be dropped after most tests.

* debug

  Extra debugging code will be included.

* multi

  Multitasking code will be included.

  TODO.
