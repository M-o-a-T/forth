Terminal
========

MoaT comes with a terminal-ish front-end that's been lightly adapted to
talking to a Forth system. It understands a number of processing statements
which minimize the amount of code you need to send to the client.

The terminal comes in two versions: ``moat-term`` opens a serial port,
while ``moat-cmd`` talks to a Forth program via its stdin/stdout.

There is no networking option. Use ``moat-term socat …`` if you need it.

This terminal strips backslashed comments, leading or trailing whitespace,
and empty lines.

.. warning

    Forth words are not recognized: strings that contain a lone backslash
    will not result in a valid program. The same holds for ``postpone \\``
    and similar code.

    It's still possible to do the latter: a ``\\`` at the end of the line
    will not be filtered if it's preceded by exactly one space.

Command line options
~~~~~~~~~~~~~~~~~~~~

Besides the obvious options (use ``--help`` to discover them), the terminal
programs come with a few Forth-related improvements.

Reset
+++++

The ``-r`` and ``-R`` options control whether the ``RTS`` line should be
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

Test flags and values
+++++++++++++++++++++

You can use ``-F NAME`` to set this flag. The Forth text can then test for
this flag's presence (or absence) and proceed accordingly.

Uses include seeding a gateway node with different parameters, or changing
various tests while keeping the source code unified.

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

``#if``-like statements may be nested arbitrarily deep. Leaving a dangling
``#if`` open at the end of your file results in an error.

Conditionals
::::::::::::

#if WORD…
+++++++++

Your basic conditional statement.

The words are evaluated. The result should be a single value on the stack.

The statement is true if that value is not zero, as in Forth.

You may depend on existing stack contents, but at the end your code must
have increased ``DEPTH`` by exactly one. We might check that.

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

    See the end of ``fs/test/ring.fs`` for an example.

#if-flag NAME…
++++++++++++++

This statements is true if every named flag is set, or cleared when prefixed with
a ``!``.

#[if] WORD…
+++++++++++

Like ``#if``, but will be wrapped in ``[`` and ``]``, thus works in compile
context (and only there).

It's bad form to use this across definitions.

#if-ram WORD…
+++++++++++++

Sometimes you might need to make decisions on code that's in RAM. This
statement changes to ``compiletoram``, does the check, then switches back
to whatever the state was before.

There is no ``[]`` version of this: switching to RAM and back, in the
middle of compiling something, might be detrimental to your health.

#else
+++++

If you don't know what ``#else`` does, this document won't help.

#endif
++++++

Whatever the last preceding ``#if``-like statement did: we continue here.

then
++++

OK, OK, this is Forth, so here's your favorite synonym for ``#endif``. 😎


Other processor statements
::::::::::::::::::::::::::

#include PATH
+++++++++++++

The contents of the file at ``PATH`` are processed.

Execution resumes after completion.

#end
++++

Processing this file is terminated. This is not an error; the terminal
resumes at the point where it was included / returns to the interactive
prompt.

#end
++++

Processing of all files is terminated. This is not an error; the terminal
immediately returns to the prompt / exits.

This is useful for debugging.

There is no way to resume uploading. (Yet?)

#error TEXT
+++++++++++

Processing this file is terminated with the message ``TEXT``.

A non-interactive terminal exits with an error condition if this statement
is encountered.

#echo [TEXT]
++++++++++++

Show this text on the terminal, without sending it to Forth.

This is useful if you need to show statements which the user needs for
manual debugging, or just to annotate your log.

#ok WORD…
+++++++++

The words are evaluated (as in ``#if``). The statement is OK if its value
is not zero; otherwise an error is raised (as in ``#error``).

.. note

    To reverse the test, just add ``not`` to the end. ``#-ok`` tests for a
    statement that breaks the interpreter.

#-ok WORD…
++++++++++

``WORD…`` is evaluated and *must not* result in a Forth "ok" prompt.

If your statement may or may not fail, you really should fix the situation
to be more deterministic. In a pinch, use this workaround::

    #if-ok WORD…
    #then

#delay TIME
+++++++++++

Change the maximum delay between sending a line and getting an ``ok``
back form Forth.

``#-ok`` will always wait this long. So will ``#if-ok`` if it doesn't get a
"good" reply.

#send NAME
++++++++++

Send the text associated with the flag ``NAME``.

If there is no text attached to the flag, ``-1`` will be sent; if it doesn't exist at
all, ``0`` (zero).

The text is sent as a line of its own.

This feature was added to send serial numbers or other parameters to your
client processors, just by passing them on the command line (or in your
Python code). The inablility to do more, e.g. interpolate flag values into
the Forth source, is deliberate: this terminal is not intended to be a macro
processor that rivals the one in C. If you need a more elaborate mechanism
to generate variations in your code, Python offers several templating
systems.

Coding hints
~~~~~~~~~~~~

For checking whether a word exists, "defined" and "undefined" words exist.
If you want to introspect a vocabulary, you can use ``#if VOC defined NAME``
which is not quite intuitive, but it works.

For open-coding you can use ``#if token NAME find drop`` (remember that
FIND returns two words), but that doesn't work for vocabularies: neither
FIND nor EXECUTE are in the root vocabulary, and copying them there would
be a bad idea.

