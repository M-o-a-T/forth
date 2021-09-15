voc: var>

class: int

__ivar
  cell+
__seal

: @ ( a -- n ) @ inline ;
: ! ( n a -- ) ! inline ;
: ? ( a -- )  @ . ;


forth definitions only


class: cint

__ivar
  1+
__seal

: @ ( a -- n ) c@ inline ;
: ! ( n a -- ) c! inline ;
: ? ( a -- )  c@ base @ swap hex  0 <# # # #> type space  base ! ;


forth definitions only

class: hint

__ivar
  2 +
__seal

: @ ( a -- n ) h@ inline ;
: ! ( n a -- ) h! inline ;
: ? ( a -- )  h@ . ;


forth definitions only

