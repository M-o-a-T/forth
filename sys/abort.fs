\ This is the bare minimum abort implementation.
#ifndef abort
: abort ( -- ) cr quit ;
#endif

\ already known? bye
#ifdef abort"
#end
#endif

\ If \voc is undefined, we defer the rest for later
#ifndef \voc
#end
#endif

\voc definitions
#ifndef aborthandler

#if defined \multi
#include lib/multi.fs
\ multi.fs will re-import us
#end
#endif

0 variable aborthandler
\ otherwise somebody else, e.g. multitask, has defined it
#endif

\ storage for error message and code
0 variable abortmsg
0 variable abortcode

forth definitions
: throw ( throwcode -- )
\ Returns directly to the closest CATCH.
\ DO NOT call with a throwcode of zero. 
  handler @ ?dup if                     
    \ restore previous state to jump to 
    rp! r> handler ! r> swap >r sp! drop r>
    unloop  exit
  else
    abortcode !
    quit  \ unhandled error: stop task
  then
;

: catch ( x1 .. xn xt -- y1 .. yn throwcode / z1 .. zm 0 ) 
\ Call something, catching a possible call to THROW 
  [ $B430 h, ]  \ push { r4  r5 } to save I and I'
  sp@ >r  handler @ >r  rp@ handler !
  execute                                                      
  r> handler !  rdrop
  0 unloop
;           

\voc definitions
: abort-print
  cr
  abortcode @ ?dup if
    0 abortcode !
	hex.
  then
  abortmsg @ ?dup if
    ." : " ctype cr 
    0 abortmsg !   
  then
;

: abort-quit ( * -- does-not-return )
\ hook for QUIT
  aborthandler @ if
    \ if there's a handler, go to that instead
    -1 throw
  then
  \ spit out any errors
  abort-print
  \ and then call the original QUIT
  [ hook-quit @ call, ]
;

: (abort) ( flag cstr )
\ runtime for abort"
  swap ?dup if
    swap abortmsg ! throw
  else drop then
;

forth definitions

: abort" postpone c" ( " )  ['] (abort) call, immediate ;

\ Two reminders here.
: init
  \ (a) ALWAYS call the old INIT *first*.
  init
  \voc ' abort-quit hook-quit !
;
\ (b) At runtime do *not* call INIT, just do your own thing.
\voc ' abort-quit hook-quit !

#endif
forth only
