\ This is the master loader.
\ It should be written to be idempotent.
\ 

#if token voc find drop 0=
\ We need to load the "vis" extension first, (a) because it doesn't upcall
\ 'init', second because currently (2021-09) there's a bug in Mecrisp(?)
\ that prevents it from working when it's not loaded first.
#include lib/vis.fs
#endif

#if token defined find drop 0=
\ We want to use "#if defined X" instead of the dance from above.

: defined   ( "token" -- flag ) token find drop 0<> ; 
: undefined ( "token" -- flag ) defined not ;
#endif

#if undefined init
: init ;
#endif

#if defined syscall undefined save and
compiletoram
#include lib/save.fs
#endif

compiletoram
#if undefined .word
#include lib/crash.fs
#endif
#if undefined multitask
#include lib/multitask.fs
#endif
