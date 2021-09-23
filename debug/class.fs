\ Classes. They go to a subvocabulary of \voc.

#if \voc undefined ~voc:
#include debug/voc-debug.fs
#endif


forth only definitions

\voc also
\cls also

class-root definitions

\ Create a class that inherits from / extends the actual class context.
\ This is just a subvocabulary.

\ When we declare "class: foo" followed by "class: bar", we want them to be
\ (a) siblings and (b) declared in the same vocabulary.
\ If you want subclasses, use "foo class: bar".

: ~class: ( "name" -- )
  _sop_ @ dup @ swap context = if ( voc )
    dup (ign
    current @ over = if
      dup lfa>wtag tag>wid current !
    then ( voc )
    vocnext
  else
    ..
  then
  ~voc-extend
;


forth definitions

\ Create a class that only inherits from / extends class-root.
: ~class: ( "name" -- )
  [ ' class-root call, ] ~voc:
;

only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
