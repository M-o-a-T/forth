\ multitask-emit:
\ smake sure output from multiple threads doesn't interleave

forth only definitions

#if-flag !multi
#error Multitask only
#endif

#if defined term
#end
#endif

#if defined syscall
#require poll lib/syscall.fs
#endif

#require rc80 lib/ring.fs
#require bits lib/bits.fs
#require gpio lib/gpio.fs

bits also
gpio also
#endif

#endif

forth only definitions
task also

voc: term

\ ****************
\      output
\ ****************

\ Theory of operation:

\ this is the send buffer. It gets filled by a single task until a
\ linefeed or a timeout happens.
rc80 object: outbuf

\ this is the queue of tasks waiting to send things. A task adds itself to
\ it when it wants to send something and OUTTHIS is not zero.
task %queue object: outq

0 variable outdly
#if-flag debug
0 variable outnum  \ #chars this task wrote
#endif
0 variable outthis

\ old emit hooks
: oemit? [ hook-emit? @ call, ] ;
: oemit  [ hook-emit  @ call, ] ;

\ This is the send loop. It has three states:
\ * wait: outthis is zero, outbuf is empty.
\   Sleep until woken up.
\ * send: outbuf is not empty.
\   Send until it is. Do not countdown
\ * 

0 variable npr

looped :task: outsend
  begin
    begin
      outbuf empty?
    not while
      1 poll wait-write
      outbuf s@  1 -rot sys call write  outbuf skip
      2 outdly !
    repeat

    \ outbuf is empty. Count down?
    outdly @ 
    ?dup if
      1- outdly !
      yield
    else
      \ Nothing to do, so let somebody else do some work.
      0 outthis !
      outq empty? if
        \ Nobody else is there, so go to sleep.
        task stop
      else
        outq one
        \ Give them a chance to start
        yield yield
      then
    then
  again
;

:init
  outsend start
  yield
;

: qemit? ( -- flag )
  -1 ;
: qemit ( char -- )
  1 npr +!
  begin
    outthis @ task this ..
  <> while
    outthis @ if
      \ some other task is writing. Delay.
      outq wait
    else
      \ our turn. Start the sender in case it's idle.
      task this .. outthis !
#if-flag debug
      0 outnum !
#endif
      1 outdly +!
      outsend continue
    then
  repeat
#if-flag debug
  dup
#endif
  outbuf !
#if-flag debug
  1 outnum +!
#endif

#if-flag debug
  10 =  outnum @ 200 > or
  if \ linefeed. Let someone else get a go.
    0 outdly !
    outq wait
  then
#endif
;

\ ***************
\      input
\ ***************

\ Operations: our input buffer is small because INTERPRET has its own and
\ will take the data.

rc16 object: inq

\ old input hooks
: okey? [ hook-key? @ call, ] ;
: okey  [ hook-key  @ call, ] ;

: qkey?
  inq empty? not
;
: qkey
  inq @
;

\ For packet input, this hook decides whether to process this byte.
\ If it returns non-zero the char is left on the stack
0 variable hook-packet  ( char -- flag )

#if defined syscall
\ For operating under Linux, don't read a single byte at a time.
256 constant insize
insize buffer: inbuf
#endif

looped :task: inrecv
  begin
    \ wait until ready
#[if] defined syscall
    \ Mecrisp-on-Linux hardcodes KEY? to return -1 and then blocks in KEY.
    \ Even if that wasn't multitask-unfriendly, reading a byte at a time is *slow*.
    0 poll wait-read
    0 inbuf insize sys call read
    \ zero is EOF. Ugh.
    ?dup 0= if
      cr ." EOF. Input closed." cr
      task stop  -9 abort
    then
    inbuf swap 0 do
      dup c@
#else
      begin okey? not while yield repeat
      okey
#endif
      dup ( buf ch ch ) hook-packet @ ?dup if execute else drop 0 then ( char flag )
      if
        drop
      else
        inq !
      then
#[if] defined syscall
      1+
    loop drop
#endif
  again
;
  

\ 

\ ***************
\      setup
\ ***************

\ :init

: emit-init
  ['] qemit? hook-emit? !
  ['] qemit hook-emit !
  ['] qkey? hook-key? !
  ['] qkey hook-key !
  task !multi
  inrecv start
  yield
;
: emit-exit
  ['] oemit? hook-emit? !
  ['] oemit hook-emit !
  ['] okey? hook-key? !
  ['] okey hook-key !
;

#if defined syscall
\ TODO doesn't yet work on real hardware
:init emit-init ;
#endif

forth only definitions

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
