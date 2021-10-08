#include test/init.fs

compiletoflash

#if-flag debug
compiletoflash

#if undefined ??
#include debug/voc.fs
#endif
#endif

compiletoflash

#if undefined class:
#include lib/class.fs
#endif

compiletoflash

#if undefined var>
#include lib/vars.fs
#endif


#if undefined p1
#include test/offset.fs
#endif

test-drop
test-ram

#if undefined .all
#include debug/class.fs
#endif

#if undefined yy
#include test/timeout.fs
#endif

test-drop
test-ram

#if undefined p1
#include test/class.fs
#endif

test-ram
#if undefined .all
#include debug/class.fs
#endif
\cls (' root-cls .all

test-drop

compiletoflash

#require d-list-head lib/linked-list.fs
#require th test/linked-list.fs

\ Testing multicast on real hardware requires IRQ-based input
\ which requires ring buffers, but ring buffers require abort
\ which is modified when multitask gets loaded.
\
\ Thus this order.

#if-flag multi
#if undefined \multi
compiletoflash
#include sys/multitask.fs
#endif
#endif
\ if-flag multi

compiletoflash

#if undefined ring
#include lib/ring.fs
#endif

test-ram

#if undefined rr
#include test/ring.fs
#endif

#if-flag multi

test-drop

compiletoflash

#if undefined term
#include lib/mt-term.fs
test-ram
#include test/mt-term.fs
#endif

test-ram

#if-flag debug
#if undefined tasks
#include debug/multitask.fs
#endif
#endif

test-ram

#if undefined timetask
#include test/multitask.fs
#endif

#endif
\ if-flag multi

test-drop

#if defined syscall

#include lib/syscall.fs
#include test/syscall.fs

#endif

#if undefined BFT
#include test/bits.fs
#endif

#include test/mach-id.fs

test-drop

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
#echo END OF TEST. SUCCESS.
