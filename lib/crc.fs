forth definitions only

#if defined crc
#end
#endif

voc: crc

\ a bunch of CRC algorithms

\ Theory of operation.
\ Building CRCs by single-bit ops is too slow. Thus we build a table.
\ A table with 256 CRC-wide entries is too large (1k for CRC32). Thus
\ we make the table more narrow.
\
\ "depth" is how wide the table is. One interesting value is 4 bits, which
\ takes up 16 entries in our table and requires no special handling if our
\ CRC'd values happen to be 8 bits wide. There are others; the elements
\ MoaT feeds into its CRC-11 are however many wires there are in your bus.
\
\ "bits" is how wide the CRC is. Common are CRC-8/16/32 but there are
\ others; the CRC of MoaT is a CRC-11.
\
\ Usage for "crc": start with a CRC of zero. For each value: you XOR it
\ into the previous crc and run "crc" / "_crc" on it.

: calc ( val poly depth -- crcval )
\ bit-wise CRC calculation primitive
  -rot swap rot 0 do
    dup 1 and if
      shr over xor
    else
      shr
    then
  loop
  nip
;

: _crc ( depth poly val -- depth poly crcval )
\ bit-wise CRC calculation primitive for serial calculation.
\ (keeps the parameters on the stack)
  2 pick 0 do
    dup 1 and if
      shr over xor
    else
      shr
    then
  loop
;
  
\ Storage:
\ polynomial
\ #bits
\ table

: _t8:  ( depth poly || )
\ make a table for up-to-8-bit CRCs
  <builds
    over 8 lshift over or h,
    over 1 swap lshift 0 do
      i 1+ _crc 8 lshift >r i _crc r> or ,
    2 +loop
    2drop

  does> 2 +
;

: _t16:  ( depth poly || )
\ make a table for up-to-16-bit CRCs
  <builds
    here hex.
    dup h, over h,
    over 1 swap lshift 0 do i _crc h, loop
    2drop
  does> 4 + dup hex.
;

: _t32:  ( depth poly || )
\ make a table for up-to-32-bit CRCs
  <builds
    dup , over h,
    over 1 swap lshift 0 do i _crc , loop
    2drop
  does> 6 +
;


\ _table[(data ^ self.crc) & ((1<<self._bits)-1)] ^ (self.crc>>self._bits)
\ The code below looks somewhat complicated but in fact the Mecrisp register
\ allocator has a field day with it

: crc8 ( crc byte table -- crc )
\ calculate a CRC up to 8 bits wide
  rot dup >r -rot ( crc byte table |R: crc )
  dup 1- c@ ( crc byte table bits )
  r> over rshift >r
  1 swap lshift 1- >r ( crc byte table |R> self.crc>>self._bits mask )
  -rot xor r> and + c@ r> xor
;

: crc16 ( crc byte table -- crc )
\ calculate a CRC up to 16 bits wide
  rot dup >r -rot ( crc byte table |R: crc )
  dup 2- h@ ( crc byte table bits )
  r> over rshift >r ( crc byte table bits |R: crc>>bits )
  1 swap lshift 1- >r ( crc byte table |R> self.crc>>self._bits mask )
  -rot xor r> and 1 lshift + h@ r> xor
;

: crc32 ( crc byte table -- crc )
\ calculate a CRC up to 32 bits wide
  rot dup >r -rot ( crc byte table |R: crc )
  dup 2- h@ ( crc byte table bits )
  r> over rshift >r
  1 swap lshift 1- >r ( crc byte table |R> self.crc>>self._bits mask )
  -rot xor r> and 2 lshift + @ r> xor
;


forth definitions only

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
