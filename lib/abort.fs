forth definitions

#if defined \multi
#error Do not use with multitasking
#endif

0 variable abortmsg
0 variable handler

: catch ( x1 .. xn xt -- y1 .. yn throwcode / z1 .. zm 0 )
    [ $B430 h, ]  \ push { r4  r5 } to save I and I'
    sp@ >r handler @ >r rp@ handler !  execute
    r> handler !  rdrop  0 unloop ;

: throw ( throwcode -- )  ?dup IF
    \ handler @ 0= IF false task-state ! THEN \ unhandled error: stop task
    handler @ rp! r> handler ! r> swap >r sp! drop r>
    UNLOOP  EXIT
    THEN ;

: abort-quit ( * -- does-not-return )
\ End task.
  cr
  abortmsg @ ?dup if 
    ." : " ctype cr
    0 abortmsg !
    dup -1 = if drop 0 then  \ ignore the ABORT" errcode
  then
  [ hook-quit @ call, ]
;
: init init ' abort-quit hook-quit ! ;
' abort-quit hook-quit !

: (abort) ( flag cstr )
\ runtime for abort"
  swap ?dup if
    swap abortmsg ! throw
  else drop then 
;

: abort" postpone c" ( " )  ['] (abort) call, immediate ;

