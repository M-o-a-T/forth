forth definitions only
#require var> lib/vars.fs
#require class: lib/class.fs

forth definitions only

\ doubly-linked list w/ list head.

\voc definitions
var> also

class: \d-list
__data
  int field: prev
  int field: next
__seal

: enq ( new this -- )
\ add new item before me
  tuck __ prev @ ( this new prev )
  2dup __ next ! over __ prev ! ( this new )
  2dup __ next ! swap __ prev ! ( new )
;


forth definitions
\voc \d-list class: d-list-head

: setup ( ptr -- )
  dup setup
  dup dup __ prev !
  dup     __ next !
;

: (run) ( xt head -- ) 
\ run XT with each item.
  swap >r  ( head |R: xt )
  dup __ next @
  begin
    2dup <>
  while ( head this )
    r@ ( head this xt )
    rot >r ( this xt |R> xt head )
    over __ next @ >r ( this xt |R: xt head next )
    execute
    r> r> swap ( head next )
  repeat ( head head )
  r> 2drop drop
;

forth definitions
\voc \d-list class: d-list-item
: setup ( ptr -- )
  dup setup
  0 over __ prev !
  0 swap __ next !
;

: deq ( this -- )
\ remove from the list, zero the pointers.
\ You can't remove a list head, so this is here.
  dup __ prev @ over __ next @ ( this prev next )
  2dup __ prev ! swap __ next ! ( this )
#if-flag debug
  unused over __ prev ! unused swap __ next !
#else
  drop
#endif
;

forth only definitions

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=