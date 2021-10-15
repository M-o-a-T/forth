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

#if-flag debug
#include debug/linked-list.fs
#endif
\voc \d-list definitions also


: insert ( new this -- )
\ add new item before me
#if-flag debug
  over __ prev @ poisoned <> if
    ." item not free: "
    over hex. over __ prev @ hex. over __ next @ hex. ct
    -22 abort
  then
#endif
  tuck __ prev @ ( this new prev )
  2dup __ next ! over __ prev ! ( this new )
  2dup __ next ! swap __ prev ! ( new )
;


forth definitions
\voc \d-list class: d-list-head

: setup ( ptr -- )
\ list heads are linked to themselves
  dup dup __ prev !
  dup     __ next !
;

: each ( xt head -- res )
\ run XT with each item. See EACH: for details.
  swap >r  ( head |R: xt )
  dup __ next @
  begin
    2dup <>
  while ( head this )
    r@ rot >r ( this xt |R> xt head )
    over __ next @ >r ( this xt |R: xt head next )
    execute ( flag )
    ?dup if
      rdrop 2rdrop exit
    then
    r> r> swap ( head next )
  repeat ( head head )
  rdrop 2drop
  0
;

: each: ( head "name" -- res )
\ run NAME with each item until one call returns nonzero. Return that,
\ or zero if we ran through all elements.
  ' literal, postpone swap postpone each
  immediate
;

forth definitions
\voc \d-list class: d-list-item

#if-flag debug
: setup ( ptr -- )
  poisoned over __ prev !
  poisoned swap __ next !
;
#endif

: remove ( this -- )
\ remove from the list, kill the pointers if debugging.
\ You can't remove a list head, so this is here.
  dup __ prev @ over __ next @ ( this prev next )
  2dup __ prev ! swap __ next ! ( this )
#if-flag debug
  poisoned over __ prev ! poisoned swap __ next !
#else
  drop
#endif
;

forth only definitions

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
