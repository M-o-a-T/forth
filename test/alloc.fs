
#include test/reset.fs

compiletoflash
#require dump lib/dump.fs
#require mem lib/alloc.fs
#require mem debug/alloc.fs
compiletoram


alloc also
pool object: tmem

220 tmem add
40 tmem alloc
80 tmem alloc
60 tmem alloc

#ok 2 pick >alloc msize 40 =
#ok over >alloc msize 80 =
#ok dup >alloc msize 60 =

tmem free
swap tmem free
30 tmem alloc
#ok dup >alloc msize 30 >=
swap tmem free
120 tmem alloc
#ok dup >alloc msize 120 >=
swap tmem free
tmem free
210 tmem alloc tmem free
#ok tmem ?free 216 =


\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
