
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

: \dead sigenter ct-irq ;  \ no need to call sigexit

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
