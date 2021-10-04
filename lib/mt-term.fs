\ multitask-emit:
\ smake sure output from multiple threads doesn't interleave

forth only definitions
task also

voc: term


\ Theory of operation:

\ this is the send buffer. It gets filled by a single task until a
\ linefeed or a timeout happens.
rc80 object: outbuf

\ this is the queue of tasks waiting to send things. A task adds itself to
\ it when it wants to send something, OUTDLY is not zero, and OUTTHIS is
\ not TASK THIS.
task %queue object: outq

0 variable outdly
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

task looped :task: outsend
  begin
    begin
      outbuf empty?
    not while
      begin
        oemit?
      not while
        yield
      repeat
      outbuf @ oemit
      3 outdly !
    repeat

    \ outbuf is empty. Count down?
    outdly @ 
    ?dup if
      1- outdly !
      yield
    else
      \ Nothing to do, so let somebody else do some work.
      0 outthis !
      outq one
      task stop
    then
  again
;

:init outsend start
  yield
;

: qemit? ( -- flag )
  -1 ;
: qemit ( char -- )
  begin
    outthis @ task this ..
  <> while
    outthis @ if
      \ some other task is writing.
      outq wait
    else
      task this .. outthis !
      3 outdly !
      outsend continue
    then
  repeat
  outbuf !

\ 10 = if \ linefeed. Let someone else get a go.
\   0 outthis !
\   9 outdly !
\   outq one
\   yield
\ then
;

\ :init
: emit-init
  ['] qemit? hook-emit? !
  ['] qemit hook-emit !
;
: emit-exit
  ['] oemit? hook-emit? !
  ['] oemit hook-emit !
;

forth only definitions

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
