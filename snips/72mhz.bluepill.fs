\
\ This fudges the system clock to 72mhz on a blue pill.
\ Stopgap code so that sending stuff to the thing is reasonably fast.

\ use the RAM thing as a proxy
#if compiletoram?
: 72mhz ( -- )  cr            \ Increase 8Mhz RC clock to 72 MHz via 8MHz Xtal and PLL.
#else
:init
#endif
  $12 $40022000 !             \ two flash mem wait states
  1 16 lshift  $40021000 bis! \ set HSEON
  begin 1 17 lshift $40021000
       bit@ until             \ wait for HSERDY
  1 16 lshift                 \ HSE clock is 8 MHz Xtal source for PLL
  7 18 lshift or              \ PLL factor: 8 MHz * 9 = 72 MHz = HCLK
  4  8 lshift or              \ PCLK1 = HCLK/2 = 36MHz
  2 14 lshift or              \ ADCPRE = PCLK2/6
  2 or $40021004  !           \ PLL is the system clock
  1 24 lshift $40021000 bis!  \ set PLLON
  begin 1 25 lshift $40021000
       bit@ until             \ wait for PLLRDY

  630 $40013808 !  \ USART1 @ 115200 Baud
  \ 72000000 bits tick setclk

  \ Options:
  \ 78  $40013808 ! \ USART1 @  460800 Baud.
  \ 312 $40004408 !  \ USART2 @ 115200 Baud
  \ 78  $40004408 !   \ USART2 @ 460800 Baud.

  \ ." Clock is now 72 MHz. " cr
;
#if compiletoram?
72mhz
#endif
