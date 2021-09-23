\ Classes. They go to a subvocabulary of \voc.

#if undefined eval
#include lib/util.fs
#endif


forth definitions

voc: \cls

\voc also


#1234567890 constant ivr-sys

\ Abort with error message if called with an invald magic number (ivr-sys).
: ?ivr-sys ( magic n -- magic n )
  over ivr-sys - if ." invalid instance size" abort exit then ;


: +field ( n1 n2 "name" -- n3=n1+n2 ) \ Exec: a1 -- a2=a1+n1
\ Create a field in a structure definition with a size of n2 bytes.
\ n1 = size of the structure before creating  the field
\ n3 = size of the structure after creating the field
\
  <builds over , +
  does> @ +
;


\cls definitions
\ A root VOC for all classes.

voc: root-cls

0 constant u/i   \ root-cls has no instance data defined
0 constant size  \ root-cls has no additional data defined

\ retrieve a possibly-subclassed u/i
: u/i@ s" u/i" voc-eval ;
: size@ s" size" voc-eval ;
: setup@ s" setup" voc-eval ;


\ Return an object's data address.
: _addr_ ( oid -- addr )
  root-cls ['] .. execute immediate
;

\ Begin or extend an instance definition in a class definition.
: __data ( -- magic 0|inherited-size )
  ivr-sys current @ dup >r voc-context !
  s" u/i" 2dup r> ??-wl
  if ." instance is sealed" abort exit then
  voc-eval ( magic size )
  ." SIZE:" dup .
;


\ Terminate an instance definition in a class definition.
: __seal ( magic size -- )
  ?ivr-sys s" constant u/i" evaluate drop
;


\ Make the current class compilation context the actual search context.
: __ ( -- )
  get-current _csr_ ! immediate
;


\cls definitions

\ Assign the actual class context to the next created word and return the
\ instance size of the class on the stack.
: class-item ( -- u/i )
  \ get the instance size
  u/i@ size@ + item  \ compile the next word as vocabulary setter
;


root-cls definitions

\ Create an instance variable in the current class definition.
: field: ( "name" magic n1 -- magic n2 )
  ?ivr-sys class-item +field
;

: setup  ( object -- )
\ Initialize class variables.
  drop
;

: teardown  ( object -- )
\ Unhook this object. By default do nothing.
  drop
;

\ Create an instance of a class.
: object: ( "name" -- )
  \ XXX the next line depends heavily on VOC internals
  align here 2 cells +  ( lfa of future word )
  class-item buffer:
  \ now we go to the newly-created word's xt and run it
  \voc lfa>xt ( xt ) execute ( obj )
  \ calling the object sets SOP, which we need to undo
  [ ' .. call, ]
  \ find and run its setup method
  setup@
;

\ \ Create a class that only inherits from / extends the root class.
\ : class: ( "name" -- )
\   [ ' root-cls call, ] voc:
\   postpone ..
\ ;

\ This is the "unsized" externally visible root class
forth definitions

root-cls voc: class-root

: setup ( object -- )
  __ size@ abort" Not sized"
  __ setup
;

\cls also
\voc also

\ Create a class that inherits from / extends a class context.

\ When we declare "class: foo" followed by "class: bar", we want them to be
\ (a) siblings and (b) declared in the same vocabulary.
\ If you want a subclass, use "foo class: bar".

root definitions

: class: ( "name" -- )
  _sop_ @ dup @ swap context =
  if ( voc ) \ no prefix
    \ check if the current compile context is a class
    s" u/i@" context @ ??-vocs-no-root
    if
      \ drop it from search
      dup (ign
      current @ over = if
        dup lfa>wtag tag>wid current !
      then ( voc )
      vocnext
    else
      drop [ class-root .. voc-context @ literal, ]
    then
  else ( voc ) \ with this prefix
    ..
  then
  voc-extend
;

forth definitions

#if undefined var>
#include lib/vars.fs
#endif

forth definitions

\cls root-cls class: sized
__data
  var> hint field: \offset
__seal

: setup ( object -- )
  dup __ setup
  __ u/i@
  swap __ \offset !
;



forth definitions only

#ok depth 0=
