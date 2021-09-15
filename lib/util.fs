\ small utility code


only forth definitions
compiletoram?


#if token defined find drop 0=
\ We want to use "#if defined X" instead of the above dance.

sticky  \ deleted by "find"
: defined   ( "token" -- flag ) token find drop 0<> ; 

sticky  \ deleted by "find" in "defined"
: undefined ( "token" -- flag ) defined not ;

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



\ clean up
#if ( compiletoram-flag  -- )
compiletoram
#else
compiletoflash
#endif

