\
\ Declare a machine ID.
\
\ * fake machines: needs to be supplied as a 32-bit number.
\
\ * real: stm32: use the "real" machine ID directly.

forth definitions only

#if defined mach-id
#end
#endif

#if-flag real

$1FFFF7E8 constant machid
12 constant #machid
\ accessible with any address mode, so can be used directly

#else
#if-flag !machid
#error You need to declare "-F machid=<possibly-random-32bit-nr>".
#endif

0 variable machid
4 constant #machid
:init
#send {machid}
  machid !
;

#endif
\ !real


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
