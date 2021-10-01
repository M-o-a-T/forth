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
\ ring-var: what to store in the ring. Must have '@' and '!' words.
\ i.e. '@' will set the search context for the next word.

#if-flag !ring-var
#set-flag ring-var cint
#set-flag ring-name ring
#else
#set-flag ring-name ring-{ring-var}
#read-flag ring-esize {ring-var} u/i
#endif

#if defined {ring-name}
\ already known
#end
#endif

#send sized class: {ring-name}
__data
  hint field: limit
  hint field: start
  hint field: num
  \ hint field: offset
#if-flag multi
  task %var field: waiter
#endif
__seal

\ xx constant elems
: elems@ s" elems" voc-eval ;

: setup ( ring -- )
\ initialize our variables
  dup __ setup
  __ elems@ over __ limit !  \ XXX depends on no overriding
  \ __ \offset @ size + offset !
  0 over __ start !
  0 over __ num !
#if-flag multi
  0 over __ waiter !
#endif
  drop
;

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

\ ************************************************************
\ Words defined after this point only work after calling SETUP
\ ************************************************************

: mask ( offset ring -- offset&bitmask )
  __ limit @ over = if drop 0 then
;

#if-flag multi
: (wait) ( ring -- flag )
\ if there's already a waiting task, hang around until the slot is free.
\ Ideally this should not happen.
  __ waiter @ .. if task =check else task =sched then
;

: wait ( ring -- )
  dup __ waiter @ .. abort" Dup wait"
  task this .. swap __ waiter !
  task stop
  \ the waker clears the var
;

: wake (  ring -- )
  dup __ waiter @ ..
  ?dup if
    ( ring task )
    0 rot __ waiter !
    task %cls continue
  else
    drop
  then
;
#endif

: empty? ( ring -- bool )
  __ num @ 0=
;

: full? ( ring -- bool )
  dup __ num @ swap __ limit @ =
;

: ! ( item* ring -- )
\ store to the ring buffer.
  >r

#if-flag multi
  begin
    r@ __ full?
  while 
    eint? if
      r@ __ wait
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
    r@ __ wake
  then
#endif

  rdrop
;

: @ ( ring -- item )
  >r
#if-flag multi
  begin
    r@ __ empty?
  while
    eint? if
      r@ __ wait
    else
      r> abort" Ring empty"
    then
  repeat
#else
  r@ __ empty? abort" Ring empty"
#endif

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
  r> __ wake
#else
  rdrop
#endif
;

;class

#if-flag ring-var=cint

: s! ( addr count ring )
\ send a string to the ring
  -rot 0 do ( ring addr )
    2dup c@ swap -- !
    1+
  loop
  2drop
;

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
