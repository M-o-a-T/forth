\
\ system tick implementation

forth only
#require bits lib/bits.fs
#require gcd lib/math.fs

bits definitions also

#if-flag !clk
#error CLK unknown
#endif

#require systick svd/fs/core/{arch}/systick.fs
bits only definitions

#require nvic soc/armcm3/nvic.fs
bits only definitions

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

#if undefined tick
voc: tick
#else
tick also definitions
#endif

bits tick definitions

\ #if-flag debug
\ compiletoram
\ forth only
\ bits also
\ tick definitions also
\ #endif

#if defined clk_max
\ compiletoflash
forth only definitions
#end
#endif

\ get bit width of the RELOAD register
_systick _rvr reload .. 5 rshift
1 swap lshift \ 1 << 24-or-whatever
1 - constant clk_max \ max value we can store there

1 variable clk_div
1 variable clk_mul

: setclk  ( clock -- )
\ simplify the clock-to-µsec quotient and just save it
\ we expect UPDATE, below, to be called next
  1000000 ratio
  clk_div !
  clk_mul !
;

: µs>clk ( µsec -- clock )
  clk_mul @ clk_div @ */ 1-foldable
;
: clk>µs ( clock -- rem µsec )
  clk_div @ clk_mul @ */mod 1-foldable
;

10 µs>clk variable clk_min
clk_max clk>µs constant µs_max  drop  \ remainder



0 variable _now

0 variable clk_cur \ current countdown

0 variable td_delta

: _delta ( -- µs-since-last )
\ calculate how many ticks went by since last IRQ
\ this updates clk_cur but does NOT add the result to _now
\ which the caller must do, along with disabling interrupts
  systick cvr current @  clk_cur @ ( current initial )
  over -
  ( cur ini-cur )
  clk>µs ( cur rem µs )
  \ add the remainder back, for next time
  -rot + clk_cur !
;

time also definitions

: now-hq ( -- µsec )
\ return the current µsec value
  dint
  _delta _now @ + dup _now !
  eint
;

: now ( -- µsec )
\ return the current µsec value as of the last yield
  _now @
;

bits tick definitions
: tick-irq ( -- )
\ As the IRQ is triggered, the just-passed clock is converted
\ to µsec and added to _NOW. RVR is used as the next starting point,
\ we add our division's remainder to it, to stay accurate.
  clk_cur @ clk>µs ( rem µs )
  _now +!
  systick rvr reload @ + clk_cur !
;

: update-no ( -- )
\ turn off as far as possible, nobody wants us
  clk_max systick rvr reload !
;

: latest ( clk -- flag )
\ ensure that the timer triggers in at most clk clocks
\ the return flag says that we shouldn't halt
\ call this with interrupts disabled!
  systick cvr current @ ( max cur )
  \ if we get triggered soon anyway, don't bother now
  \ but store a new limit for later
  dup clk_min @ over > if 2drop systick rvr ! 1 exit then

  \ at this point our own interrupt is far off, but some other IRQ
  \ might delay us sufficiently anyway, so interrupts stay off.

  \ Some tolerance?
  ( max cur min )
  1 rshift + over < if drop 0 exit then
  ( max )
  clk_cur @
  \ now quickly replace the value.
  \ We know that CVR won't roll over any time soon,
  \ so no need to account for that.
  systick cvr current @
  ( max cur now )
  rot dup systick rvr !
  ( cur now max )
  0 systick cvr !
  clk_cur !
  ( cur now )
  -
  \ The difference needs to be accounted for.
  \ TODO add a small fudge value to make up for the lost ticks
  clk_div @ clk_mul @ */mod ( rem µs )
  _now +! clk_cur +!
  0  \ no need to loop quickly
;

: update ( now next -- flg )
\ given the time until the next timeout, and the one after that,
\ set up the clock and return whether the caller should not call WFI
  dup -1 <> if µs>clk then clk_max umin
  dup clk_min @ < if drop clk_max then
  swap µs>clk ( next now )
  dint
  latest if
    \ LATEST already updated clk_next
    drop -1
  else
    systick rvr ! 0
  eint
  then
;

:init
  nvic aiarc <% 7 prigroup *% MAGIC VECTKEY *% %>!
#send {clk}
  setclk
  systick csr <% clksource +% tickint -% enable +% %>!
  0 _now !
  clk_max dup systick rvr ! clk_cur !
  ['] tick-irq irq-systick !
  systick csr tickint +!
;

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
