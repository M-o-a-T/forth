============
Linked lists
============

Linked lists, particularly doubly-linked ones, are a staple data structure
that's easy enough to get wrong. Thus we provide a small library for them.

The basics are easy enough::

    #require d-list-item lib/linked-list.fs

    d-list-head object: th
    d-list-item object: x1

Initially the head points to itself::

    #ok th next @ th .. =
    #ok th prev @ th .. =

Enqueue an item::

    x1 .. th enq
    #ok th next @ x1 .. =
    #ok x1 next @ th .. =
    #ok th prev @ x1 .. =
    #ok x1 prev @ th .. =

… and another::

    x2 .. th enq
    #ok th next @ x1 .. =
    #ok x1 next @ x2 .. =
    #ok x2 next @ th .. =
    #ok th prev @ x2 .. =
    #ok x2 prev @ x1 .. =
    #ok x1 prev @ th .. =

Dequeuing is easier and doesn't require the list head::

    x1 deq
    #ok th next @ x2 .. =
    #ok x2 next @ th .. =
    #ok th prev @ x2 .. =
    #ok x2 prev @ th .. =

You can easily walk through the list::

    0 variable count
    : process ( link ) 
    0 count ! ' process th (run) count @ .

Adding items, and dequeuing the current item from within PROCESS, is OK
during ``(run)``, but no other task may remove any items. Use a lock if
necessary. If you want to leave the loop prematurely, use THROW / CATCH.
``(run)`` leaves the stack alone, so other elements are accessible if
required.


Embedding
=========

You'll probably embed a list somewhere in your data structure. Two things
to keep in mind:

For one, always call the embedded list's SETUP from yours, this is not done
automatically.

Also, when you walk a list you'll need to get back to your original data
structure. This is easy enough::

    class: my-data
    __data
      int field: some-data
      int field: more-data

      \ The offset for our link, which is next, is on the stack. Thus:
      dup constant \link-off
      my-data item
      : >my-data \link-off - inline ;

      d-list-item field: link
      int field: even-more-data
    __seal

    : setup ( ptr -- )
      dup setup
      dup link setup
      \ set up or zero your other data fields here. Seriously.
      drop

You should not depend on the idea that you can put the link first and thus
forego calculating the offset. There may be other fields before yours,
particularly when debugging is turned on.
