forth definitions only

#require class: lib/class.fs

#if defined var>
#end
#endif

voc: var>

class: int

__data
  cell+
__seal

: @ ( a -- n ) @ inline ;
: ! ( n a -- ) ! inline ;
: ? ( a -- )  @ . ;
: +! ( n a -- ) +! inline ;

;class

int class: uint
: ? ( a -- )  @ u. ;
;class

class: cint

__data
  1+
__seal

: @ ( a -- n ) c@ inline ;
: ! ( n a -- ) c! inline ;
: ? ( a -- )  c@ base @ swap hex  0 <# # # #> type space  base ! ;

;class

class: hint

__data
  2 +
__seal

: @ ( a -- n ) h@ inline ;
: ! ( n a -- ) h! inline ;
: ? ( a -- )  h@ . ;
: +! ( n a -- ) dup h@ rot + swap h! ;

;class

class: dint

__data
  cell+ cell+
__seal

: @ ( a -- n ) 2@ inline ;
: ! ( n a -- ) 2! inline ;
: ? ( a -- )  2@ ud. ;
: +! ( n a -- ) >r r@ 2@ d+ r> 2! ;

;class

dint class: fixed
\ same as double, but fixcomma: print sensibly
: ? ( a -- )  2@ 3 f.n ;

;class

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
