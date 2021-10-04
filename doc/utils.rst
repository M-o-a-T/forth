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

obj-lfa>?cwid  ( lfa -- cwid|0 )
================================

Given an LFA of an object, return its context WID (i.e. its class).

Any other LFA, or a naked buffer, yields zero.

Split off because both system initialization and object debugging need it.

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
