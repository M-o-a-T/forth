\ basic MoaT bus connection test

#if-flag !multi
#echo requires multitasking
#end
#endif

#include test/reset.fs

#require unix lib/unixsock.fs

task also
sys also

0 variable rbuf
0 variable wbuf

:task: reader
  rbuf @
  dup sys nonblock
  begin
    dup poll wait-read
    dup rbuf 1 call read
    1 = if
      rbuf c@ ." BUS:" . cr
    else
      ." ER" cr exit
    then
  again
;

:task: writer
  wbuf @
  dup sys nonblock
  begin
    16 0 do
      5 time seconds
      i wbuf c!
      dup poll wait-write
      dup wbuf 1 call write 
      1 <> if ." EW" cr exit then
    loop
  again
;

:task: bustest
  1 time seconds
  s" /tmp/mbs" unix connect
  ( rsock wsock )
  2dup
  wbuf !
  rbuf !
  writer start
  reader start
  10 time minutes

  1 reader signal
  1 writer signal
  call close
  call close
  ." Bus test done" cr
;

bustest start
