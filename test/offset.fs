
voc: gpio
$00 offset: in ( a1 -- a2 )
$02 offset: out
$04 offset: dir

: port: ( "name" a -- ) item constant ;
$1230 gpio port: p1 ( -- a1 )
$2340 gpio port: p2 ( -- a1 )

#ok p1 out $1232 =
#ok p2 dir $2344 =