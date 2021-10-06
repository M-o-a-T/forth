forth only definitions
decimal

#if undefined ring
#include lib/ring.fs
#endif

#if undefined dump
#include lib/dump.fs
#endif
#ok depth 0=

#if-flag ram
compiletoram
#else
compiletoflash
#endif

ring class: r4
3 constant elems
;class

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

\ test whether data wrapping round the ring edge is handled correctly.
rr >setup
31 rr !
32 rr !
rr s@
#ok 2 =
#ok c@ 31 =
2 rr skip
33 rr !
34 rr !
rr s@
#ok 1 =
#ok c@ 33 =
1 rr skip
rr s@
#ok 1 =
#ok c@ 34 =
1 rr skip


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
1 variable foo
:init
  #2 foo ! ;
\voc %init!
#ok rr empty?
#ok foo @ 2 =
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

#if-flag multi

\ now let's do the multi-ring thing
0 variable done

:task: t1
  s" Hello Ring!" rr s!
  $0A rr ! \ cr
  0 rr !
  1 done !
;

:task: t2
  begin
    rr @
    ?dup while emit repeat
;
t2 start
t1 start
: tw begin  task yield  done @ until ;
tw

ring class: r9
9 constant elems
;class

r9 object: rd9

0 variable r9c
0 variable r9y
:task: r9w  100 0 do i rd9 !
  \ ." W " i . rd9 ? rd9 .. 30 dump
  r9y @ 1 and if task yield then loop ;
:task: r9r
  100 0 do
  r9y @ 2 and if task yield then
  rd9 @
  \ ." R " dup . i . rd9 ? rd9 .. 30 dump
  i <> if
    ." ERROR AT " i .
#if-flag debug
    rd9 ?
#endif
    cr rd9 .. 30 dump unloop 1 r9c ! exit then
  loop
  2 r9c !
;

: r9l  1000 0 do task yield r9c @ if unloop exit then loop ;

r9w start  r9r start
r9l
#ok r9c @ 2 =

1 r9y !  0 r9c !
r9w start  r9r start
r9l
#ok r9c @ 2 =

2 r9y !  0 r9c !
r9w start  r9r start
r9l
#ok r9c @ 2 =

3 r9y !  0 r9c !
r9w start  r9r start
r9l
#ok r9c @ 2 =

#endif

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
