============
Vocabularies
============

Traditional Forth uses a linear list of words. This list can become quite
long and demands some creative naming in larger projects. Creative names,
however, increase the mental load on the programmer, i.e. cause bugs.

Using vocabularies you can modularize your code.

Also, words may chain into vocabularies. Thus you can define a constant,
e.g. a base register, which chains into a list of offsets that only apply
to this register type.

After loading this extension there are three pre-defined vocabularies:

* FORTH contains all the previously defined words.

* ROOT contains a few words that are essential for switching vocabularies.

* \VOC stores all the boring internals of the vocabulary implementation itself.

.. note:

    Mecrips is case insensitive (in ASCII); so is this extension,
    as it uses Mecrisp's COMPARE.

-----------
Basic Usage
-----------

voc: ( "name" -- )
++++++++++++++++++

Create a stand-alone vocabulary prefix; it extends the root vocabulary.

‹voc› voc: ( "name" -- )
++++++++++++++++++++++++

Create a vocabulary prefix that extends, i.e. inherits words from, the given voc.

;voc ( -- )
+++++++++++

Undoes the effect of ``voc:``, i.e. switch back to the parent context.

‹voc› ?? ( -- )
+++++++++++++++

Show a list of all words in the current search context.

This word stays in the current search context. The intent is that, if you
want to call word ``baz`` which is in vocabulary ``bar`` which is itself in
vocabulary ``foo``, but you don't remember its spelling, you can enter ``foo
bar ??`` for lookup, then follow that directly with ``baz`` without having
to re-type the ``foo bar`` part. This is particularly relevant when ``foo``
or ``bar`` are *item* words (see below) instead of vocabularies: they might
have stack effects.

‹voc› ??? ( -- )
+++++++++++++++

Like ``??``, but display one word per line and also show context flags and
assorted addresses.

\.. ( -- )
++++++++++

Switch back from a temporary VOC search to the default Forth search order.

‹voc› definitions ( -- )
++++++++++++++++++++++++

Make *‹voc›* the current compilation context.

‹voc› item ( -- )
+++++++++++++++++

Make the next created word a context switching one: along with any other
effect that calling this word may have, it sets the temporary search context
to *‹voc›* at compile time.

One use of this is for describing register maps::

    voc: gpio
    $00 offset: in ( a1 -- a2 )
    $02 offset: out
    $04 offset: dir

    : port: ( "name" a -- ) item constant ;
    $40004C00 gpio port: p1 ( -- a1 )

We can now write ``p1 out ( -- a2 )``. This avoids the long constant names
and the combinatorial explosion you get when you have five GPIO registers
with five accessors each.

``offset:`` is defined in ``lib/util.fs``.

sticky ( -- )
+++++++++++++

Make the next created word a sticky one.

Normally, looking up a word clears the context switching
associated with the previous word. A sticky word prevents that.

One use is for debugging: ``??`` and ``.s`` are sticky so you can
add them to your code freely (assuming that the word before them doesn't
consume the next token).

__ ( -- )
+++++++++

Look up the next word in the current compilation context.

This is relevant for vocabulary/class inheritance, particularly if you
recycle words that also occur in Forth itself (or another vocabulary that's
also on your search list).

.named ( "name" -- )
++++++++++++++++++++

This word solves the "oh boy in which vocabulary did I put that word"
problem. You're likely to run into this when you come back to your code
base after a few months and remember a word but not where to find it.

``.named`` scans the dictionary and prints every occurrence of the word
``name``, along with its address and the vocabulary it is in.
You can then say ``voc… ??? ..`` to display what else is in there.

-----------
Word search
-----------

Unless you're in a context switch, Forth has a list of vocabularies to look
up all words in.

The words in this section are all immediate and have no stack effect.

<voc>
+++++

Look up the next word in <voc>.

<voc> only
++++++++++

Reset the search order to ``<voc>``, FORTH and ROOT.
If you want Forth only, use ``FORTH ONLY``.

