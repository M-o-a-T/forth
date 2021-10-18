============
Coding style
============

------
Layout
------

Do this::

    : word ( in -- out )
    \ what does it do?
      words that
      begin
        implement whatever ( stack state at this point )
      repeat
        \ comment if anything might not be cleat
        feature you need
      until
    ;

Don't indent the trailing semicolon and definitely don't put it behind some
other word.

Indent is two spaces. If that's not enough to see what's going on, your
word is too long.

Indenting keywords should be at the start of a line, except for one-word
helpers, particularly ``?dup if``. If the test is longer that one word it
should be on a line of its own. Don't append it to some code that actually
does something.

------
Naming
------

Words that take the address of the word after them, when interpreting, end
with a (straight) apostrophe.

Words that do some thing when compiling are in angle brackets. ``'`` and
``[']`` are a good example to follow.

Words that define the word after them end with a colon.

Words that do some internal function should be in normal parentheses.
Don't just start with one, we can spare the single additional byte.

Words should be written to avoid busy-work for the programmer. For instance:

* ``voc: foo`` automatically does ``foo definitions`` behind the scenes, as
  that's *always* what you do anyway.

* ``foo object: bar`` auto-calls ``bar setup`` because, again, that's what
  you *always* do anyway.

We don't use special words to access things; we use specual vocabularies
and normal words. Thus, instead of writing ``some-pin io@`` to read the
state of a GPIO pin, ``some-pin`` switches to a vocabulary where ``@`` is
defined to read a pin state, not a memory cell.

To set or clear bits, we consistently use ``+!`` and ``-!``.

---------------
Code separation
---------------

There's three kinds of words in most systems. OK, five.

* Scaffolding which you need to make the programmer's life easier. Like
  vocabularies or classes (except for the words used by ``setup`` as these
  need to be discoverable at runtime).

* Code that actually does things.

* Code that introspects things, like ``.word`` or ``??``.

* Code that tests things.

* Code that controls which other code to compile, commonly using words like ``#if``
  or ``[if]``.

Each of the first four should be in separate files, preferably in separate
directories.

We will not use words from the last category.

------------
System state
------------

Library include files should end with ``forth only definitions`` (or leave
the system in that state). They should not call ``compiletoram`` or
``compiletoflash``.

The same thing holds for debug include files.

All include files should protect themselves against re-inclusion.

Conditionals / processing directives
====================================

MoaT Forth comes with its own terminal program. It's written in
Python and (almost) doesn't understand Forth syntax.

This is intentional.

Of course, the terminal still needs to have some Forth-specific knowledge,
because it needs to support directives like ``#if``, ``#[if]`` and ``#ok``.
These simply send the rest of the line to the Forth
interpreter, add a ``.``, and check whether the result is a non-zero
number.

That's all. (Well, there's ``#require`` as a convenient if undefined/include
shorthand, but that doesn't really count.)

Directives start at the beginning of a line. ``mf/term.py`` doesn't know
how to find inline ``#else`` or ``[else]`` tokens.

Please see the `opinion file <doc/meta/opinion.rst>`_ for our rationale.

Structure
+++++++++

In the beginning, add a few lines of comments about what the code in this
file does, and why.

Protect the whole file against inclusion if it has already been sent. You
typically do this::

    #if defined my-voc
    #end
    #then

    voc my-voc:
    …

Start and end with ``forth only definitions``.

At the end of the file, add an `SPDX License Identifier <https://spdx.dev/>`_
and do ``#ok depth 0=`` to protect against spurious nonsense left on the
stack. This line also serves as our standard end-of-file marker.


---------------
Code management
---------------

This project is using git. That is, a ("the", these days) distributed
version control system.

It's currently on Github because everybody else is, there are
mostly-adequate tools for handling issues and change requests, there's a
discussion forum that's close to the code, and all that.

That being said, if contributors object to Github on philosopical or even
practical grounds, it's certainly possible to mirror or move the code
someplace else.

Commits
=======

Commits should be self-contained and contain no unrelated changes, if at all
possible.

Sometimes you fix bug C D and E while chasing bug B in pursuit of feature
A. Please do your very best to collect these changes into at five commits,
not four, and certainly not one. Non-material changes (a typo here, an
improved comment there, and a fixed indent over yonder) may be collected
into a single commit, but don't mash them with functional or documentation
changes.

Updates to working code, its documentation, and the tests for that code
may be in the same commit, but they're not required to be.

There's no requirement for every commit to work. However, we'd like to
require every *tagged* version to work. Thus, please run at least ``make
test`` before updating the main version tag or submitting a merge.

We don't squash-merge or otherwise mutilate our project history. Don't
rebase public code.


Comments
========

The basic commenting style this project strives to achieve is mentioned
above.

*Do not* write words without adding a comment WRT what it does. This
applies to both its stack effects and its function.

*Do not* write elaborate comments on how to use a word, or a collection of
words. That stuff belongs in the accompanying documentation.

*Do not* comment out non-working code. Exception: if there's another way to
do something that's intuitive (as in "every experienced Forth progrmmer who
reads this immediately asks whether you don't do X") but X turned out to be
wrong, write a comment with X in it and explain.

*Do not* add issue numbers and other random stuff to the code. Finding the
reason for the existence of any single line of code is easy, that's what
version control and ``git blame`` are for.

-----------
Documenting
-----------

Yes, every user-visible word should be documented. No, not in the code – in
a file that resides in the ``doc`` folder.

-------
Testing
-------

Yes, we want testing. Lots of it.

Please don't just try your new code on the Forth command line. Add a file
to the ``test`` subdirectory that does the testing, judiciously use ``#ok``
tests, trigger a couple of errors and ``catch`` the result to verify that
the system is left in a reasonable state, and all that.

Your code must be able to run both from RAM and from Flash. Use ``:init``
markers (in general code) and ``setup`` words (in objects) to set up your
data; don't override ``init``.

Don't add startup code that crashes the system, or calls ``abort`` or
``quit``, just because some peripheral isn't present. Setup code beyond
basic hardware or variable initialization should be delegated to a task.

Bonus points for testing some interface or device: Fake it! We have
multitasking and queues and all that, so why not just declare an area of
RAM to be some hardware thing in disguise and let another task mimic its
responses?


