#include test/reset.fs

compiletoflash
#require \halt sys/basic.fs
#require abort" sys/abort.fs
\ ( " )
compiletoram

: err1 ." throwing 123" cr 123 throw ." ??? 1" cr ;
: ct1 ['] err1 catch 123 = ;
: ect1 ['] err1 catch 2* throw ;
: ct2 ['] ect1 catch 246 = ;

ct1
#ok ( )
ct2
#ok ( )

: check-abort ( -- flag )
  \voc aborthandler ." Handler at " dup hex. ." has " @ hex. cr
  ct1
  ct2 and
;


#if token forth find drop
forth definitions only
#endif

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