As a special case, ``ROOT ONLY`` causes Forth not to be added.

<voc> also
++++++++++

Add ``<voc>`` to the search list.

<voc> first
+++++++++++

Add ``<voc>`` to the search list.

Currently there is no difference between ALSO and FIRST. FIRST is intended
to replace the top word; you can achieve this effect by ``<voc> IGNORE``.

<voc> ignore
++++++++++++

Remove ``<voc>`` from the search list.

Removing FORTH probably isn't what you want. Removing ROOT is not
possible.


---------
Internals
---------

Storage
+++++++

In front of every word defined after (and including) ``forth-wl``,
i.e. in higher memory addresses, there's a cell ``wtag`` with the address
of the word list which the word is a member of.

A word list is identified by the fact that it's a constant which contains
its own lfa. Thus ``forth-wl lfa>wtag`` is equal to ``forth-wl``.

If either bit 0 or 1 of ``wtag`` are set *or* if the word is a wordlist,
another word before it may contain a context pointer. If bit 0 is set it's a
context switch; if bit 1 is set and the word is a vocabulary, the context
pointer contains the address of the parent vocabulary.


Word resolution
+++++++++++++++

The main word is ``vocs-find``. It is hooked to ``hook-find`` by ``init``.

Context switching is done by ``??-dictionary`` which is a replacement for
``find`` (i.e. its address is stored in ``hook-find``):

* Before searching the dictionary ``_?csr_`` checks whether the last
  interpreted word requested a temporary search context. If so, that
  context is used instead of the default search.

* After a successful dictionary search ``_!csr_`` records if the word in
  question requests a context switch.

* If an error occurs (i.e. ``quit`` is called), the temporary search
  context is cleared.


Support words
+++++++++++++

lfa>flags ( lfa -- h-addr )
---------------------------

Retrieves the flag half-cell of a word.

See the Mecrisp documentation for their meaning.

lfa>nfa ( lfa -- cstr )
-----------------------

Retrieves a word's name, printable via ``ctype`` and convertible to a
counted string via ``count``.

lfa>xt ( lfa -- xt )
--------------------

Retrieves the word's executable token, i.e. the address you'd get with
``' NAME``.

lfa>wtag ( lfa -- wtag )
------------------------

Retrieves the word's vocabulary tag, consisting of the vocabulary's address
and two possible flag bits.

tag>wid ( wtag -- wid )
-----------------------

Removes the flags from the vocabulary tag, leaving its word list ID.
This is identical to its lfa, as described above.

lfa>xt,flags ( a-addr -- xt|0 flags )
-------------------------------------

A shortcut to retrieve both executable token and flags of a lfa.

This accepts a lfa of zero for convenience.

last-lfa
--------

A variable that points to the most-recently-created word.

??-wl ( c-addr u wid -- lfa|0 )
-------------------------------

Searches a single word list.

vocnext ( wid1 -- wid2|0 )
--------------------------

Return the parent word list, i.e. the list which ``wid1`` inherits from.

??-vocs ( c-addr len a-addr -- lfa|0 )
--------------------------------------

Search a word list and its ancestors.

This search includes the root word list; it is used when context switching.

??-vocs-no-root ( c-addr len a-addr -- lfa|0 )
----------------------------------------------

Search a word list and its ancestors.

This search does not include the root word list; it is used during normal
search, as the root list must be searched last.

??-order ( c-addr u a-addr -- lfa|0 )
-------------------------------------

Search a number of word lists and their ancestors, depth-first.

``a-addr`` must point to the first cell in the ``context`` list, described
above. The list must contain the root vocabulary and a zero-valued cell at
the end.

??-dictionary ( c-addr len -- lfa|0 )
-------------------------------------

Search the dictionary according to the current state of the interpreter,
i.e. call ``??-vocs`` when context switching is in effect and ``??-order``
otherwise.

Return zero if not found.

