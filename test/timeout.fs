#if-flag !multi
#end
#endif

#if undefined time
#include lib/timeout.fs
#endif

#if time undefined now
#include lib/timeout2.fs
#endif

#if undefined *delay
#include debug/timeout.fs
#endif

forth definitions only
time also

compiletoram

:task: tt1
  ." T1 !" cr
  10 millis
  ." T1 A" cr
  20 millis
  ." T1 B" cr
  30 millis
  ." T1 C" cr
  40 millis
  ." T1 D" cr
;

:task: tt2
  ." T2 !" cr
  30 millis
  ." T2 A" cr
  20 millis
  ." T2 B" cr
  10 millis
  ." T2 C" cr
  40 millis
  ." T2 D" cr
;

:task: tt3
  ." T3 !" cr
  5 millis
  ." T3 A" cr
  55 millis
  ." T3 B" cr
  100 millis
  ." T3 C" cr
  1 millis
  ." T3 D" cr
;

time first

:task: ttm
  task yield
  3 *delay
  ." DLY: " . . . cr
  task yield
  3 *delay
  ." DLY: " . . . cr
;

\ now let's do some real tests

tt1 start  tt2 start  tt3 start  ttm start

task also
: yy
  begin tt1 state @ =dead <> while yield repeat
  begin tt2 state @ =dead <> while yield repeat
  begin tt3 state @ =dead <> while yield repeat
;

#delay 5
yy
#delay 0.5

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
