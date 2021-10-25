
#include test/reset.fs

compiletoflash
#require crc
compiletoram

4 $583 _t16: t11_583_4

: crc11_583_4 ( crc byte -- byte )
  t11_583_4 crc16
  2-foldable
;

0
$1 crc11_583_4
$2 crc11_583_4
$3 crc11_583_4
$E crc11_583_4
$9 crc11_583_4
#ok #724 =

forth definitions only
                
\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=   
