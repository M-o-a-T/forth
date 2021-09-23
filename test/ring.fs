forth only definitions
decimal

#if undefined dump
#include lib/dump.fs
#endif
#ok depth 0=

ring class: r4
4 constant elems

forth only definitions

r4 object: rr
rr 40 dump 

#echo ## TEST
#ok rr empty?
#ok rr full? not
12 rr !
23 rr !
#ok rr empty? not
#ok rr full? not
34 rr !

rr 20 dump 
#ok rr empty? not
#ok rr full?
#ok rr @ 12 =
#ok rr @ 23 =
#ok rr @ 34 =
#ok rr empty?
#ok rr full? not

1 rr !
12 rr !
23 rr !
: \push 34 rr ! ." ERROR" quit ;
: push ' \push catch . ;
#ok rr @ 1 =
#ok rr @ 12 =
#ok rr @ 23 =

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
