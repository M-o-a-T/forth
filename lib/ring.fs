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
\cls also

sized class: ring
__data
  var> hint field: limit
  var> hint field: start
  var> hint field: end
  \ var> hint field: offset
#if defined \multi
  var> int  field: task
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
  0 over __ end !
#[if] defined \multi
  0 over __ task !
#endif
  drop
;

: size size elems@ + ;

\ ************************************************************
\ Words defined after this point only work after calling SETUP
\ ************************************************************

: mask ( offset ring -- offset&bitmask )
  __ limit @ over = if drop 0 then
;

#if defined \multi
: wait ( ring -- )
  dup __ task @ abort" Dup wait"
  \multi this-task  swap __ task !
  stop
  \ the waker clears
;
#endif

#if defined \multi
: wake (  ring -- )
  dup __ task @
  ?dup if
    ( ring task )
    0 rot __ task !
    forth wake
  else
    drop
  then
;
#endif

: empty? ( ring -- bool )
  dup __ end @ swap __ start @ =
;

: full? ( ring -- bool )
  dup __ end @ 1+ over __ mask swap __ start @ =
;

: ! ( item ring -- )
  >r

#[if] defined \multi
  begin
    r@ __ end @ dup 1+ r@ __ mask dup r@ __ start @ = 
  while 
    2drop
    eint? if
      r@ __ wait
    else
      r> abort" Ring full"
    then
  repeat
#else
  r@ __ full? if r> abort" Ring full" then
  r@ __ end @ dup 1+ r@ __ mask
#endif

  ( item end endn )
  -rot
  ( endn item end )
#[if] defined \multi
  dup 3 -roll ( end endn item end )
#endif

  r@ dup __ \offset @ + + c!  ( endn )  \ ! should be offset
  r@ __ end !

#[if] defined \multi
  \ 1st char? wake up
  r@ __ start @ = if
    r@ __ wake
  then
#endif
  rdrop
;

: @ ( ring -- item )
  >r
#[if] defined \multi
  begin
    r@ __ start @  r@ __ end @  over =  while
    eint? if
      drop r@ __ wait
    else
      r> abort" Ring empty"
    then
  repeat
#else
  r@ __ empty? if r> abort" Ring empty" then
  r@ __ start @
#endif
  ( start |r: ring )
  dup
  r@ dup __ \offset @ + + c@  \ ! should be offset
  swap 1+ r@ __ mask r@ __ start !

#[if] defined \multi
  \ wake up writer
  \ We should only need that if the ring was full
  \ but testing for it is more expensive than simply checking
  r> __ wake
#else
  rdrop
#endif
;

: s! ( addr count ring )
\ send a string to the ring
  -rot 0 do ( ring addr )
    2dup c@ swap -- !
    1+
  loop
  2drop
;
  

forth only
ring definitions

ring

\ subclasses for various sizes. We do it this way because Mecrisp cannot
\ init only part of a buffer.

class: r32
32 constant elems

class: r80
80 constant elems

class: r16
16 constant elems


forth definitions only

#ok depth 0=
