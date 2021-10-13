Terminal
========

MoaT comes with a terminal-ish front-end that's been lightly adapted to
talking to a Forth system. It understands a number of processing statements
which minimize the amount of code you need to send to the client.

The terminal can open a serial port, or talk to a program via its stdin/stdout.

There is no networking option. Use ``moat-term -c socat …`` if you need it.

This terminal strips backslashed comments, leading or trailing whitespace,
and empty lines as it sends things.

.. warning

    Forth words are not recognized: strings that contain a lone backslash
    will not result in a valid program. The same holds for ``postpone \\``
    and similar code.

    It's still possible to do the latter: a ``\\`` at the end of the line
    will not be filtered if it's preceded by exactly one space.


Special Features
~~~~~~~~~~~~~~~~

The terminal has its own input line. It will only send complete lines to
Forth. It never sends a backspace.

The input line has command history. Use the `Up` and `Down` keys to page
through it. TODO: save the history between invocations.

You can insert text into the input line. If the text contains a newline,
it is transmitted line by line and the current contents are ignored.

The output window uses em-spaces (Unicode 0x2003) between the
input and the output, should the latter be in the same line.
When you do a multi-line insert, the Forth output is skipped. Thus you can
select a few lines of Forth in the output window, and directly paste them
back in.

.. note::
    An em-space is supposed to be about as wide as the letter `M`.
    That is not very interesting given a monospace font, but you can easily
    see the difference in a normal text editor.

The terminal uses helpful colorful characters to show what the Forth
interpreter thought of your input. Thus you get a nice green checkmark
instead of the ``Ok.`` prompt, and an ❌ if there was an error. Assuming
that you use our modified Mecrisp, that is. Standard Forth doesn't print
anything when there's an error, so you get a timeout instead, which we show
using a 🕠.

You can send a file from the terminal by using the standard ``Control-O``
shortcut, or via the menu button in the top left corner.

The terminal uses Unicode / UTF-8.

There is no cursor in the terminal window. It's distracting and pointless;
your cursor is in the text entry box below it.

Command line options
~~~~~~~~~~~~~~~~~~~~

Besides the obvious options (use ``--help`` to discover them), the terminal
programs come with a few Forth-related improvements.

Reset
+++++

The ``-r`` and ``-R`` options control whether the ``RTS`` line should be
asserted or not, and whether to briefly invert it before proceeding.

Boldface
++++++++

The ``-B`` option shows all output from Forth in bold typeface. This may or
may not be annoying, so the default is not to do this.

Go Ahead
++++++++

Forth sends "ok" (or some variation thereof) after each valid line of
input. The terminal waits for this sequence. You can modify it with the
``-g TEXT`` flag.

This mode cannot recognize invalid lines; the workaround is to assume such
after some timeout.

Ack/Nack mode
+++++++++++++

If you have a suitably modified Forth (such as our Mecrisp variant),
you can trigger on control characters that signal successful execution / an
error. The default is Control-C for errors and Control-D for success. This
is somewhat non-optimal, but we'd like to reserve the "real" ACK and NACK
codes for running a "real" protocol on the serial line.

To turn off the default, use ``-a -1 -A -1``.

If you use both go-ahead and ack options, the terminal will wait for it
the first time it encounters the go-ahead prompt.

Ack/Nack mode can use C1 control characters (0x80…0x9F). While these
characters do show up as legitimate parts of UTF-8 sequences, we can
recognize that situation.

Batch mode
++++++++++

If you use ``-b`` then no interactive session will be created and no window
will be opened. Instead, the session will end after sending any file(s)
to the client.

Sending data
++++++++++++

You can use ``-x PATH`` to send the contents of a (text, ASCII / UTF-8) file.

This option can be used multiple times.

Development mode
++++++++++++++++

This mode is activated with the ``-D`` option and tells the terminal 
to stay in the terminal after sending data. This is obviously the default
when no file is given on the command line.

Batch and development mode are mutually exclusive.

Logging
+++++++

Use ``-l PATH`` to send the terminal output to this file.

This option is required in batch mode if you want to capture the terminal
output. For display to your terminal, use ``-l /dev/stdout``.

Test flags and values
+++++++++++++++++++++

You can use ``-F NAME`` to set this flag. The Forth text can then test for
this flag's presence (or absence) and proceed accordingly.

Uses include seeding a gateway node with different parameters, or changing
various tests while keeping the source code unified.

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
++++++++++++

#if WORD…
---------

