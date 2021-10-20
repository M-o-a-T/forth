=================
Memory Management
=================

Forth has traditionally been used on (if not "confined to") systems where
there's not enough memory available to manage anything but the programmer's
expectation of free RAM. However, (a) today there are Forth systems with
comfortable amounts of free memory, and (b) even on smaller systems
sometimes you need the flexibility to, say, either allocate ten small or
one large buffer.

This implementation is of the "really dumb" type. It doesn't even have a 
best-fit algorithm, though one could be added.

As memory fragmentation is a concern, esp. on small systems, this
implementation allows you to use multiple memory headers. Also, allocating
memory from large pools potentially locks out interrupts for longer.

+++++
Words
+++++

Global words
++++++++++++

pool object: NAME
-----------------

Allocate a new, mostly-indepenent memory pool.

``pool`` is in the ``alloc`` vocabulary.

mem
---

A global memory pool. Use this if you have no further restrictions.

``mem`` is in the Forth vocabulary.

>header ( adr -- hdr )
----------------------

Return the address of the control information for this address.

The address must have been returned by ``alloc``, below, and it must not
have been freed.

Pool words
++++++++++

add ( size pool -- )
--------------------

Pools are initially empty.

This word allots a ``size``-byte chunk of RAM and adds it to a pool.

The maximum size of a memory region allocatable from this block is a few
bytes smaller than ``size`` because we need space for truncated headers at
the beginning and end of each region.

alloc ( size pool -- adr )
--------------------------

Allocate ``size`` bytes from this pool.

Aborts if no free block with sufficient space can be found.

This word is interrupt safe.

free ( adr pool -- )
--------------------

Free the memory previously allocated with ``alloc``.

You can return an address to a different pool. This will not confuse our
memory allocation algorithm, but it causes any free areas following this
address to move to the new pool also, which probably is not what you want.

This word is interrupt safe.

?free ( pool -- size )
----------------------

Return the size of the largest free block in this pool.

This word is interrupt safe.

Header words
++++++++++++

msize ( hdr -- size )
----------------------

Return how many bytes this header controls.

This is at least the size used with ``alloc``.

