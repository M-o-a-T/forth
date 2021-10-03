\ This is the bare minimum abort implementation.
#if token forth find drop
forth definitions only
compiletoflash
#endif

#if token abort find drop 0=
: abort ( -- ) cr quit ;
#endif

\ already known? bye
#if token abort" ( " ) find drop
#end
#endif

\ If \voc is undefined, we defer the rest for later
#if-flag !plain
#if token \voc find drop 0=
#end
#endif
\voc definitions also
#endif

\ at this point the basics are known to be loaded,
\ thus we can use "#if defined".

#if undefined aborthandler

#if-flag multi
#include sys/multitask.fs
\ multitask.fs will re-import us
#end
#endif

0 variable aborthandler
\ otherwise somebody else, e.g. multitask, has defined it
#endif

\ storage for error message and code
0 variable abortmsg
0 variable abortcode

#if token forth find drop
forth definitions
#if undefined aborthandler
\voc also
#endif
#endif

#require r>ctx sys/base.fs
#if-flag debug
#require ct lib/crash.fs
#endif

: throw ( throwcode -- )
\ Returns directly to the closest CATCH.
\ DO NOT call with a throwcode of zero.
#if-flag debug
  dup ." THR:" hex. cr ct cr
#endif
  aborthandler @ ?dup if
    \ restore previous state to jump to
    rp! r> aborthandler ! r> swap >r sp! drop r>
    r>ctx  exit
  else
    abortcode !
    quit  \ unhandled error: stop task
  then
;

: catch ( x1 .. xn xt -- y1 .. yn throwcode / z1 .. zm 0 )
\ Call something, catching a possible call to THROW
  ctx>r sp@ >r  aborthandler @ >r  rp@ aborthandler !
  execute
  r> aborthandler !  rdrop r>ctx
  0
;

: .abort
  0
  abortcode @ ?dup if
    0 abortcode !
    hex
    drop 1
  then
  abortmsg @ ?dup if
    ." : " ctype
    0 abortmsg !
    drop 1
  then
  if cr then
;

#if-flag !plain
\voc definitions
#endif

: abort-quit ( * -- does-not-return )
\ hook for QUIT
  aborthandler @ if
    \ if there's a handler, go to that instead
    -56 throw
  then

  \ spit out any errors
  .abort
  \ and then call the original QUIT
  [ hook-quit @ call, ]
;

: (abort) ( flag cstr )
\ runtime for abort"
  swap ?dup if
    swap abortmsg ! throw
  else drop then
;

#if-flag !plain
forth definitions
#endif

: abort" postpone c" ( " )  ['] (abort) call, immediate ;

#if-flag plain
' abort-quit hook-quit !
#else
:init
  ['] abort-quit hook-quit !
;

#if token forth find drop
forth only
#endif
#endif

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
