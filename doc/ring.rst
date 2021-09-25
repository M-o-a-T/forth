Ring buffer
===========

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
things. Let's store "real" cells:

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

    

