#include sys/init.fs

compiletoflash
forth definitions only
\ essential for testing, unfortunately

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
#include debug/crash.fs
#endif

\ word disassembly
#if undefined see
#include lib/disasm.fs
#endif

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
