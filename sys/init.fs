\ System initialization.
\ 
\ This file contains the basic minimum to get the rest of all of this up
\ and running.

#if token vis find drop 0=
\ We need to load the "vis" extension first, (a) because it doesn't upcall
\ 'init', second because currently (2021-09) there's a bug in Mecrisp(?)
\ that prevents it from working when it's not loaded first.
#include lib/vis.fs
#endif

compiletoflash
#if token defined find drop 0=
#include lib/util.fs
#endif

compiletoflash
#if undefined init
: init ;
#endif

#if-flag multi
#include lib/multitask.fs
#else
#include lib/abort.fs
#endif

