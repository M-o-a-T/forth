forth definitions

#if undefined var>
compiletoflash
#include lib/vars.fs
#endif

test-ram

var> also

int object: i1

12345 i1 !
i1 ?
#ok i1 @ 12345 =
i1 _addr_  hex.
#ok i1 _addr_  @ 12345 =

int class: uint
: show ( a-addr -- ) __ @ u. ;

#if defined ??
__ ?? ..
#endif

#ok depth 0=

;class


class-root class: point
__data
  int field: x
  int field: y
__seal

: setup ( obj -- )
  -1 over __ x !
  -1 swap __ y !
;
;class

point object: p1
point object: p2

#ok p1 x @ -1 = 
#ok p2 y @ -1 =

#100 p1 x !  #200 p1 y !
#102 p2 x !  #202 p2 y !

p1 x ?
p1 y ?

#ok depth 0=
#ok p1 x @ #100 =
#ok p1 y @ #200 =
#ok p2 x @ #102 =
#ok p2 y @ #202 =

point definitions also
: get ( a-addr -- x y ) dup __ y @  swap __ x @ ;

: set ( x y a-addr -- ) dup >r __ y ! r> __ x ! ;

: show ( a-addr -- )    dup __ x ? __ y ? ;
forth only definitions

p1 show

#1 #2 p1 set

p1 show

p1 get .s 2drop
#ok p1 get #1 = swap #2 = and

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
