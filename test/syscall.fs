#if undefined syscall
#end
#endif

forth only definitions
sys also
epoll also

timespec object: dly
#delay 1
0,5 dly !
dly sleep
#delay 0.2

epcb object: ep

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

0 variable okr
:task: rdr
  rfd ep wait-read
  rfd dbuf 6 call read 4 <> abort" RDR"
  1 okr !
;

rdr start
:task: wrt
  task yield
  task yield
  okr @ abort" early"
  wfd ep wait-write
  wfd bla call write 4 <> abort" WRT"
  task yield
  okr @ 1 <> abort" RES"
;
wrt start
: pol
  10 0 do
    task yield
	0, ep poll .
	okr @ if unloop exit then
  loop
;
pol

#else

rfd ep wait-read
#ok depth 0=
#delay 1
0,5 ep poll
#ok -1 =
#ok depth 0=
#delay 0.2

wfd ep wait-write
#ok depth 0=

0,5 ep poll
.s
#ok wfd =
#ok depth 0=
wfd bla call write
#ok 4 =
0,5 ep poll
#ok rfd =
#delay 1
0,5 ep poll
#ok -1 =
#delay 0.2

#endif

\ close the test epoll instance
rfd sys call close
wfd sys call close
ep teardown


\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
