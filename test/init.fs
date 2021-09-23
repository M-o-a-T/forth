#include sys/init.fs

compiletoflash
\ essential for testing, unfortunately

#if-flag ram
: test-ram compiletoram ;
#else
: test-ram ;
#endif

#if-flag drop
: test-drop forgetram ;
#else
: test-drop ;
#endif


#if-flag debug
compiletoflash
#else
#if-flag ram
#end
\ skip the rest if it's going to be flushed anyway
#endif

test-ram
#endif

\ VOC word analysis
#if undefined ??
#include debug/voc.fs
#endif

\ memory dump
#if undefined dump
#include lib/dump.fs
#endif

\ call tracing, system abort exceptions
#if undefined ct
#include lib/crash.fs
#endif

\ word disassembly
#if undefined see
#include lib/disasm.fs
#endif

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
