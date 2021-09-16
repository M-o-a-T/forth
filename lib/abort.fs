forth definitions

#if defined \multi
#error Do not use with multitasking
#endif

#if undefined eint
: eint inline ;
: dint inline ;
: eint? false 0-foldable ;
#endif

#if defined \voc
\voc definitions
#endif

0 variable abortmsg
0 variable abortcode
0 variable handler

#if defined \voc
forth definitions \voc also
#endif

: catch ( x1 .. xn xt -- y1 .. yn throwcode / z1 .. zm 0 )
  [ $B430 h, ]  \ push { r4  r5 } to save I and I'
  sp@ >r handler @ >r rp@ handler !  execute
  r> handler !  rdrop  0 unloop ;

#if defined \voc
\voc definitions
#endif

: abort-quit ( * -- does-not-return )
\ End task.
  cr
  abortcode @ ?dup if 0 abortcode ! hex. then
  abortmsg @ ?dup if
    ." : " ctype cr
    0 abortmsg !
    dup -1 = if drop 0 then  \ ignore the ABORT" errcode
  then
  [ hook-quit @ call, ]
;

#if defined \voc
forth definitions
#endif

: throw ( throwcode -- )
  ?dup if
    handler @ ?dup if
      \ restore previous state to jump to
      rp! r> handler ! r> swap >r sp! drop r>
      unloop  exit
    else
      abortcode !
      abort-quit  \ unhandled error: stop task
    then
  then
;


: init init ' abort-quit hook-quit ! ;
' abort-quit hook-quit !

#if defined \voc
\voc definitions
#endif

: (abort) ( flag cstr )
\ runtime for abort"
  swap ?dup if
    swap abortmsg ! throw
  else drop then
;

#if defined \voc
forth definitions
#endif

: abort" postpone c" ( " )  ['] (abort) call, immediate ;

forth only
