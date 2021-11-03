\ basic time

forth definitions only

#if defined time
#if time defined now
#end
#endif
#endif

#if defined syscall
#include lib/syscall.fs
#endif

#if undefined time
voc: time
#else
time definitions also
#endif

#if defined syscall
sys also

monotonic object: systime

: now
  systime get
  systime @  ( float.seconds )
  1000000, f* nip
;

: now-hq now ;

#else

\ include the real MCU's tick configuration here

#if-flag !real
#error On virtual hardware but no syscall? doesn't work
#endif

#include soc/{arch}/tick.fs

#endif

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=    
