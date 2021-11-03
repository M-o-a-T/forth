#if undefined syscall
#end
#endif

#include test/reset.fs

compiletoflash
#require sys lib/syscall.fs
compiletoram

#if-flag multi
#include test/mt-term.fs
\ output is not stable without this
#endif

sys also
epoll also

timespec object: dly
#delay 1
0,5 dly !
dly sleep
#delay 0.2

\ for testing, create a pipe
call pipe 
constant rfd
constant wfd
6 buffer: dbuf

: bla s" Bla!" ;
wfd bla call write
#ok 4 =
rfd dbuf 6 call read
#ok 4 =
#ok dbuf 0 + c@ char B =
#ok dbuf 3 + c@ char ! =
dbuf 6 0 fill

#if-flag multi

rfd nonblock
: ?rchk
  rfd dbuf 6 call read
  ;
' ?rchk catch
#ok err EAGAIN =

epoll ignore

0 variable okr
:task: rdr
  rfd poll wait-read
  rfd dbuf 6 call read 4 <> abort" RDR"
  1 okr !
;

rdr start
:task: wrt
  5 time millis
  okr @ abort" early"
  wfd poll wait-write
  wfd bla call write 4 <> abort" WRT"
  10 time millis
  okr @ 1 <> abort" RES"
;
task !multi

wrt start

0 okr !
20 time millis
#ok okr @
#else

epcb object: ep

rfd ep wait-read
#ok depth 0=
#delay 1
500 ep poll
#ok -1 =
#ok depth 0=
#delay 0.2

wfd ep wait-write
#ok depth 0=

50 ep poll
.s
#ok wfd =
#ok depth 0=
wfd bla call write
#ok 4 =
500 ep poll
#ok rfd =
#delay 1
500 ep poll
#ok -1 =
#delay 0.2

#endif

\ close the test epoll instance
rfd sys call close
wfd sys call close
#if-flag !multi
ep teardown
#endif


\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
