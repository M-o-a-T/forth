\ small utility code


only forth definitions
compiletoram?


#if undefined eval

compiletoflash

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


\ clean up
#if ( compiletoram-flag  -- )
compiletoram
#else
compiletoflash
#endif

