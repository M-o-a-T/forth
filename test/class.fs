#if undefined var>
#include lib/vars.fs
#endif

forth definitions

var> also

int object: i1

12345 i1 !
i1 ?
i1 _addr_  dup hex.  @ .

int class: uint
: show ( a-addr -- ) __ @ u. ;

__ ?? ..


forth definitions

class-root class: point
__ivar
  int ivar: x
  int ivar: y
__seal


point object: p1

#100 p1 x !  #200 p1 y !

p1 x ?
p1 y ?

: get ( a-addr -- x y ) dup __ y @  swap __ x @ ;

: set ( x y a-addr -- ) dup >r __ y ! r> __ x ! ;

: show ( a-addr -- )    dup __ x ? __ y ? ;

p1 show

#1 #2 p1 set

p1 show

p1 get

2drop

