\ This is the loader for debugging tools.
\ 

\ declare "#if defined X" instead of "#ifdef X" to avoid Forth errors.
\ We will need that later because the real serial handler won't tolerate errors.

#ifndef defined
compiletoflash
: defined   ( "token" -- flag ) token find drop 0<> ; 
: undefined ( "token" -- flag ) defined not ; 
\ delay
\ : !dly #100000 0 do loop immediate ;
#endif

#if undefined init
: init ;
#endif

compiletoram

#if undefined .word
#include lib/crash.fs
#endif

#if undefined dump
#include lib/dump.fs
#endif

#if undefined see
#include lib/disasm.fs
#endif

