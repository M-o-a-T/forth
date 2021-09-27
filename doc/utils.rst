====================
Various utility code
====================

The file ``lib/utils.fs`` contains various words to make life easier.

++++++++
Generics
++++++++

eval ( X* str len-- Y* )
========================

Given a word, find it (in the current search order), then run it.

voc-eval  ( X* str len -- Y* )
==============================

Given a word, find it in the current vocabulary, then run it.

The current vocabulary, to put it simply, is the one associated with the
last-used vocabulary-switching word. Thus::

    class: foo
    : bar ." bar" cr ;
    forth defs
    foo object: x

    forth defs only
    x .. token bar voc-eval
    >> bar

This is particularly useful in SETUP words because you can use this to
easily find constants (possibly declared in a subclass) that can be used to
initialize generic data.

voc-lfa  ( str len -- )
=======================

Usse this if you need the LFA of a word, instead of immediately executing
it.

haligned ( c-addr -- h-addr )
=============================

Fix alignment to store half-words.

post-def
========

This hook, if set, is executed (and cleared) after a word definition has
completed.

You can use this hook to define complex word-generating behaviors. See
``sys/multitask.fs`` for a non-trivial example.

roll, -roll
===========

These mostly-standard words are declared if your Forth core doesn't supply
them and the ``roll`` flag is used.

offset:
=======

See `VOC documentation <doc/voc.rst>`_.

[with] ( X* str len "name" -- Y* )
==================================

Suppose you have a token, stored as a string+length tuple. You now want to
pass this token to some word that reads one from the input stream.

This involves temporarily replacing Forth's input source with the string in
question, calling that word, then return to the original input, all without
disturbing the stack.

``[with]`` does this.

This is a compile-only word because in interpreter mode you can simply
write ``NAME foo`` instead of ``s" foo" [with] NAME``.

(with) ( X* str len xt -- Y* )
==============================

This is the runtime part of ``[with]``.

+++++++++++++++++++++
System initialization
+++++++++++++++++++++

The standard way, i.e. overriding INIT with a new word that calls the old
one and then does whatever setup you need, is error-prone as soon as you
add vocabularies because while *your* INIT will work if you mistakenly
declare it inside your own vocabulary, the next piece that also overrides
it won't find it.

Also, objects stored in Flash must be re-initialized after a reset, but
writing a new INIT override for every object out there gets ugly quickly.

Thus we propose a new system that's less error-prone and more composable.

init: ( "name" )
================

``name`` is a word that should be called to set something up. Call this
word. Also, declare a word (named ``%init``) which calls your word when the
system starts up.

That's it. Everything else happens behind the scenes:

* we register a "final" override to INIT (well, except for your main
  program, if you have one) which calls every word named ``%init``, in
  order, when the system reboots.

* interspersed with that: for all named objects we call its ``setup``
  word.

The result is that after a reset, your setup and init words run in the
correct order, i.e. the same order in which you initially declared them.

Usually you don't call the words you pass to ``init:``, or the ``setup``
words of your object, directly. The system does that for you. Of course
there may be occasions where that is useful, most notably with error
recovery.

