forth only definitions
decimal

#if undefined dump
#include lib/dump.fs
#endif
#ok depth 0=

ring class: r4
3 constant elems

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

\ leave one in on purpose, to test whether initialization works.
\ If compiling into RAM it may or may not work *shrug*
#if-flag !ram
#ok rr empty? not
\voc call-%init
\ #ok rr empty?
#endif

\ now let's do another of these with half words
.s

#set-flag ring-var hint
#include lib/ring.fs
ring-hint class: ring-h3
.s
3 constant elems
forth only definitions
ring-h3 object: rh3
.s

12345 rh3 !
23456 rh3 !
-1 rh3 !
.s
#ok rh3 @ 12345 =
#ok rh3 @ 23456 =
#ok rh3 @ $FFFF =
.s


\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
