#include test/init.fs

#include test/bits.fs
#include test/mach-id.fs
#include test/offset.fs
#include test/timeout.fs
#include test/class.fs
#include test/linked-list.fs
#include test/io.fs
#include test/syscall.fs
#include test/crc.fs

#if-flag multi
#if undefined \multi
compiletoflash
#include sys/multitask.fs
#endif
#endif
\ if-flag multi

\ Testing multitasking on real hardware requires IRQ-based input
\ which requires ring buffers, but ring buffers require abort
\ which is modified when multitask gets loaded.
#include test/abort.fs
#include test/ring.fs
#include test/timeout.fs

#include test/mt-term.fs
#include test/multitask.fs
\ if-flag multi

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
#echo END OF TEST. SUCCESS.
