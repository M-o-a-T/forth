#if undefined syscall
#end
#endif

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
  ." RDR A" cr
  rfd ep wait-read
  ." RDR B" cr
  rfd dbuf 6 call read 4 <> abort" RDR"
  ." RDR C" cr
  1 okr !
  ." RDR D" cr
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

\ we need that
ep teardown

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=

