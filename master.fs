\ This is the master loader.
\ 
compiletoflash

\ declare "#if defined X" instead of "#ifdef X" to avoid Forth errors.
\ We will need that later because the real serial handler won't tolerate any errors.

: defined   ( "token" -- flag ) token find drop 0<> ; 
: undefined ( "token" -- flag ) token find drop 0=  ; 
\ delay
: !dly #100000 0 do loop immediate ;

#if undefined init
: init ;
#endif

#include vis
\ #include lib/multitask.fs
