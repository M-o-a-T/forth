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

    x1 .. th insert
    #ok th next @ x1 .. =
    #ok x1 next @ th .. =
    #ok th prev @ x1 .. =
    #ok x1 prev @ th .. =

… and another::

    x2 .. th insert
    #ok th next @ x1 .. =
    #ok x1 next @ x2 .. =
    #ok x2 next @ th .. =
    #ok th prev @ x2 .. =
    #ok x2 prev @ x1 .. =
    #ok x1 prev @ th .. =

Dequeuing is easier and doesn't require the list head::

    x1 remove
    #ok th next @ x2 .. =
    #ok x2 next @ th .. =
    #ok th prev @ x2 .. =
    #ok x2 prev @ th .. =

You can easily walk through the list::

    0 variable count
    : process ( link ) 
      0 count !
      ['] process th (run) count @ .

Adding items, and dequeuing the current item from within PROCESS, is OK
during ``(run)``, but no other task may remove any items. Use a lock if
necessary. If you want to leave the loop prematurely, use THROW / CATCH.
``(run)`` leaves the stack alone, so other elements are accessible if
required.

Classes
=======

d-list-head
+++++++++++

The head of a list. When set up this is initialized to point to itself,
thereby forming the head of a list.

You cannot remove the list head from a list.

insert ( item head -- )
-----------------------

Adds a new item to this list.

The item is inserted at the end, i.e. before the head.

run: ( "name" X* head -- Y* )
-----------------------------

Execute NAME with each list member's address, in turn, on the stack.

This word must take the address of each element from the list. It can
freely access the rest of the stack; ``head`` has already been removed.

NAME may remove the current item. It can freely add items to the list.
It must not remove any other item, and no other code may do so while ``run:``
is active.

(run) ( xt head -- )
--------------------

Like ``run:`` but expects an explicit execute token on the stack.

d-list-item
+++++++++++

A list entry. When set up the link pointers are initially zero; if
debugging is on, they are cleared when the item is removed.

Do not try to introspect the item to determine whether it is in a list;
always follow the list.

insert ( item this -- )
-----------------------

Adds a new item to the list the current item is on.

The new item is inserted behind the current item.

remove ( this -- )
------------------

Remove this item from whichever list it is on.

Trying to remove an item that is not on any list will crash your system.

Embedding
=========

You'll probably want to embed a linked list in another data structure.

Things to keep in mind:

* always call the embedded list's SETUP from yours. This is not done
  automatically.

* When you walk a list you'll need to get the address of your original data
  structure. This is easy enough::

    class: my-data
    __data
      int field: some-data
      int field: more-data

      \ The offset of our link is on the stack. Thus:
      \ First, let's make a subclass of the link pointer …
      d-list-item class: data-link
      \ … which knows this constant
      dup constant \link-off
      \ … and pretend that reading the link itself accesses our data
      : @ __ \link-off - inline ;
      ;class

      data-link field: link
      int field: even-more-data
    __seal

    : setup ( ptr -- )
      \ you need to explicitly call >SETUP on embedded fields
      dup __ link >setup
      \ initialize your other data fields here if they're not zero
      drop
    ;
    : some-method … ;
    ;class

    : process ( item -- )
    \ this word can be passed to a linked-list "run:" handler.
      my-data @ some-method
      \ This '@' simply subtracts our offset
    ;

You should not depend on the idea that you can put the link first and thus
don't need to bother calculating the offset. There may be other fields,
declared in a superclass.

Also, debug mode might want to place a magic number first, so that trying to
access a wrong address as an object fails early.
