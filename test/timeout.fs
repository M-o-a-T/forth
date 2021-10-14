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

time also

:task: t1
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

:task: t2
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

:task: t3
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

:task: tm
  task yield
  3 *delay
  ." DLY: " . . . cr
  task yield
  3 *delay
  ." DLY: " . . . cr
;

\ now let's do some real tests

t1 start  t2 start  t3 start  tm start

task also
: yy
  begin t1 state @ =dead <> while yield repeat
  begin t2 state @ =dead <> while yield repeat
  begin t3 state @ =dead <> while yield repeat
;
#delay 5
yy
#delay 0.5

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
