#if undefined class-root
#include lib/class.fs
#endif

forth only

#if undefined var>
#include lib/vars.fs
#endif

forth only
class-root also
\cls definitions

class-root class: sized
__data
  var> hint field: \offset
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
  \cls class-item +
  buffer:
;


forth only definitions

