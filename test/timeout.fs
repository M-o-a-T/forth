#if-flag !multi
#end
#endif

#if undefined time
#include lib/timeout.fs
#endif

#if undefined *delay
#include debug/timeout.fs
#endif

:task: t1
  ." T1 !" cr
  10000 task sleep
  ." T1 A" cr
  20000 task sleep
  ." T1 B" cr
  30000 task sleep
  ." T1 C" cr
  40000 task sleep
  ." T1 D" cr
;

:task: t2
  ." T2 !" cr
  30000 task sleep
  ." T2 A" cr
  20000 task sleep
  ." T2 B" cr
  10000 task sleep
  ." T2 C" cr
  40000 task sleep
  ." T2 D" cr
;

:task: t3
  ." T3 !" cr
  5000 task sleep
  ." T3 A" cr
  55000 task sleep
  ." T3 B" cr
  100000 task sleep
  ." T3 C" cr
  1000 task sleep
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
yy

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
