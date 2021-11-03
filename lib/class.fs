\ Classes. They go to a subvocabulary of \voc.

forth definitions only

#require eval lib/util.fs

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

: here: ( n1 "name" -- n1 ) \ Exec: a1 -- a2=a1+n1
\ Mark a position in a structure.
\ n1 = size of the structure before creating  the field
\
  <builds dup ,
  does> @ +
;

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
;


\ Terminate an instance definition in a class definition.
: __seal ( magic size -- )
  ?ivr-sys s" constant u/i" evaluate drop
;

: ;class
  ;voc
;

\cls definitions

\ Assign the actual class context to the next created word and return the
\ instance size of the class on the stack.
: mem-sz
  u/i@ size@ +
;

: class-item ( -- u/i )
  \ get the instance size
  mem-sz item  \ compile the next word as vocabulary setter
;


root-cls definitions

\ Create an instance variable in the current class definition.
: field: ( "name" magic n1 -- magic n2 )
  ?ivr-sys class-item +field
;

: setup  ( object -- )
\ Initialize class storage to zero.
  __ u/i@ 0 fill
;

: teardown  ( object -- )
\ Unhook this object. By default do nothing.
  drop
;

\voc also

\cls definitions
: (>setup) ( obj cvoc -- )
  voc-context @ >r
  swap !setup
  r> voc-context !
;

root-cls definitions
: >setup
\ Call an object's SETUP. Works both interactively and compiled.
  voc-context @
  forth state @ if \ compile
    literal, postpone (>setup)
  else \ interactive
    (>setup)
  then
immediate ;

\ Create an instance of a class.
: object: ( "name" -- )
  \ XXX the next line depends heavily on VOC internals
  align here 2 cells +
  ( lfa of future word )
  class-item buffer:
  ( lfa )
  \ run its setup words
  dup lfa>ctag tag>wid
  \ .s dup .idd
  swap lfa>xt execute
  !setup
  postpone ..
;

\voc ignore

\ \ Create a class that only inherits from / extends the root class.
\ : class: ( "name" -- )
\   [ ' root-cls call, ] voc:
\   postpone ..
\ ;

\ This is the "unsized" externally visible root class
forth definitions

root-cls voc: class-root

: setup ( object -- )
  drop
  __ size@ if ."  Not sized" abort then
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
    drop [ class-root .. voc-context @ literal, ]
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
  __ u/i@
  2dup swap __ \offset !
  \ zero the variable-size area
  + size@ 0 fill
;



forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
