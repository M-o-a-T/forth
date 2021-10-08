\ Program Name: utils.fs  for Mecrisp-Stellaris by Matthias Koch and licensed under the GPL
\ Copyright 2019 t.porter <terry@tjporter.com.au> and licensed under the BSD license.
\ Also included is "bin." which prints the binary form of a number with no spaces between numbers for easy copy and pasting purposes

forth only
bits also definitions

\ -------------------
\  Beautiful output
\ -------------------

: u.n ( u n -- ) >r 0 <# r> 0 do # loop #> type ;
: u.0 ( u -- ) 0 <# #S #> type ;
: u.1 ( u -- ) 0 <# # #> type ;
: u.2 ( u -- ) 0 <# # # #> type ;
: u.3 ( u -- ) 0 <# # # # #> type ;
: u.4 ( u -- ) 0 <# # # # # #> type ;
: u.8 ( u -- ) 0 <# # # # # # # # # #> type ;
: h.n ( u n -- ) base @ hex -rot  u.n  base ! ;
: h.0 ( u -- ) base @ hex swap  u.0  base ! ;
: h.1 ( u -- ) base @ hex swap  u.1  base ! ;
: h.2 ( u -- ) base @ hex swap  u.2  base ! ;
: h.3 ( u -- ) base @ hex swap  u.3  base ! ;
: h.4 ( u -- ) base @ hex swap  u.4  base ! ;
: h.8 ( u -- ) base @ hex swap  u.8  base ! ;
: b.n ( u n -- ) base @ binary -rot  u.n  base ! ;
: b.0 ( u -- ) base @ binary swap  u.0  base ! ;
: b.1 ( u -- ) base @ binary swap  u.1  base ! ;
: b.2 ( u -- ) base @ binary swap  u.2  base ! ;
: b.3 ( u -- ) base @ binary swap  u.3  base ! ;
: b.4 ( u -- ) base @ binary swap  u.4  base ! ;
: b.8 ( u -- ) base @ binary swap  u.8  base ! ;

_rg definitions
\voc also
: ?r ( value width )
\ show all our bitfields
  \ Shortcut: all words in the current voc must have been declared
  \ after the voc itself, so start our search here
  \voc voc-context @ dup
  begin ( value width voc cur )
    dup lfa>wtag tag>wid 2 pick = if
      dup lfa>xt execute ( value width voc cur bit )
      dup $1f > if
        dup #5 rshift swap $1f and
        ( value width voc cur masklen shift )
        2dup tuck u.0 [char] : emit + 1- u.0 ( same )
        5 pick swap rshift
        ( value width voc cur masklen val )
        over bit 1- and swap
      else
        ( value width voc cur shift )
        dup u.0
        4 pick swap rshift 1 and
        1
      then space
      ( value width voc cur val masklen )
      2 pick lfa>nfa ctype [char] = emit b.n space space
    then
  dictionarynext until
  2drop 2drop
  cr
;
\voc ignore

&rg definitions
: ? __ @ dup 8 __ ?r [char] $ emit h.8 space ;

&rg16 definitions
: ? __ @ dup 4 __ ?r [char] $ emit h.4 space ;

&rg8 definitions
: ? __ @ dup 2 __ ?r [char] $ emit h.2 space ;

&bi definitions
: ? __ @ . ;

&bi16 definitions
: ? __ @ . ;

&bi8 definitions
: ? __ @ . ;

&bf definitions
: ? dup __ _spl nip -rot __ @ swap . ;

&bf16 definitions
: ? dup __ _spl nip -rot __ @ swap . ;

&bf8 definitions
: ? dup __ _spl nip -rot __ @ swap . ;


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
