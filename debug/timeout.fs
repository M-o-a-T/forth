forth definitions only

#require time lib/timeout.fs

#if defined *delay
#end
#endif

: *delay ( n -- n*delay )
\ measure n mainloop round-trip delays.
\ Leave them all on the stack for display / postprocessing.
  time now
  swap 0 do 
    task yield
    time now
    ( old new )           
    tuck swap - swap
    ( new-old new )
  loop              
  drop
  ;    

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
