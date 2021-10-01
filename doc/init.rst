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

Thus we introduce a new system that's less error-prone and more composable
by basically hiding all the setup code.

Our ``init`` scans the dictionary. It executes all words named ``%init``,
no matter which vocabulary they're defined in. Also, for all named objects
their ``setup`` word will be executed.

The defining word for initialisation code auto-runs itself when complete,
and the object allocator auto-runs its ``setup`` word.

This method ensures that every piece of setup code runs in the exact
same order when you restart a system from Flash as when you defined it
originally.


init:
=====

Introduce code that will be called to set something up. It is a synonym to
``: %init``, except that the word thus declared will auto-run itself as
soon as it is completed.

Thus, you should replace all occurrences of::

    : init
      init
      \ whatever you need
    ;

with a more concise::

    :init
      \ whatever you need
    ;

Of course there may be occasions where it is useful to directly call your
``%init`` or ``setup`` words, most notably with error recovery. As
``%init`` words can now safely be stored in vocabularies and no longer need
to call up to any previous INIT, this sequence::

    foo definitions
    : foo-init 
      \ whatever
    ;
    : on-error
      foo-init
    ;
    forth definitions
    : init init foo-init ;

now simplifies to::

    foo definitions
    :init 
      \ whatever
    ;
    : on-error
      %init
    ;

