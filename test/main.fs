#include test/init.fs

compiletoflash

#if undefined class:
#include lib/class.fs
#endif

compiletoram

#if undefined int
#include test/class.fs
forgetram
#endif


\ Testing multicast on real hardware requires IRQ-based input
\ which requires ring buffers, but ring buffers require abort
\ which is modified when multitask gets loaded.
\
\ Thus this order.

#if-flag multi
#if undefined \multi
compiletoflash
#include lib/multicast.fs
#endif
#endif

#if undefined ring
#include lib/ring.fs
#endif

compiletoram
#if undefined rr
#include test/ring.fs
forgetram
#endif

#if-flag multi
compiletoram
#if undefined tasks
#include debug/multitask.fs
#endif

#if undefined timetask
#include test/multitask.fs
forgetram
#endif
#endif


