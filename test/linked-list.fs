forth definitions only

#require d-list-item lib/linked-list.fs

compiletoram

d-list-head object: th
d-list-item object: x1
d-list-item object: x2
d-list-item object: x3

#ok th next @ th .. =
#ok th prev @ th .. =

x1 .. th insert
#ok th next @ x1 .. =
#ok x1 next @ th .. =
#ok th prev @ x1 .. =
#ok x1 prev @ th .. =

x2 .. th insert
#ok th next @ x1 .. =
#ok x1 next @ x2 .. =
#ok x2 next @ th .. =
#ok th prev @ x2 .. =
#ok x2 prev @ x1 .. =
#ok x1 prev @ th .. =

0 variable n
: process hex. n @ 1+ n ! ;
' process th (run)
#ok n @ 2 =

x1 remove
#if-flag debug
#ok x1 prev @ poisoned =
#ok x1 next @ poisoned =
#endif
' process th (run)
#ok n @ 3 =

#ok th next @ x2 .. =
#ok x2 next @ th .. =
#ok th prev @ x2 .. =
#ok x2 prev @ th .. =

x2 remove
#ok th next @ th .. =
#ok th prev @ th .. =
' process th (run)
#ok n @ 3 =

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
