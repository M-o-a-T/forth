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

#require rc16 lib/ring.fs
#require bits lib/bits.fs
#if undefined syscall
#require gpio lib/gpio.fs
#endif

bits also
#if undefined syscall
gpio also
#endif

#if undefined usart1
#include svd/fs/soc/STMicro/stm32f103xx/usart.fs
#endif
#if undefined rcc
#include svd/trim/soc/STMicro/stm32f103xx/rcc.fs
#endif
bits also
#if undefined nvic
#include soc/armcm3/nvic.fs
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
#if defined syscall
rc80
#else
rc16
#endif
object: outbuf

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

task looped :task: outsend
  begin
    begin
      outbuf empty?
    not while
#[if] defined syscall
      1 poll wait-write
      outbuf s@  1 -rot sys call write  outbuf skip
#else
      \ send the buffer. No need to loop on OEMIT?
      \ as it does that internally anyway.
      \ TODO use an interrupt instead.
      outbuf @ oemit
#endif
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
  dup  \ for the debug block below
#endif
  outbuf !
#if-flag debug
  1 outnum +!
#endif

#if-flag debug
  10 =  outnum @ 200 > or
  if
    \ linefeed. Let someone else get a go.
    \ This is debug only because we'll use the same code to send
    \ data packets and they mustn't be broken just because they contain \x0A.
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


looped :task: inrecv
  begin
    \ wait until ready
    \ Mecrisp-on-Linux hardcodes KEY? to return -1 and then blocks in KEY.
    \ Even if that wasn't multitask-unfriendly, reading a byte at a time
    \ is slow and wastes CPU.
    0 poll wait-read
    0 inbuf insize sys call read
    \ zero is EOF. Ugh.
    ?dup 0= if
      cr ." EOF. Input closed." cr
      task stop  -9 abort
    then
    inbuf swap 0 do
      dup c@
      dup ( buf ch ch ) hook-packet @ ?dup if execute else drop 0 then ( buf char flag )
      if
        drop
      else
        inq !
      then
      1+
    loop drop
  again
;

#else

: uart-irq-handler ( -- )  \ handle the USART receive interrupt
  begin
    USART1 SR RXNE @
  while
    USART1 DR @
    \ will drop input when there is no room left
    dup ( ch ch ) hook-packet @ ?dup if execute else drop 0 then ( char flag )
    if drop else
      inq full? if drop else inq ! then
    then
  repeat
;

: uart-irq-init ( -- )
\ initialise the USART1. Most is already done.
  \ pa9 mode af pp fast !
  \ pa10 mode float !
  \ 17 bit RCC-APB1ENR bis!  \ set USART2EN
  \ $138 USART1 BRR ! \ set baud rate divider for 115200 Baud at PCLK1=36MHz
  \ USART1 CR1 <% TE +! RE +! UE +! %>!
  ['] uart-irq-handler irq-usart1 !
  37 NVIC enabled +!  \ enable USART1 interrupt 37
  USART1 CR1 RXNEIE +!
;

#if undefined syscall
gpio also
#endif

#endif
\ !syscall

\ ***************
\      setup
\ ***************

\ :init

#if-flag debug
#require tasks debug/multitask.fs

forth only definitions
task also
bits also
term also

:task: dbg
  begin
    10 time seconds
    cr inq ?
    tasks
    task \int main ?
  again
;

#if-flag real
: demit? usart1 sr txe @ ;
: demit begin demit? until usart1 dr ! ;
#else
: \p ;
: demit? 1 ;
: demit hook-pause @ ['] \p hook-pause ! swap oemit hook-pause ! ;
#endif

: emit-debug
\ switch to debugging print words
\ Warning: when multitasking this can get messy
  ['] demit? hook-emit? !
  ['] demit hook-emit !
;

#endif

task definitions

: !multi
#if-flag debug
  \ dbg start
#endif
  outsend state @  task =idle >= if exit then
  outsend start
  yield
  ['] qemit? hook-emit? !
  ['] qemit hook-emit !
  ['] qkey? hook-key? !
  ['] qkey hook-key !
  task !multi
#[if] defined syscall
  inrecv start
#else
  uart-irq-init
#endif
;

: !single
#if-flag debug
  \ 1 dbg signal
#endif
#[if] defined syscall
  1 inrecv signal
#else
  USART1 CR1 RXNEIE -!
#endif
  1 outsend signal

  task !single
  ['] oemit? hook-emit? !
  ['] oemit hook-emit !
  ['] okey? hook-key? !
  ['] okey hook-key !
;

:init task !multi ;

forth only definitions

\ SPDX-License-Identifier: GPL-3.0-only
#ok depth 0=
