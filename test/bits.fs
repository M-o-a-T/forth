#include test/reset.fs

compiletoflash
#require bits
compiletoram

bits also
#require u.n debug/bits.fs
#require BFT test/gen/bugtest.fs

#ok BFT .. $12343210 =
\ need to override that
    
12 buffer: \BFT
\BFT _BFT port: BFT


\BFT 12 0 fill
: t \bft 4 + @ = ;
: tt \bft 4 + @  base @ swap binary . base ! ;
binary

#ok 0 t
bft br @
#ok 0 =
1 bft br x0 !
#ok 1 t
1 bft br x1 !
#ok 11 t
011 bft br x432 !
#ok 01111 t
110 bft br x432 !
#ok 11011 t
0 bft br x1 !
#ok 11001 t
bft br x1 +!
#ok 11011 t
bft br x1 -!
#ok 11001 t
$12 bft br x432 !
#ok 01001 t
bft br <% x5 +% #2 x8 *% %>!
#ok 100101001 t
bft br x1 +!
bft br <% x5 -% x0 -% 010 x432 *% 1 x76 *% %>!
#ok 101001010 t

decimal

bft br ?

forth definitions only
                
\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=   
