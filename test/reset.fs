\ This resets the system.

#delay 5
#-ok .. forth reset
#delay 1
#if-ok 1
\ possible startup nonsense from some task
#endif
#ok 1

compiletoram

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
