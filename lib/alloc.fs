\
\ memory allocator.

forth only definitions

#if defined alloc
#end
#endif

#require class: lib/class.fs
#require d-list-head lib/linked-list.fs

voc: alloc

class: hdr
\ Memory element header.
__data
  var> hint field: flag
  var> hint field: sz
  aligned

  \ Intermission: get from the link field back to the header.
  d-list-item class: hdr-link
  dup constant \off
  hdr item
  : @ ( d-list-adr -- task )
  \ pretend that the list item stores a link to the task it's in
    __ \off - inline
  ;
  ;class

  \ free blocks only
  hdr-link field: link
__seal

2 base !
1110000000000111 constant *free*
1111010101011111 constant *used*
1110001111000111 constant *merged*
decimal

: msize  ( hdr -- count )
\ size of the block with this (allocated) header
\ get the header with ">alloc"
  __ sz @  __ hdr-link \off -
;

hdr item
: next-mem
  dup __ sz @ .. +
;

: merge-next  ( hdr -- )
   dup __ next-mem .. dup __ link remove  ( dbuf next-dbuf )   \ Off of free list
   over __ sz @ swap __ sz @ +  rot __ sz !     \ Increase size
;


hdr item
: merge-down  ( dbuf -- dbuf )
   begin
      dup __ next-mem flag @  *free*  =
   while
      dup __ merge-next
   repeat
;

;class

forth definitions
hdr item
: >alloc  ( adr -- dbuf )
\ given an address returned by "alloc" returns the block header
  hdr hdr-link @ .. ( dbuf )
  dup hdr flag @ case
    hdr *used* of  endof
    hdr *free* of
      true abort" already free"
    endof
    true abort" bad address"
  endcase
;


\
\ this collects methods of the memory allocator head.
\ 
\ There may be more than one of those, to avoid fragmentation.

alloc definitions

d-list-head class: pool

: (adj) ( xt link -- xt )
\ call xt with the task that contains "link"
  hdr hdr-link @ .. \ adjust link so that it addresses the task

  \ park the original xt on the stack during its execution
  \ for transparency
  swap >r 
  r@ execute
  r> swap
;

: each ( xt queue -- flag )
\ call xt with every job
  ['] (adj) swap
  __ each
  ( xt flag )
  nip
;

: link-free  ( dbuf pool -- )
  swap  ( pool dbuf )
  hdr *free*  over  hdr flag !  \ Set node status to "free"
#if-flag debug
  dup hdr link >setup
#endif
  hdr link ..                   \ get address of link block
  swap __ append                \ insert in list after head node
;

: add  ( len pool -- )
\ add a block of `len` bytes to this allocator
  compiletoram? -rot swap

  compiletoram
  align here swap ( mem? pool adr len )
  aligned dup allot ( mem? pool adr len )
  over hdr >setup
  over hdr link >setup
  hdr hdr-link \off -  \ subtract size of fake end-of-mem header
  over hdr sz ! ( mem? pool adr )
  swap __ link-free ( mem? )

  here  hdr hdr-link \off - \ dummy header at end
  0 over  hdr sz !
  hdr *used* swap  hdr flag !

  0= if compiletoflash then
;

: free  ( adr pool -- )
\ add the block at `adr` to the pool `mem`
   eint? -rot dint
   swap >alloc merge-down ..
   swap __ link-free
   if eint then
;

: \?free ( len hdr -- len 0 )
  hdr merge-down ..
  hdr sz @ max
  0
;

: ?free ( pool -- maxblk )
  eint? swap dint
  0 ['] \?free rot __ each
  drop
  swap if eint then
;

: add-if ( len free pool )
\ add len bytes if not at least free bytes
  tuck __ ?free < if
    2drop
  else
    __ add
  then
;

#if-flag alloc-fit
#error implement me

#else
: \scan ( len hdr -- len hdr|0 )
\ Return the first block that fits.
  hdr merge-down ..
  dup hdr sz @  ( len hdr hdrlen )
  2 pick < if  \ won't fit
    drop 0
  then
;

: _scan ( len pool -- len block )
  ['] \scan swap __ each
  dup 0= if .s over abort" pool full" then
;

#endif

: alloc ( len pool -- adr )
  eint? -rot dint
  swap
  hdr hdr-link \off +
  hdr u/i max
  aligned
  swap __ _scan  ( len block )
  dup hdr sz @ ( len best blen )
  rot - ( best len.over )
  dup hdr u/i 2* < if  \ check if the free space is big enough
    drop dup hdr link remove ( best )
    \ no: remove from free list
  else ( best len.over )
    \ yes: store len.over in the header and add a new block to its end
    over hdr sz @ ( best len.add len.both )
    over - >r ( best len.add |R: len )
    tuck over hdr sz ! ( len.add best )
    + r> over hdr sz ! ( best )
  then
  swap if eint then
  hdr *used* over hdr flag !
  hdr hdr-link \off +
;

;class


forth definitions

pool object: mem
\ generic

forth only


\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
