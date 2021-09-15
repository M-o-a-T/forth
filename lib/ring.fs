forth definitions only

#if undefined class:
#include lib/class.fs
#endif

#if undefined \multi
#include lib/multitask.fs
#endif

#if undefined var>
#include lib/vars.fs
#endif

#if undefined eval?
#include lib/util.fs
#endif

\ Bah. We need "exec,".
#if undefined [sticky]
#include lib/exec.fs
#endif

forth definitions only

var> also
\voc \cls also

class: ring
__ivar
  cint ivar: limit
  cint ivar: start
  cint ivar: end
  cint ivar: offset
  int ivar: task
__seal

0 constant size
: size@ s" size" voc-eval ;
: u/i@ s" u/i" voc-eval ;

: setup ( ring -- )
\ initialize our variables
  u/i@ over offset !
  size@ over limit !
  0 over start !
  0 over end !
  0 swap task !
;

\ ************************************************************
\ Words defined after this point only work after calling SETUP
\ ************************************************************

: mask ( offset ring -- offset&bitmask )
  __ limit @ over = if drop 0 then
;

: wait ( ring -- )
  dup __ task @ abort" Dup wait"
  \multi this-task  swap __ task !
  stop
  \ the waker clears
;

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

\voc sticky  \ temp VOC is cleared
: object: ( -- )
  \ add actual buffer size
  size@ swap
  class-item +  buffer:
  ..
;


: empty? ( ring -- bool )
  dup __ end @ swap __ start @ =
;

: full? ( ring -- bool )
  dup __ end @ over __ start @ - swap __ mask 1 =
;

: ! ( item ring -- )
  >r

  begin
    r@ __ end @ dup 1+ r@ __ mask dup r@ __ start @ = 
  while 
    .s
    2drop
    eint? if
      r@ __ wait
    else
      rdrop exit
    then
  repeat

  ( item end endn )
  -rot
  ( endn item end )
  
  \ 1st char? wake up
  dup r@ __ start @ = if
    r@ __ wake
  then
  
  r@ dup __ offset @ + + c!  ( endn )
  r> __ end !
;

: @ ( ring -- item )
  >r
  begin
    r@ __ start @  r@ __ end @  over =  while
    eint? if
      drop r@ __ wait
    else
      -6 abort" empty"
    then
  repeat
  ( start |r: ring )
  dup
  r@ dup __ offset @ + + c@
  swap 1+ r@ __ mask r@ __ start !

  \ wake up writer
  \ We should only need that if the ring was full
  \ but testing for it is more expensive than simply checking
  r> __ wake
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
32 constant size

class: r80
80 constant size

class: r16
16 constant size


forth definitions only
