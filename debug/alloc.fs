forth only definitions

#require mem lib/alloc.fs
#if \voc \d-list undefined ?
#include debug/linked-list.fs
#endif

alloc also

hdr definitions
: ? 
  dup  __ flag @ case 
    __ *used* of  ." alloc:" endof
    __ *free* of  dup __ link ? ." free:" endof
    ." ??-not-alloc " 2drop exit
  endcase
  __ sz @ .
;

forth definitions

: mem? ( hdr -- )
  hdr hdr-link @ .. ( dbuf )
  hdr ?
;

mem definitions

: ?
  dup __ ?
  ." free:" __ ?free .
;

forth only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
