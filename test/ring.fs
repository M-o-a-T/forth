forth only definitions
compiletoram decimal

ring class: r4
4 constant size

multitask
forth only definitions

r4 object: rr
rr setup
rr 40 dump 

#echo ## TEST
12 rr !
23 rr !
34 rr !
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
