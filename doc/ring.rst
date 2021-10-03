Ring buffer
===========

Intro
+++++

A ring buffer is a FIFO list. Our implementation supports arbitrary element
sizes and multitasking, i.e. waiting for a data item if the list is empty /
waiting for a slot if the list is full.

Our ring buffer implementation can handle arbitrary-sized objects:
it doesn't care how many cells the ``@`` and ``!`` operations send to /
take from the stack.

For the simple case, let's assume you want a buffer for your terminal.
Let's make it 100 characters long::

    #include lib/ring.fs

    ring class: rc100
    100 constant elems

    forth definitions
    rc100 object: buf

    #ok buf empty?
    #ok buf full? 0=

Trying to read from the buffer right now will cause an ABORT, except when
you're using multitasking and interrupts are enabled – then your task will
get suspended until somebody sends something.

Now we write something to the buffer::

    char H buf !
    char i buf !
    char ! buf !
    10 buf !

Char-sized buffers have an additional word to store a string::

    token Hello? buf s!  10 buf !

You can also retrieve multiple elements at a time; however, there's a
wrinkle: as the buffer wraps around, you might need to fetch the data 
in two parts. See ``s@`` and ``skip`` for details.

Let's add a procedure to our class that empties the buffer::

    rc100 definitions
    : out ( buf -- )
      begin dup __ empty? while dup __ @ emit repeat drop ;

    forth definitions
    buf out
    >> Hi!
    >> Hello?
    >> ok.

Now, character buffers are nice but sometimes you want to store larger
things. Because Forth doesn't do classes we need to duplicate the ring code
for that.

Let's store "real" cells::

    #set-flag ring-var int
    #include lib/ring.fs
    ring-int class: ri3
    3 constant elems

    forth definitions
    ri3 object: nums
    123456 nums !
    234567 nums !
    #ok nums @ 123456 =
    #ok nums @ 234567 =
    #ok nums empty?

You also can store pointers to objects. There's an additional requirement
here: ``@`` should change the search order so that the result is directly
useable::

    class: magic
    : . ( ptr -- ) ." It's a kind of magic!" cr drop ;
    forth definitions
    magic object: mg
    ri3 class: mring

    magic item
    : @ __ @ inline ; 

    forth definitions
    mring object: magic-ring
    mg .. magic-ring !

    \ … some time later …
    magic-ring @ .
    >> It's a kind of magic!
    >> ok.

Constants
+++++++++

elems
-----

The number of elements in the ring. Can be up to 65535 if you have that
much data (and RAM).

Words
+++++

empty? ( ring -- flag )
-----------------------

False if at least one data element can be read from the ring.

@ ( ring -- data* )
-------------------

Retrieve an element from the ring. On single-tasking systems,
``empty?`` must be False before calling this.

full? ( ring -- flag )
----------------------

False if at least one data element can be stored to the ring.

! ( data* ring -- )
-------------------

Store an element to the ring. On single-tasking systems,
``full?`` must be False before calling this.

s! ( ptr n ring -- )
--------------------

Retrieve ``n`` elements from memory, starting at ``ptr``, and store them to
the ring.

On single-tasking systems, there must be enough room in the ring. TODO: add
a function to test for that.

s@ ( ring -- ptr n )
--------------------

Return a pointer into the ring where ``n`` elements can be read.

The ring space occupied by these elements is not marked as free; you need
to call ``skip`` after processing them.

Never try to access more items than ``s@`` told you are available.

skip ( n ring -- )
------------------

Jump over the first ``n`` elements. Use this word after reading them
directly with ``s@``, to mark the space as free.

Never skip more items than ``s@`` told you are available.
