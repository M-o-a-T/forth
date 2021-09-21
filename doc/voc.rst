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

-----
Usage
-----

voc: ( "name" -- )
------------------

Create a stand-alone vocabulary prefix; it extends the root vocabulary.

<voc> voc ( "name" -- )  Create a vocabulary prefix that extends (inherits
                         from) the given voc.

<voc> ?? ( -- )   Show all words of the actual VOC search order and stay in
                  that VOC context.

.. ( -- )  Switch back from a VOC search order to the default Forth search
           order.

<voc> definitions ( -- )  Make <voc> the current compilation context.

<voc> item ( -- )  Make the next created word a context switching one, i.e.
                   #123 int item variable int1  \ int1 ( -- a ; NS: int )

sticky ( -- )  Make the next created word a sticky one.

@voc ( -- )  Make the current compilation context the actual search context.




---------
Internals
---------

Storage
+++++++

In front of every word defined after (and including) ``forth-wordlist``
there's a cell ``wtag`` with the address of the word list it points to.

A word list is identified by the fact that it's a constant which contains
its own lfa. Thus ``forth-wordlist lfa>wtag`` is equal to ``forth-wordlist``.

If either bit 0 or 1 of ``wtag`` are set *or* if the word is a wordlist,
another word before it may contain a context pointer. If bit 0 is set it's a
context switch; if bit 1 is set and the word is a vocabulary, the context
pointer contains the address of the parent vocabulary.


Word resolution
+++++++++++++++

The main word is ``vocs-find``. It is hooked to ``hook-find`` by ``init``.




\ Context switching is done by FIND-IN-Dictionary which is hooked to HOOK-FIND :

\ * Before searching the dictionary, it is checked ( by _?csr_ ), if the last
\   interpreted word requested to change the search context. Then it's done.

\ * After a successful dictionary search it is recorded ( by _!csr_ ) if a context
\   switch is requested. Then it will then be done ( by ?csp ) before the next
\   search.

\ * If an error occures, the search context is reset to the systems default
\   search order.



Variables
+++++++++

context
-------

a list of ``#vocs`` cells (+1, guard) with voabularies to search "normally".

Access via ``get-order`` and ``set-order``.

current
-------

the vocabulary where the next definition is to be added to

Access via ``get-current``and ``set-current``

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
words, and prints a redefinition warning if it finds it. Obviously this
warning should only be emitted when the word is in the same dictionary.
Also, the lookup would otherwise trigger our context switching support,
but we're defining a new word, so the context switch attributes of the old
word must not apply.

-------
History
-------

This code and documentation is based on version 0.8.4 by Manfred Mahlow.

* The vocabulary-defining word ``voc`` has been renamed to ``voc:``.

* The vocabulary for this extension itself has been renamed from ``inside``
  to ``\\voc``.

* Some minor optimizations and clean-ups, at least in this author's opinion.

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
