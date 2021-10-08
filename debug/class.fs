\ Debug classes.

forth only definitions

\voc also
\cls also

\voc definitions

: is-sub? ( wid cwid -- flag )
\ check whether CWID is (the same or) a subclass of WID
  begin
    2dup = if
      2drop 1 exit
    then
    vocnext
  dup 0= until
  2drop 0
;

forth definitions

: .all ( lfa -- )
\ list all objects with this class
  >r
  dictionarystart begin
    ( test )
    dup obj-lfa>?cwid ?dup if
      ( test cwid )
      r@ over is-sub? if
        dup s" ?" rot ??-vocs-no-root
        ( test cwid lfa-? )
        ?dup if
          cr
          2 pick .idd ." :: " swap .idd cr
          over lfa>xt execute ( test lfa-? obj )
          swap lfa>xt execute
          cr
        else
          drop
        then
      else
        drop
      then
    then
  dictionarynext until
  drop rdrop
;


: .all' ( "name" -- list all objects of this class )
  (' .all
;

forth only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