(') ( str len -- lfa )
----------------------

Look up the LFA of a word. Print an error message and abort if not found.

(' ( "name" -- lfa )
--------------------

Look up the LFA of a word.

``(' NAME`` (interpreter mode) is equivalent to ``s" NAME" (')`` (compiler
mode).

(dovoc ( wid -- )
-----------------

Tell the interpreter to start a context switch, using ``wid`` as the
(initial) context.



Variables
+++++++++

context
-------

A list of ``#vocs`` cells (+1, guarding zero) with voabularies to search "normally".

Access via ``get-order`` and ``set-order``.

current
-------

The vocabulary where the next definition is to be added to.

Access via ``get-current`` and ``set-current``

_sop_
-----

The search order pointer.

The SOP addresses either the ``context`` or ``voc-context`` variable. The
latter happens when a context switching word has been looked up.

_csr_
-----

Context Switching Request.

If bit 0 is set, the lookup will clear the bit and return, i.e. it
acts as a Postpone flag.

After a lookup, ``_!csr_`` checks whether a context pointer exists and,
if so, stores it in ``_csr``.

Then, before the next lookup, ``_?csr`` stores the pointer in
``voc-context``, clears ``_csr_``, and temporarily points ``_sop_`` to
``voc-context`` instead of ``context``.


voc-context
-----------

The vocabulary that should be searched due to a context switch request.

This value is never changed (except by ``_!csr_``) and thus can be used as
a referent for the dictionary of the last word that had a context attached
to it, even if the switch has since been processed.

_indic_
-------

A flag. If true, context switching is supported, otherwise only the
compilation context is searched.

The reason for this is that Forth scans the dictionary when you define new
words. It prints a redefinition warning if it finds an old version.
Obviously this warning should only be emitted when the new word is in the
current dictionary itself.

Also, this lookup must not trigger our context switching support.

-------
History
-------

This code and documentation is based on version 0.8.4 by Manfred Mahlow.

Changes, so far:

* Debugging has been split off.

* The vocabulary-defining word ``voc`` has been renamed to ``voc:``.

* The vocabulary container for this extension itself has been renamed from
  ``inside`` to ``\\voc``; the word list is now ``\\voc-wl`` instead of
  ``inside-wordlist``. Likewise, ``forth-wordlist`` is now ``forth-wl``.
  Several other internal words have been shortened.

* ``voc:`` auto-switches the current vocabulary to itself, as the
  previously-required dance of ``voc foobar foobar definitions`` is rather
  tedious.

* ``only`` adds the current voc on top, not forth twice. The common idiom
  of ``forth only`` is thus unaffected, but you now can write ``foobar
  only`` instead of ``only foobar first``.

* ``forgetram`` is overridden to switch back to the ``forth`` vocabulary,
  just to protect against deleting a vocabulary the context is still
  pointing to.

* ``'`` and ``[']`` are now in the root vocabulary because otherwise you
  couldn't take the address of something that's only reachable by a context
  switch.

* The new ``ignore`` search order modifier removes a given vocabulary from
  the search order.

* Add ``offset:`` for declaring registers and similar constants.

* The built-in ``('`` now reports which word hasn't been found.

* ``.s`` is now sticky so that you can use it more easily for debugging.

* Some other minor optimizations and clean-ups, at least in this author's opinion.

* The original code's versioning comments et al. are of no interest to anybody
  else, and thus have been deleted.


-------------
Original docs
-------------

TODO: integrate these.

\ This is an implementation of a subset of words from the Forth Search-Order
\ word set.

\ ** This file must be loaded only once after a RESET (the dictionary in RAM
\    must be empty) and before any new defining word is added to Mecrisp-
\    Stellaris. It is and needs to be compiled to FLASH.

\ ** Requires

\    Mecrisp-Stellaris  2.3.6-hook-find  or  2.3.8-ra  or a later version with
\    hook-find.

' hook-find drop

\ * The Forth Search-Order and three wordlists are added:
\
\   FORTH-WORDLIST
\
\       \WORDS          ( -- )
\       FORTH-WORDLIST  ( -- wid )
\       VOC-WORDLIST    ( -- wid )
\       ROOT-WORDLIST   ( -- wid )
\       WORDLIST        ( -- wid )
\       SHOW-WORDLIST   ( wid -- )
\       GET-ORDER       ( -- wid1 ... widn n )
\       SET-ORDER       ( wid1 ... widn n | -1 -- )
\       SET-CURRENT     ( wid -- )
\       GET-CURRENT     ( -- wid )
\
\   ROOT-WORDLIST
\
\       INIT            ( -- )
\       WORDS           ( -- )
\       ORDER           ( -- )
\
\   VOC-WORDLIST
\   holds words needed for the implementation but normally not required for
\   applications.
\
\
\ * The default search order is FORTH-WORDLIST FORTH-WORDLIST ROOT-WORDLIST.
\
\ * The search order can be changed with GET-ORDER and SET-ORDER.
\
\ * Dictionary searching is done by the new word FIND-IN-DICTIONARY (defined in
\   the VOC-WORDLIST). It is called via HOOK-FIND by the now vectored Mecrisp
\   word FIND .
\
\ * New words are added to the FORTH-WORDLIST by default. This can be changed
\   by setting a new compilation context with <wordlist> SET-CURRENT.
\ * Compiling to FLASH and RAM is supported.
\
\ * The curious may take a look at the implementation notes at the end of this
\   file.
\
\ Some usage examples:
\
\   WORDLIST constant <name>  Creates an empty wordlist and assigns its wid to
\                             a constant.
\
\   <name> SHOW-WORDLIST      Lists all words of the wordlist <name>.
\
\   GET-ORDER NIP <name> SWAP SET-ORDER
\
\                             Overwrites the top of the search order.
\
\   <name> SET-CURRENT        Overwrites the compilation wordlist.
\
\   WORDS                     Lists all words of the top of the search order.
\                             ( initially this is the FORTH-WORDLIST )
\
\   \WORDS                    Alias for the word WORDS defined in the Mecrisp
\                             core. Ignores all wordlist related information.
\                             Might be useful in special debuging situations.
\
\   INIT                      Initialisation of the wordlists extension.
\
\ ------------------------------------------------------------------------------

\ ------------------------------------------------------------------------------
\ Implementation Notes:
\ ------------------------------------------------------------------------------
\ The code was created with Mecrisp-Stellaris 2.3.6 lm4f120 and tm4c1294 and
\ finally tested with Mecrisp-Stellaris 2.5.0 lm4f120-ra, msp432p401r-ra and
\ tm4c1294-ra.

\ Wordlists are not implemented as separate linked lists but by tagging words
\ with a wordlist identifier (wid). The tags are evaluated to find a word in a
\ specific wordlist. This idea was taken from noForth V.

\ The main difference to noForth is, that not all words are tagged but only
\ those, created after loading this extension. So only one minor change of the
\ Mecrisp-Stellaris Core was required: FIND had to be vectored (via HOOK-FIND).

\ A look at the Mecrisp-Stellaris dictionary structure shows, that a list entry
\ (a word) can be prefixed with the wid of the wordlist, the word belongs to.
\ This is what is done in this implementation.
\ ------------------------------------------------------------------------------

\ Address: 00004000 Link: 00004020 Flags: 00000081 Code: 0000400E Name: current
\ Address: 00004020 Link: 0000404C Flags: 00000000 Code: 00004030 Name: variable
\ Address: 0000404C Link: FFFFFFFF Flags: 00000000 Code: 0000405A Name: xt>nfa

\ 0404C         | Address (lfa) , holds the address of the next word or -1
\               |
\               |
\               |
\ cell+ = 04050 | Flags, 2 bytes    = lfa>flags
\         04051 |
\         04052 : 06     Name (nfa) = lfa>nfa
\         04053 : x
\               : t
\               : >
\               : n
\               : f
\               : a
\         04059 : 0    alignment
\ 405A          : Code (xt)         = lfa>xt = lfa>nfa skipstring

\ ------------------------------------------------------------------------------
\ After loading wordlists.txt all new words are prefixed/tagged with a wordlist-
\ tag ( wtag ).

\ wtag = wid || wflags

\  wid = identifier of the wordlist, the word belongs to

\  wflags = the 1 cells 2 / lowest bits of a wtag

\  we are only using Bit0 here (to be 16 Bit compatibel)

\   Filename: vis-0.8.4-core.fs
\    Purpose: Adds VOCs, ITEMs and STICKY Words to Mecrisp-Stellaris
\        MCU: *
\      Board: * , tested with TI StellarisLaunchPad
\       Core: Mecrisp-Stellaris by Matthias Koch.
\   Required: wordlists-0.8.4.fs for Mecrisp-Stellaris
\     Author: Manfred Mahlow          manfred.mahlow@forth-ev.de
\   Based on: vocs-0.7.0
\    Licence: GPLv3
\  Changelog: 2020-04-19 vis-0.8.2-core.txt --> vis-0.8.3-core.fs
\             2020-05-22 vis-0.8.4-core.fs  minor changes

\ Source Code Library for Mecrisp-Stellaris
\ ------------------------------------------------------------------------------
\              Vocabulary Prefixes ( VOCs ) for Mecrisp-Stellaris
\
\              Copyright (C) 2017-2020 Manfred Mahlow @ forth-ev.de
\
\        This is free software under the GNU General Public License v3.
\ ------------------------------------------------------------------------------
\ Vocabulary prefixes ( VOCs ) help to structure the dictionary, make it more
\ readable and can reduce the code size because of shorter word names.
\
\ Like VOCABULARYs VOCs are context switching words. While a vocabulary changes
\ the search order permanently, a VOC changes it only temporarily until the next
\ word from the input stream is interpreted. VOCs are immediate words.
\
\ VOCABULARYs and VOCs are words for explicit context switching.
\
\ This extension also supports implicit context switching ( see the words ITEM
\ and STICKY ) and (single) inheritanc for VOCs.

\ Implicit Context Switching:

\ Implicit context switching means that a "normal" Forth word is tagged with
\ the wordlist identfier (wid) of a VOC. When Forths outer interpreter FINDs
\ such a word, it is executed or compiled as normal (depending on STATE) and
\ the VOCs search order is set as the new search context. The next word from
\ the imput stream is then found in this context and afterwards the search
\ context is reset to the "normal" Forth search order.

\ Inheritance:

\ Inheritance means that a new VOC can inherit from (can extend) an existing
\ one. The search order of the new VOC is then the VOCs wordlist plus the
\ inherited VOCs search order.

\ So VOCs can be used to create libraries, register identifiers, data types
\ and to define classes for objects with early binding methods and (single)
\ inheritance.

\ Give it a try and you will find that VOCs are an easy to use and powerful
\ tool to write well factored code and code modules.

\ Glossary:

\ init ( -- )  Initialize the VOC extension.

\ ------------------------------------------------------------------------------

\ ------------------------------------------------------------------------------
\ Last Revision: MM-200522 0.8.3 : voc-init changed to only display (C) message
\                          on reset  find and (' added  ' and postpone changed
\                MM-200122 0.8.2 revision
\ ------------------------------------------------------------------------------
\ Implementation Notes:
\ ------------------------------------------------------------------------------
\ After loading wordlists.txt all new words are prefixed/tagged with a wordlist-
\ tag ( wtag ).

\ wtag = wid || wflags

\  wid = identifier of the wordlist, the word belongs to

\  wflags = the 1 cells 2 / lowest bits of a wtag

\  we are only using Bit0 here (to be 16 Bit compatibel)


\ To make a word a context switching one, it's additionally prefixed with a
\ context-tag ( ctag ) and bit wflags.0 is set.

\ ctag = wid || cflags

\ wid = identifier of the wordlist, to be set as top of the actual search order
\       after interpreting the word

\ cflags = the 1 cells 2 / lowest bits of a ctag ( not yet used )
