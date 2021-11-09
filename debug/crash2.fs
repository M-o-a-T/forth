
\ -----------------------------------------
\  Resolve addresses, dump the return stack
\ -----------------------------------------

forth definitions only

#require ct debug/crask.fs

\voc also

: ct-irq ( -- ) \ Try your very best to help tracing unhandled interrupt causes...
  cr cr
#[if] defined unhandled
  unhandled
#endif
  cr
  \ h.s \ not if we're somewhere random
  rp@ ." RP=" hex.
  sp@ ." SP=" hex. cr
  sp@ $40 dump
  cr cr
  ." Calltrace:" ct
  cr cr
  reset \ Trap execution
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
