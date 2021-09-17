\ small utility code
\ WARNING must be called with "forth only definitions" in effect


compiletoram?
compiletoflash


#if token defined find drop 0=
\ We want to use "#if defined X" instead of the above dance.

only root definitions

sticky  \ deleted by "find"
: defined   ( "token" -- flag ) token find drop 0<> ; 

sticky  \ deleted by "find" in "defined"
: undefined ( "token" -- flag ) defined not ;

forth definitions

#endif


#if undefined eval

: eval  ( addr n -- )
\ given a word, find and run it.
  2dup find drop ?dup if
    -rot 2drop execute
  else
    space type ."  not found"
    -3 abort
  then
;
#endif

#if undefined voc-eval  defined \voc  and
: voc-eval  ( addr n -- )
\ given a word in the current vocabulary, find and run it.
  1 \voc _csr_ bis!
  2dup find drop ?dup if
    -rot 2drop execute
  else
    space type ."  not found"
    -3 abort
  then
;

#endif


\ mecrisp-without-RA doesn't have these

#if undefined roll
: roll ( xu xu-1 … x0 u -- xu-1 … x0 xu )
  ?dup if  \ zero is no-op
    dup
    begin ( x* u u' )
    ?dup while
      rot >r 1-
    repeat
    ( xu u |R: xu-1 … x0 )
    begin ?dup while
      r> -rot 1-
    repeat
  then
; 
#endif

#if undefined -roll
: -roll ( xu-1 … x0 xu u -- xu xu-1 … x0 )
  ?dup if  \ zero is no-op
    dup
    begin  ( x* xu u u' )
    ?dup while
      \ 3 roll
      >r rot r> swap
      >r 1-
    repeat
    ( xu u |R: xu-1 … x0 )
    begin
    ?dup while
      r> swap 1-
    repeat
  then
;
#endif


\ clean up
#if ( compiletoram-flag  -- )
compiletoram
#else
compiletoflash
#endif

