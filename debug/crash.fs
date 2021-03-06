
\ -----------------------------------------
\  Resolve addresses, dump the return stack
\ -----------------------------------------

forth definitions only

\voc also
#require .idd debug/voc.fs
#require dump lib/dump.fs

\voc definitions also

#if defined closest-found
forth definitions only
#end
#endif

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
    dup smudged? if
      dup dup r@ closest-chk
      \ is it a buffer or a "ramallot" variable?  XXX Mecrisp specific
      dup lfa>flags h@ $180 and
      if
        dup dup lfa>xt execute r@ closest-chk
      then
    then
    dictionarynext
  until
  drop rdrop

  closest-d @ dup $100 u> if drop 0 exit then
  closest-found @ 
;

forth definitions

: .word-off ( address -- )
\ print word name and offset. Print adress if not found.
  dup addr>woff
  ?dup if ( addr off lfa )
    \voc .idd
    ?dup if ( addr off )
      [char] + emit base @  16 base ! swap .   base !
    then ( addr )
    drop
  else
    ." ?:" hex.
  then
;

: .word-off-n ( address -- )
\ print word name and offset, but don't say anything if not found
  addr>woff
  ?dup if ( addr off lfa )
    \voc .idd
    ?dup if ( addr off )
      [char] + emit base @  16 base ! swap .   base !
    then ( addr )
  then
;

: .word ( address -- )
  dup addr>woff ?dup if
    \voc .idd space 2drop
  else
    ." ?:" hex.
  then
; 

\ Call trace on return stack.

\ Beware: This searches for the closest dictionary entry points to the addresses on the return stack
\         and may give random results for values that aren't return addresses.
\         I assume that users can decide from context which ones are correct.

: -ct ( rp -- )
  cr
  0
  begin ( i )
    dup 0 <# # # #> type space
    2dup cells + @ dup hex. dup .word-off-n
    ( i addr )
    addr>woff ?dup if
      nip lfa>nfa
      ( i name )
      dup count s" quit" compare if cr 2drop drop exit then
      dup count s" (go)" compare if cr 2drop exit then
      count s" (task)" compare if cr 2drop exit then
    then ( i )
    cr 1+
    dup 99 > if drop exit then
  again
  drop
;

: ct rp@ -ct ;

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
