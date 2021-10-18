forth only definitions

#if undefined nvic
#include soc/armcm3/nvic.fs
#endif

nvic irq definitions

\ copied from STM32F1 reference manual (RM0008), table 61 ff.

0 constant wwdg
1 constant pvd
2 constant tamper
3 constant rtc
4 constant flash
5 constant rcc
6 constant exti0
7 constant exti1
8 constant exti2
9 constant exti3
10 constant exti4
11 constant dma1_1
12 constant dma1_2
13 constant dma1_3
14 constant dma1_4
15 constant dma1_5
16 constant dma1_6
17 constant dma1_7
18 constant adc1_2
19 constant usb_hp
19 constant can1_tx
20 constant usb_lp
20 constant can1_rx0
21 constant can1_rx1
22 constant can1_sce
23 constant exti9_5
24 constant tim1_brk
24 constant tim9 \ second function on XL devices
25 constant tim1_up
25 constant tim10 \ second function on XL devices
26 constant tim1_trg_com
26 constant tim11 \ second function on XL devices
27 constant tim1_cc
28 constant tim2
29 constant tim3
30 constant tim4
31 constant 12c1_ev
32 constant 12c1_er
33 constant 12c2_ev
34 constant 12c2_er
35 constant spi1
36 constant spi2
37 constant usart1
38 constant usart2
39 constant usart3
40 constant exti15_10
41 constant rtcalarm
42 constant OTG_FS_WKUP
50 constant tim5
51 constant spi3
52 constant uart4
53 constant uart5
54 constant tim6
55 constant tim7
56 constant dma2_1
57 constant dma2_2
58 constant dma2_3
59 constant dma2_4
59 constant dma2_45 \ on XL devices; the table ends here
60 constant dma2_5
61 constant eth
62 constant eth_wkup
63 constant can2_tx
64 constant can2_rx0
65 constant can2_rx1
66 constant can2_sce
67 constant otf_fs

forth only definitions

#ok depth 0=
\ SPDX-License-Identifier: GPL-3.0-only
