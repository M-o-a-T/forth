\ System initialization.
\ 
\ This file contains the basic minimum to get the rest of all of this up
\ and running.

\ We need to load the "vis" extension first, (a) because it doesn't upcall
\ 'init', second because currently (2021-09) there's a bug in Mecrisp(?)
\ that prevents it from working when it's not loaded first.
#if-flag erase
#delay 3
#-ok eraseflash
#delay 0.5
#endif

#include sys/voc.fs

compiletoflash
#if token defined find drop 0=
#include lib/util.fs
#endif

compiletoflash
#if undefined init
: init ;
#endif

#if-flag debug
#include debug/voc.fs

#if undefined dump
compiletoflash
#include lib/dump.fs
#endif

#endif
\ debug

compiletoflash
#include sys/abort.fs

#if-flag multi
#include sys/multitask.fs
#include lib/syscall2.fs
#endif

#if-flag multi
#echo
#echo ************
#echo  MULTI TASK
#echo ************
#echo
#else
#echo
#echo -------------
#echo  SINGLE TASK
#echo -------------
#echo
#endif

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
