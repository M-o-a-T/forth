forth only
class-root also
\voc \cls definitions

class-root class: sized
__ivar
  var> hint ivar: \offset
__seal

0 constant size
: size@
  s" size" voc-eval
;

: setup ( object -- )
  dup __ setup
  __ u/i@ swap __ \offset !
;

\voc sticky  \ temp VOC is cleared
: object: ( -- )
  \ add actual buffer size
  size@ swap
  \voc \cls class-item +
  buffer:
;


forth only definitions

