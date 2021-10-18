\ Small math helpers

forth only definitions
#if defined gcd
#end
#endif

forth definitions
: gcd ( a b -- gcd )
  begin
  ?dup while
    tuck mod
  repeat    
  2-foldable
;
 
: ratio ( a b -- a' b' )
  2dup gcd tuck ( a gcd b gcd )
  / -rot / swap
  2-foldable   
;              
 

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
