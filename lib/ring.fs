forth definitions only

#if undefined abort" ( " )
#include sys/abort.fs
#endif

#if undefined class:
#include lib/class.fs
#endif

#if undefined var>
#include lib/vars.fs
#endif

#if undefined voc-eval
#include lib/util.fs
#endif

forth definitions only
var> also

\ Predefined flags:
\ ring-var: the type to store in the ring. Must context-switch
\ to correctly-sized '@' and '!' words.

#if-flag !ring-var
#set-flag ring-var cint
#set-flag ring-name ring
#else
#set-flag ring-name ring-{ring-var}
#endif
#read-flag ring-esize {ring-var} u/i

#if defined {ring-name}
\ already known
#end
#endif


#if undefined ring-base
\voc definitions also

sized class: ring-base

__data
  hint field: limit
  hint field: start
  hint field: num
  aligned
#if-flag multi
  task %queue field: q-empty \ readers
  task %queue field: q-full  \ writers
#endif
__seal

\ xx constant elems
: elems@ s" elems" voc-eval ;

: setup ( ring -- )
\ initialize our variables
  __ elems@ over __ limit !  \ XXX depends on no overriding
#if-flag multi
  dup q-empty >setup
  dup q-full >setup
#endif
  drop
;

\ Words defined after this point only work after calling SETUP
\ ************************************************************

: mask ( offset ring -- offset&bitmask )
  __ limit @ over = if drop 0 then
;

#if-flag multi
: wait-empty ( ring -- )
\ wait for wake-up
  __ q-empty wait
;

: wake-empty (  ring -- )
\ wake up one waiting task
  __ q-empty one
;

: wait-full ( ring -- )
\ wait for wake-up
  __ q-full wait
;

: wake-full (  ring -- )
\ wake up one waiting task
  __ q-full one
;
#endif

: empty? ( ring -- bool )
  __ num @ 0=
;

: full? ( ring -- bool )
  dup __ num @ swap __ limit @ =
;

\ no SIZE, so not directly usable

;class

previous 
forth definitions

#endif
\ ring-base

#send \voc ring-base class: {ring-name}

: *esize ( elem-offset -- byte-offset )
#if-flag !ring-esize=1
#if-flag ring-esize=2
  1 lshift
#else
#if-flag ring-esize=4
  2 lshift
#else
#if-flag ring-esize=8
  3 lshift
#else
#send {ring-esize} *
#endif
#endif
#endif
#endif
  inline ;

: size  ( -- bytes )
  size elems@ *esize
  +
;

#if-flag debug
: ? ( ring -- )
  cr
  ." N:" dup __ limit ?
  ." E:" 1 __ *esize .
  ." S:" dup __ start ?
  ." #:" dup __ num ?
  ." Buf:" dup __ \offset @ swap + hex.
;
#endif

\ ************************************************************
: ! ( item* ring -- )
\ store to the ring buffer.
  >r

#if-flag multi
  begin
    r@ __ full?
  while 
    eint? if
      r@ __ wait-full
    else
      r> abort" Ring full"
    then
  repeat
#else
  r@ __ full? if r> abort" Ring full" then
#endif

  \ now compute where to store the next bit
  r@ __ start @ r@ __ num @ + r@ __ mask  __ *esize
  r@ __ \offset @ r@ + +
#send {ring-var} !
  r@ __ num @
#if-flag multi
  dup
#endif
  1+ r@ __ num !
  ( num-old ) \ when \multi is on

#if-flag multi
  \ 1st char? wake up
  0= if
    r@ __ wake-empty
  then
#endif

  rdrop
;

: w-e ( ring -- )
#if-flag multi
  begin
    dup __ empty?
  while
    eint? if
      dup __ wait-empty
    else
      abort" Ring empty"
    then
  repeat drop
#else
  __ empty? abort" Ring empty"
#endif
;

: @ ( ring -- item* )
  >r
  r@ w-e

  ( )
  r@ __ start @ __ *esize
  r@ __ \offset @ r@ + +
#send {ring-var} @
  r@ __ start @ 1+ r@ __ mask r@ __ start !
  r@ __ num @ 1- r@ __ num !

#if-flag multi
  \ wake up writer
  \ We only need that if the ring was full,
  \ but testing for that is more expensive than
  \ simply checking whether someone's waiting
  r> __ wake-full
#else
  rdrop
#endif
;


#if-flag ring-var=cint

\ TODO this should be (a) done with MOVE (b) extended to non-char

: s! ( addr count ring -- )
\ send a string to the ring
  -rot 0 do ( ring addr )
    2dup c@ swap __ !
    1+
  loop
  2drop
;

#endif

: s@ ( ring -- start count )
\ return an address plus the max #chars accessible
  >r
  r@ __ w-e
  r@ __ start @ __ *esize
  r@ __ \offset @ r@ + +
  ( start )

  r@ __ start @ r@ __ num @ +
  r@ __ limit @
  ( end lim )
  <= if \ OK to get all
    r@ __ num @
  else
    r@ __ limit @ r@ __ start @ -
  then
  rdrop
;

: skip ( len ring -- )
\ drops the first N chars, after S@
  >r
  r@ __ num @ over - dup 0< abort" not enough chars"
  ( len num- )
  r@ __ num !
  r@ __ start @ + r@ __ mask r@ __ start !
#if-flag multi
  r@ __ wake-empty
#endif
  rdrop
;

;class

#if-flag ring-var=cint
ring class: rc32
32 constant elems
;class

ring class: rc80
80 constant elems
;class

ring class: rc16
16 constant elems
;class

#endif


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
