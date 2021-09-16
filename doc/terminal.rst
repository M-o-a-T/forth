Terminal
========

MoaT comes with a terminal-ish front-end that's been lightly adapted to
talking to a Forth system. It understands a number of processing statements
which minimize the amount of code you need to send to the client.

The terminal comes in two versions: ``moat-term`` opens a serial port,
while ``moat-cmd`` talks to a Forth program via its stdin/stdout.

There is no networking option. Use ``moat-term socat …`` if you need it.

Command line options
~~~~~~~~~~~~~~~~~~~~

Besides the obvious options (use ``--help`` to discover them), the terminal
programs come with a few Forth-related improvements.

Reset
+++++

The ``-r`` and ``-R`` options control whether the RTS line should be
asserted or not, and whether to briefly invert it before proceeding.

Go Ahead
++++++++

Forth sends "ok" (or some variation thereof) after each valid line of
input. The terminal waits for this sequence. You can modify it with the
``-g TEXT`` flag.

Batch mode
++++++++++

If you use ``-b`` then no interactive session will be created. Instead the
session will end, possibly after sending a file to the client.

Sending data
++++++++++++

You can use ``-x PATH`` to send the contents of a (text, ASCII / UTF-8) file.

(OK, so batch mode is pretty useless without this option …)

Development mode
++++++++++++++++

This mode is activared with the ``-D`` option and tells the terminal 
to return to the terminal instead of exiting with an error if/when sending
data fails (defined as "the terminal sent a line and Forth didnt reply with
OK").

Batch and development mode are mutually exclusive.

Logging
+++++++

Use ``-l PATH`` to send the terminal output to this file.

This option is required in batch mode if you want to capture the terminal
output. For display on your terminal, use ``-l /dev/stdout``.

Terminal control
~~~~~~~~~~~~~~~~

Use ``Ctrl+]`` (located on ``Ctrl+5`` on some European keyboards), or
``Ctrl+T`` followed by ``Q``, to exit the terminal.

Use ``Ctrl+T Ctrl+U``, then type the file name, to send a text file.

Use ``Ctrl+T Ctrl+H`` to display help, and for other actions.

Processing statements
~~~~~~~~~~~~~~~~~~~~~

This terminal understands a number of processing commands. They all start
with a ``#`` character.

The initial ``#`` is not magic: everything else that starts with it is
passed through unmodified.

These commands are not magic. Everything that doesn't start with some of
these magic words is passed though as-is.

These commands are only processed when sending text files. Typing them to
the Forth interpreter will send them unmodified, most likely resulting in a
``not found.`` error message.

#if WORD…
+++++++++

The words are evaluated. The sequence is expected to leave a single value on the
stack.

The statement is true if the value thus printed is not zero.

It's bad form to depend on existing stack contents.

``#if`` statements may be nested arbitrarily deep, but not across files.

``WORD…`` must not emit anything and may not cause an error.

#if-ok WORD…
++++++++++++

This test checks whether the WORDs result in a Forth OK prompt.

.. warning

    Unlike on some other Forth terminals, your system's ``QUIT`` word
    should **not** send an OK back.

.. note

    Tests using ``if-ok`` are notoriously unreliable because the test may
    fail for other reasons than you expected. It's generally better to
    catch specific failures, using ``CATCH`` and ``THROW``.

#ifdef NAME
+++++++++++

A shortcut for ``#if token NAME find drop``.

#ifndef NAME
++++++++++++

A shortcut for ``#if token NAME find drop 0=``.

#[if] WORD…
+++++++++++

Like ``#if``, but will be wrapped in ``[`` and ``]``, thus works in compile
context (and only there).

It's bad form to use this across definitions.

There are no ``#[ifdef]`` or ``#[ifndef]`` versions; use the long form if
you really need them.

#if-ram WORD…
+++++++++++++

Sometimes you might need to make decisions on code that's in RAM. This
statement changes to ``compiletoram``, does the check, then switches back
to whatever the state was before.

There is no ``[]`` version of this: switching to RAM and back, in the
middle of compiling something, might be detrimental to your health.

#else
+++++

If the previous ``#if`` failed, execution resumes after this statement,
otherwise it is suspended.

#endif
++++++

Resume execution skipped by the last previous unclosed ``#if`` or
``#else``.

.. note

    All processing statements below this point are ignored.

then
++++

OK, OK, this is Forth, so here's your favorite synonym for ``#endif``. 😎


#include PATH
+++++++++++++

The contents of the file at ``PATH`` are processed.

Execution resumes after completion.

#end
++++

Processing this file is terminated. This is not an error; the terminal
resumes at the point where it was included / returns to the interactive
prompt.

#error TEXT
+++++++++++

Processing this file is terminated with the message ``TEXT``.

A non-interactive terminal exits with an error condition if this statement
is encountered.

#echo [TEXT]
++++++++++++

Emits this text without sending it to Forth.

This is useful if you need to comment statements which should wait for
manual debugging.

#-ok WORD…
++++++++++

``WORD…`` is evaluated and *must not* result in a Forth "ok" prompt.

If your statement may or may not fail, you really should fix the situation
to be more deterministic. In a pinch, use this workaround::

    #ifok WORD…
    #endif

#check WORD…
++++++++++++

The words are evaluated (as in ``#if``). The statement is OK if its value
is not zero; otherwise an error is raised (as in ``#error``).

#delay TIME
+++++++++++

Change the allowable delay between sending something and getting an ``ok``
back form Forth.

``#-ok`` will always wait this long. So will ``#if-ok`` if it doesn't get a
"good" reply.
