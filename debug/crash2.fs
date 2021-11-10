
\ -----------------------------------------
\  Resolve addresses, dump the return stack
\ -----------------------------------------

forth definitions only

#require ct debug/crash.fs

\voc also

: -cti ( rp sp -- >reset )
  cr
  over ." Interrupt! RP=" hex.
  dup ." SP=" hex. cr
  $40 dump
  cr
  ." Calltrace:" -ct
  cr cr
  reset \ no, we can't continue
;

: ct-irq ( -- >reset ) \ Try your very best to help tracing unhandled interrupt causes...
  cr
#if-flag multi
  task !single
#endif
#[if] defined unhandled
  unhandled
#endif
  cr
  rp@ sp@ -cti
;

#if defined irq-fault
:init ['] ct-irq irq-fault ! ;
#endif

#if defined syscall
#require sys lib/syscall.fs

: ct-sig ( ctx info signo -- >reset )
  rot >r
  \ The top values of the stacks are in registers.
  \ Write them to the stacks.
  r@ $54 + @  2 cells -    \ Return stack pointer
  r@ $58 + @ over cell+ !  \ link register
  r@ $5C + @ over !        \ PC of crash address

  r@ $3C + @  1 cells -    \ Param stack pointer
  r@ $38 + @ over !        \ TOS register

  rdrop
  -cti
;


:init
  compiletoram

  ['] \dead  sys sig bus   sys call signal
  ['] \dead  sys sig ill   sys call signal
  ['] \dead  sys sig segv  sys call signal
;
compiletoflash

#endif


\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
