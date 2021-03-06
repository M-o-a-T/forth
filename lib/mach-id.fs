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

#if-flag arch=armcm3
$1FFFF7E8 constant machid
12 constant #machid
\ accessible with any addressing mode, so can be used directly
#else
#error Don't know where to find the CPU ID for your architecture
#endif

#else
\ !real

#if-flag !machid
#error You need to declare "-F machid=<possibly-random-32bit-nr>".
#endif

#send {machid} variable machid
4 constant #machid

#endif
\ !real


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
