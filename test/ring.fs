forth only definitions
compiletoram decimal

ring class: r4
4 constant size

forth only definitions

r4 object: rr
rr setup
rr 40 dump 

#echo ## TEST
rr empty? . rr full? .
12 rr !
23 rr !
rr empty? . rr full? .
34 rr !
rr empty? . rr full? .

rr 20 dump 
rr empty? .
rr @ .
rr @ .
rr @ .
rr empty? .

1 rr !
12 rr !
23 rr !
34 rr ! ( hangs )
