\ Test random ID things

#include lib/mach-id.fs

#ok #machid 4 >=
#ok #machid 12 <=

#echo First cell of machine id:
machid @ hex.

forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
