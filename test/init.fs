#include sys/init.fs

compiletoflash \ essential for testing, unfortunately
#if undefined ??
#include debug/voc.fs
#endif

compiletoram
#if undefined dump
#include lib/dump.fs
#endif

compiletoram

\ call tracing, system abort exceptions
#if undefined ct
#include lib/crash.fs
#endif

\ word disassembly
#if undefined see
#include lib/disasm.fs
#endif
