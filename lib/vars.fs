#if undefined class:
#include lib/class.fs
#endif

forth definitions only

voc: var>

class: int

__data
  cell+
__seal

: @ ( a -- n ) @ inline ;
: ! ( n a -- ) ! inline ;
: ? ( a -- )  @ . ;


class: cint

__data
  1+
__seal

: @ ( a -- n ) c@ inline ;
: ! ( n a -- ) c! inline ;
: ? ( a -- )  c@ base @ swap hex  0 <# # # #> type space  base ! ;


class: hint

__data
  2 +
__seal

: @ ( a -- n ) h@ inline ;
: ! ( n a -- ) h! inline ;
: ? ( a -- )  h@ . ;


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
