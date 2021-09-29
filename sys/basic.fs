#if undefined \halt
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

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
