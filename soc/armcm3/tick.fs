\
\ system tick implementation

forth only
bits definitions also

#if-flag !clk
#error CLK unknown
#endif

#if undefined rcc
#include svd/fs/core/{arch}/systick.fs
#endif

forth only
bits definitions also

\ Theory of operation:
\
\ The clock chip counts down to zero, then resets the counter to
\ the value of SYSTICK RVR RELOAD and fires off an interrupt.
\ 
\ The interrupt triggers our timers, and the timer code tells us
\ how long to wait next.
\ 
\ The tick update function receives the current and next
\ counter values. If the current value is not 

compiletoflash
voc: tick

#if undefined gcd
: gcd ( a b -- gcd )
  begin
  ?dup while
    tuck mod
  repeat
  2-foldable
;

: ratio ( a b -- a' b' )
  2dup gcd tuck ( a gcd b gcd )
  / -rot / swap
  2-foldable
;
  
#endif

\ compiletoflash \ not while testing
compiletoram
forth only
bits also
tick definitions also

1
\ get bit width of RELOAD register
_systick _rvr reload .. 5 rshift
lshift \ 1 << 24-or-whatever
1 - constant clk_max \ width of register

#send {clk}
1000000 ratio
variable clk_div
variable clk_mul

: µs>clk clk_mul @ clk_div @ */ 1-foldable ;

10 µs>clk variable clk_min
clk_max clk>µs constant µs_max



0 variable _now

0 variable clk_cur \ current countdown
0 variable clk_next \ timestamp when the next clock tick happens

0 variable td_delta

: _delta ( -- µs-since-last )
\ calculate how many ticks went by since last IRQ
\ this updates clk_cur but does NOT add the result to _now
\ which the caller must do, along with disabling interrupts
  systick cvr current @  clk_cur @ ( current initial )
  over -
  ( cur ini-cur )
  clk_div @ clk_mul @ */mod ( cur rem µs )
  \ add the remainder back, for next time
  -rot + clk_cur !
;

: now ( -- µsec )
\ return the current µsec value
  dint
  _delta _now @ + dup _now !
  eint
;

: tick-irq ( -- )
  clk_cur @ clk>µs _now +!
  clk_next @ clk_cur !
  1 triggered !
;

: update-no ( -- )
\ turn off as far as possible, nobody wants us
  clk_max dup clk_next ! systick rvr reload !
;

: latest ( clk -- flag )
\ ensure that the timer triggers in at most clk clocks
\ the return flag says that we shouldn't halt
  systick cvr current @ ( max cur )
  \ if we get triggered soon anyway, don't bother now
  \ but store a new limit for later
  dup clk_min @ over > if 2drop clk_next ! 1 exit then

  \ at this point our own interrupt is far off, but some other IRQ
  \ might delay us sufficiently anyway.

  \ Some tolerance?
  ( max cur min )
  1 rshift + swap < if drop 0 exit then
  ( max )
  clk_cur @
  \ now quickly replace the value.
  \ We know that there won't be an interrupt any time soon.
  systick cvr current @
  ( max cur now )
  rot dup systick rvr !
  ( cur now max )
  0 systick cvr !
  clk_cur !
  ( cur now )
  -
  \ The difference needs to be accounted for.
  \ TODO add some fudge value to make up for the lost ticks
  clk_div @ clk_mul @ */mod ( rem µs )
  _now +! clk_cur +!
;

: update ( now next -- )
  dup -1 <> if µs>clk then clk_max umax
  dup clk_min < if drop clk_max then
  swap µs>clk ( next now )
  dint
  latest if
    \ LATEST already updated clk_next
    drop -1
  else
    clk_next ! 0
  eint
  then
;

: setup
  dint
  0 _now !
  clk_max systick rvr !
  ['] tick-irq irq-systick !
  systick csr <% clksource +% tickint +% enable +% %>!
  0 systick cvr !
  eint
;

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
