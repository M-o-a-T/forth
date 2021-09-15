\ This is the master loader.
\ It should be written to be idempotent.
\ 

#if token vis find drop 0=
\ We need to load the "vis" extension first, (a) because it doesn't upcall
\ 'init', second because currently (2021-09) there's a bug in Mecrisp(?)
\ that prevents it from working when it's not loaded first.
#include lib/vis.fs
#endif

#if token defined find drop 0=
#error vis.fs should have done this
#include lib/util.fs
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
