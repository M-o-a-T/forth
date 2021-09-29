==============
Error handling
==============

Forth is good at controlling peripherals. Peripherals are good at
misbehaving. We want to prevent misbehaviour from crashing the whole
system.

We do not want to teach every word which could possibly fail to return an
error code, or to check errors after every call to such words. It's
notoriously difficult to test error paths, and your code size would
explode.

Thus, Forth has pioneered non-local error handling. Our code expands on
that.

Basic principle
===============

The basic building block for Forth's error management is CATCH and THROW.
CATCH calls a word and adds a zero to the stack. However, if that word
calls ``n throw``, execution immediately returns to the CATCH, which
restores the stack to whatever it was when it was called, and then returns
``n``.

Words
=====

catch ( X* xt -- Y* 0 | X* n)
+++++++++++++++++++++++++++++

Record the current state of the stacks, execute ``xt``, and leave a zero on
the stack. The rest of the stack effects are whatever ``xt`` did.

throw ( n -- does-not-return )
++++++++++++++++++++++++++++++

Unwind the stacks to the next-closest ``catch``, which will leave ``n``.

abort ( -- does-not-return )
++++++++++++++++++++++++++++++

``abort`` is basically the same as ``-1 throw``, except that it calls
``quit`` when there's no open ``catch`` instead of crashing your system.

abort" TEXT" ( n -- does-not-return )
+++++++++++++++++++++++++++++++++++++

If ``n`` is zero, do nothing.

Otherwise, save a pointer to the following text, then do ``n abort``.

quit ( -- does-not-return )
+++++++++++++++++++++++++++

This system word returns to the interpreter. However, in a subtask it will 
call ``-56 throw``. (-56 is the Forth 2012 constant for ``quit``.)

.abort ( -- )
+++++++++++++

Print a line with the most recent abort code and/or message. Code and
message are then cleared.

If both are clear, this word does nothing.

If using multitasking: Abort data are not task-specific, thus this word
should be called before ``yield``\ing.

Caveats
=======

``catch`` restores the stack by remembering the parameter and return
stack pointers. The word it calls thus shouldn't remove anything from
that stack, at least not if there's a chance that it could calll ``throw``
before it returns.

Also, it stores CPU context registers on the return stack. You need to
ensure that there's enough stack space left, particularly when you're using
multitasking.

