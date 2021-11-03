forth definitions only

#require var> lib/vars.fs
#require class: lib/class.fs

\ doubly-linked list w/ list head.

#if \voc defined \d-list
forth definitions only
#end
#endif

forth only
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

#if-flag debug
: _free?
  __ prev @ poisoned <> if
    ." item not free: "
    over hex. over __ prev @ hex. over __ next @ hex. ct
    -22 abort
  then
;

#endif
: insert ( new this -- )
\ add new item before me
#if-flag debug
   over _free?
#endif
   dup >r __ prev @ .. over __ prev !   \ old's prev is now new's prev
   dup r@ __ prev !                     \ new is now old's prev
   r> over __ next !                    \ old is now new's next
   dup __ prev @ __ next !              \ new is now new's next's next
;

: append ( new this -- )
\ add new item after me
#if-flag debug
   over _free?
#endif
   dup >r __ next @ .. over __ next !   \ old's next is now new's next
   dup r@ __ next !                     \ new is now old's next
   r> over __ prev !                    \ old is now new's prev
   dup __ next @ __ prev !              \ new is now new's next's prev
;

forth definitions
\voc \d-list class: d-list-head

: setup ( ptr -- )
\ list heads are linked to themselves
  dup dup __ prev !
  dup     __ next !
;

: empty?  ( q -- flag )
  dup __ next @ .. swap =
;

: each ( xt head -- res )
\ run XT with each item until one call returns nonzero. Return that,
\ or zero if we ran through all elements.
\ XT must keep the current element but may otherwise do whatever else.
  swap >r  ( head |R: xt )
  dup __ next @
  begin
    2dup <>
  while ( head this )
    r@ rot >r ( this xt |R> xt head )
    over >r ( this xt |R: xt head this )
    execute ( flag )
    ?dup if
      rdrop 2rdrop exit
    then
    r> r> swap ( head this )
    __ next @ ( head next )
  repeat ( head head )
  rdrop 2drop
  0
;


: each.x ( xt head -- res )
\ run XT with each item until one call returns nonzero. Return that,
\ or zero if we ran through all elements.
\ XT may remove the current element but must not remove any others.
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
  dup __ prev @ over __ next @ __ prev !
  dup __ next @
#if-flag debug
                over
#else
                swap
#endif
                     __ prev @ __ next !
#if-flag debug
  poisoned over __ prev ! poisoned swap __ next !
#endif
;

forth only definitions

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
