\ syscall, part 2: load after lib/multitask

#if undefined syscall
#end
#endif

#if sys defined epcb
#end
#endif

forth only
sys also
sys epoll epcb definitions also

#if-flag multi
: (tear) ( task -- flag )
  err EBADF swap  task %cls signal
  0
;
#endif
 
: teardown ( addr -- )
  dup __ teardown
  dup __ fd @  call close
#if-flag multi
  __ waiters each: (tear) drop
#else
  drop
#endif
;
 

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
