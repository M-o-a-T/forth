#if-flag !multi
#end
#endif

forth definitions only
term also
task also

: qtype ( ptr len )
#if-flag debug
  0 ?do dup c@ qemit  task yield  1+ loop
#else
  type
#endif
;

:task: one
  s" This is task one. " qtype 
  10 0 do yield loop
  s" Enjoy." qtype  10 qemit
;
:task: two
  s" This is task two. " qtype
  yield yield
  s" Oh well." qtype   10 qemit
;
:task: three
  s" This is task three. " qtype
  yield
  s" Not again." qtype   10 qemit
;

one start  two start  three start

: ?slp
  begin yield one state @ =dead = until 
  begin yield two state @ =dead = until 
  begin yield three state @ =dead = until 
;

: slp
#if-flag !debug
  emit-init
#endif
  ['] ?slp catch
#if-flag !debug
  emit-exit
#endif
;

#if-flag debug
tasks
#endif
slp
#if-flag debug
tasks
#endif
#ok 0 =

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=

