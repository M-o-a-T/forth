#if defined \halt
#end
#endif

\ Push / pop register-based context
\ TODO this is specific to Mecrisp Stellaris.
\ CTX is R4+R5, the innermost loop count+end
2 constant #ctx  \ number of cells pushed/popped
\ push context registers to return stack
: ctx>r $B430 h, immediate ;  \ push R4+R5
\ pop context registers from return stack
: r>ctx $BC30 h, immediate ;  \ pop R4+R5
\ halt the CPU until an interrupt arrives
#if defined irq-systick
: \halt [ $BF30 h, ] immediate ; \ WFI Opcode, Wait For Interrupt
#else
: \halt immediate ;  \ no-op, for now
#endif

\ Helper: decrement a stack pointer and store data there.
\ Here because needs modification on systems where the stack
\ does not predecrement-on-store.

: sp+! ( data sp -- sp-1 )
  1 cells -
  tuck ! inline ;

: stackfill ( addr cells -- ) 
\ fill a stack from the bottom
\ this overwrites the cell which the address points to. That address is not
\ part of the stack; we do this for stack underrun protection
  0 ?do
    poisoned over !
    1 cells -
  loop
  drop
;

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
