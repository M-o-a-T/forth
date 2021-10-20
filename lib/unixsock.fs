\ Chat on a Unix-domain socket

forth only definitions

#require syscall

voc: unix

sys also

: connect ( adr len -- rfd wfd )
  pf unix here h!
  here 2+ over + 0 swap c!
  tuck here 2+ swap move ( len |R: adr )
  af unix  sock stream  pf unix  call socket  ( len sock )
  tuck swap here swap 2+ call connect ( sock )
  ?err 
  dup call dupfd
;

;voc

forth only definitions

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
