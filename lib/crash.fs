
\ -----------------------------------------------------------------------------
\  Trace of the return stack entries
\ -----------------------------------------------------------------------------

forth definitions only

#require .idd debug/voc.fs

\voc definitions also

0 variable closest-found
0 variable closest-d

: closest-chk ( word addr chk -- )
  2dup u> if drop 2drop exit then
  swap -
  ( word dist )
  dup closest-d @ u> if 2drop exit then
  ( word dist )
  closest-d !  closest-found !
;

: addr>woff ( address -- offset lfa | 0 )
  1 bic \ Thumb has LSB of address set.
  0 closest-found !
  -1 closest-d !

  >r
  dictionarystart
  begin
    \ check the word itself
    dup dup r@ closest-chk
    \ is it a buffer or a "ramallot" variable?  XXX Mecrisp specific
    dup lfa>flags h@ $180 and
    if
      dup dup lfa>xt execute r@ closest-chk
    then
    dictionarynext
  until
  drop rdrop

  closest-d @ dup $1000 u> if drop 0 exit then
  closest-found @ 
;

forth definitions

: .word-off ( address -- )
  addr>woff
  ?dup if ( off lfa )
    \voc .idd
    ?dup if ( off )
      [char] + emit base @  16 base ! swap .   base !
    then
  then
;

: .word ( address -- )
  addr>woff ?dup if \voc .idd space drop then
; 

\ Call trace on return stack.

\ Beware: This searches for the closest dictionary entry points to the addresses on the return stack
\         and may give random results for values that aren't return addresses.
\         I assume that users can decide from context which ones are correct.

: ct ( -- )
  cr
  0
  begin ( i )
    dup 0 <# # # #> type space
    dup rpick dup hex. dup .word-off
    ( i addr )
    addr>woff ?dup if
      nip lfa>nfa
      ( i name )
      dup count s" quit" compare if cr 2drop exit then
      dup count s" (go)" compare if cr drop exit then
      count s" (task)" compare if cr drop exit then
    then ( i )
    cr 1+
    dup 99 > if drop exit then
  again
;

#if defined unhandled

: ct-irq ( -- ) \ Try your very best to help tracing unhandled interrupt causes...
  cr cr
  unhandled
  cr
  \ h.s
  sp@ ." SP=" hex.
  rp@ ." RP=" hex.
  cr
  \ ." Calltrace:" ct
  reset \ Trap execution
;

: ct-init ['] ct-irq irq-fault ! ;
: init ct-init init ;
ct-init

#endif

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
