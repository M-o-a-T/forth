\ A convenient memory dump helper

#if token forth find drop
forth only definitions
#endif

#if token dump find drop
#end
#endif

: u.4 ( u -- ) 0 <# # # # # #> type ;
: u.2 ( u -- ) 0 <# # # #> type ;

: dump16 ( addr -- ) \ Print 16 bytes memory
  base @ hex swap  ( base addr )
  dup $F bic swap $F and  ( base addr< addrf )
  over hex. ." :  "

  over dup 16 + swap do ( base addr< addrf )
    i $F and ( base addr f if )
    dup 8 = if space then
    over < if 9 spaces else
    i @ hex.
    then
  1 cells +loop
  drop

  ."  | "

  dup 16 + swap do
        i c@ 32 u>= i c@ 127 u< and if i c@ emit else [char] . emit then
        i $F and 7 = if 2 spaces then
      loop

  ."  |" cr
  base !
;

: dump ( addr len -- ) \ Print a memory region
  cr
  over + swap ( end addr )
  begin
    dup dump16
    $f bic
    $10 + ( end addr+16 )
    2dup <=
  until
  2drop
;

: dump16.8 ( addr -- ) \ Print 16 bytes memory
  base @ hex swap  ( base addr )
  dup $F bic swap $F and  ( base addr< addrf )
  over hex. ." :"

  over dup 16 + swap do ( base addr< addrf )
    i $F and ( base addr f if )
    space dup 3 and 0= if space then
    over < if 2 spaces else
    i @ u.2
    then
  loop
  drop

  ."  | "

  dup 16 + swap do
        i c@ 32 u>= i c@ 127 u< and if i c@ emit else [char] . emit then
        i $F and 7 = if 2 spaces then
      loop

  ."  |" cr
  base !
;

: dump8 ( addr len -- ) \ Print a memory region
  cr
  over + swap ( end addr )
  begin
    dup dump16.8
    $f bic
    $10 + ( end addr+16 )
    2dup <=
  until
  2drop
;

: list ( -- )
  cr
  dictionarystart
  begin
    dup 6 + ctype space
    dictionarynext
  until
  drop
;

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
