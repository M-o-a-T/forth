#if-flag !real
#end
#endif

compiletoflash
forth only definitions
#require gpio lib/gpio.fs
compiletoram

forth only definitions

0 2 gpio pin: pa2
0 3 gpio pin: pa2

pa2 mode af pp fast !
pa3 mode float !

pa2 +!
#ok pa2 @
#ok pa3 @

pa2 -!
#ok pa2 @ 0=
#ok pa3 @ 0=
