\ Compile executing 


#if token exec,, find drop 0=

\voc definitions

sticky  \ taken off by '
: exec,, ( "token" -- )
\ like postpone, but ignores any immediate flag
  ' literal, ['] call, call,  \ indirection galore
immediate ;

root definitions \voc also

\ Save the temp VOC around the next word
sticky  \ dropped by postpone
: [sticky] ( "token" -- ) 
  exec,, voc-context
  exec,, @
  exec,, >r
  postpone postpone
  exec,, r>
  exec,, voc-context
  exec,, !
immediate ;

#endif
