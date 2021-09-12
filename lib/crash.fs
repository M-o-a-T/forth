
\ -----------------------------------------------------------------------------
\  Trace of the return stack entries
\ -----------------------------------------------------------------------------

0 variable closest-found

: addr>woff ( address -- cstr offset addr | 0 )
  1 bic \ Thumb has LSB of address set.
  dup flashvar-here u>= if drop 0 exit then \ Flash variables or peripheral registers cannot be resolved this way.
  0 closest-found ! \ Address zero (vector table) is more far away than all other addresses

  >r
  dictionarystart
  begin
    dup r@ u< \ No need for u<= because we are scanning dictionary headers here.
    if \ Is the address of this entry BEFORE the address which is to be found ?
      \ Distance to current   Latest best distance
      r@ over -               r@ closest-found @ -  <
      if dup closest-found ! then \ Is the current entry closer to the address which is to be found ?
    then
    dictionarynext
  until
  drop

  closest-found @ ?dup if
    6 + dup skipstring r> over - swap
  else
    rdrop 0
  then
;

: .word ( Address -- )
  addr>woff if swap ctype ?dup if ( len ) [char] + emit base @ swap . base ! then space then
;

: .word-off ( address -- )
  addr>woff if drop ctype space then
; 

\ Call trace on return stack.

\ Beware: This searches for the closest dictionary entry points to the addresses on the return stack
\         and may give random results for values that aren't return addresses.
\         I assume that users can decide from context which ones are correct.

: ct ( -- )
  cr
  rdepth 0 do
    i hex. i 2+ rpick dup hex. .word cr
  loop
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
