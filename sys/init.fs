\ System initialization.
\ 
\ This file contains the basic minimum to get the rest of all of this up
\ and running.

#if token vis find drop 0=
\ We need to load the "vis" extension first, (a) because it doesn't upcall
\ 'init', second because currently (2021-09) there's a bug in Mecrisp(?)
\ that prevents it from working when it's not loaded first.
#include sys/voc.fs
#endif

compiletoflash
#if token defined find drop 0=
#include lib/util.fs
#endif

compiletoflash
#if undefined init
: init ;
#endif

#if-flag debug
#include debug/vis.fs
#endif

#include sys/abort.fs

#if-flag multi
#include sys/multitask.fs

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