Your basic conditional statement.

The words are evaluated. The result should be a single value on the stack.

The statement is true if that value is not zero, as in Forth.

You may depend on existing stack contents, but at the end your code must
have increased ``DEPTH`` by exactly one. We might check that.

``WORD…`` must not emit anything and may not cause an error.

#if-ok WORD…
------------

This test checks whether the WORDs result in a Forth OK prompt.

.. warning

    Unlike on some other Forth terminals, your system's ``QUIT`` word
    should **not** send an OK back.

.. note

    Tests using ``if-ok`` are notoriously unreliable because the test may
    fail for other reasons than you expected. It's generally better to
    catch specific failures, using ``CATCH`` and ``THROW``.

    See the end of ``test/ring.fs`` for an example.

#if-flag NAME…
--------------

This statements is true if every named flag is set, or cleared when prefixed with
a ``!``.

If NAME has the form WORD=VALUE, the check applies to whether the flag's
value is equal to VALUE (or not).

#[if] WORD…
-----------

Like ``#if``, but will be wrapped in ``[`` and ``]``, thus works in compile
context (and only there).

It's bad form to use this across definitions.

#if-ram WORD…
-------------

Sometimes you might need to make decisions on code that's in RAM. This
statement changes to ``compiletoram``, does the check, then switches back
to whatever the state was before.

There is no ``[]`` version of this: switching to RAM and back, in the
middle of compiling something, might be detrimental to your health.

#else
-----

If you don't know what ``#else`` does, this document won't help.

#endif
------

Whatever the last preceding ``#if``-like statement did: we continue here.

then
----

OK, OK, this is Forth, so here's your favorite synonym for ``#endif``. 😎


Other processor statements
++++++++++++++++++++++++++

#include PATH
-------------

The contents of the file at ``PATH`` are processed.

Execution resumes after completion.

#require WORD PATH
------------------

The existence of WORD is checked using ``token WORD find drop 0=``. 
If it is not found, PATH is interpreted as a file name and included.

if PATH is missing, ``lib/WORD.fs`` is used. If it ends with a slash,
``PATH/WORD.fs`` is substituted.

#end
----

Processing this file is terminated. This is not an error even if ``#end``
is inside a conditional. The terminal resumes at the point where the
current file was included.

#end*
-----

Processing of all files is terminated. This is not an error; the terminal
immediately returns to the prompt / exits.

This is useful for debugging.

There is no way to resume uploading. (Yet.)

#set-flag FLAG DATA
-------------------

The flag FLAG is set to DATA.

If the data is ``-``, the flag is deleted.

#read-flag FLAG CODE
--------------------

CODE is sent to Forth.

The flag FLAG is set to whatever output it generates.

#error TEXT
-----------

Processing this file is terminated with the message ``TEXT``.

A non-interactive terminal exits with an error condition if this statement
is encountered.

#echo [TEXT]
------------

Show this text on the terminal, without sending it to Forth.

This is useful if you need to show statements which the user needs for
manual debugging, or just to annotate your log.

#ok WORD…
---------

The words are evaluated (as in ``#if``). The statement is OK if its value
is not zero; otherwise an error is raised (as in ``#error``).

.. note

    To reverse the test, just add ``not`` to the end. ``#-ok`` tests for a
    statement that breaks the interpreter.

#-ok WORD…
----------

``WORD…`` is evaluated and *must not* result in a Forth "ok" prompt.

If your statement may or may not fail, you really should fix the situation
to be more deterministic. In a pinch, use this workaround::

    #if-ok WORD…
    #then

#delay TIME
-----------

Change the maximum delay between sending a line and getting an ``ok``
back form Forth.

``#-ok`` will always wait this long. So will ``#if-ok`` if it doesn't get a
"good" reply.

#send NAME
----------

Send the text associated with the flag ``NAME``.

If there is no text attached to the flag, ``-1`` will be sent; if it doesn't exist at
all, ``0`` (zero).

The text is sent as a line of its own.

If the text contains curly parentheses, the word inside them is interpreted
as a flag and inserted into it.


Coding hints
~~~~~~~~~~~~

For checking whether a word exists, "defined" and "undefined" words exist.
If you want to introspect a vocabulary, you can use ``#if VOC defined NAME``
which is not quite intuitive, but it works.

For open-coding you can use ``#if token NAME find drop``.

.. note:

    The ``drop`` is there because ``find`` returns two words and ``#if`` only eats
    one of them.
