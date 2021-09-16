#include sys/init.fs

compiletoflash \ essential for testing, unfortunately
#if undefined ??
#include debug/vis.fs
#endif

compiletoram
#if undefined dump
#include lib/dump.fs
#endif

compiletoram

#if undefined class:
#include lib/class.fs
#endif

#if undefined ct
#include lib/crash.fs
#endif

#if undefined see
#include lib/disasm.fs
#endif

#if undefined int
#include test/class.fs
#endif

#if undefined ring
#include lib/ring.fs
#endif

#if undefined rr
#include test/ring.fs
#endif

\ #if defined syscall undefined save and
\ compiletoram
\ #include lib/save.fs
\ #endif
\ 
\ compiletoram
\ #if undefined .word
\ #include lib/crash.fs
\ #endif
