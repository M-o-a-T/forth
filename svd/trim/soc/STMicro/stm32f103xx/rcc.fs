\ This file is auto-generated from ./svd/soc/data/STMicro//STM32F103xx.svd.
\ It contains registers for RCC
\ Generator: mapgen 0.1

bits only definitions


\ Reset and clock control
_reg voc: _RCC


\ CR : Clock control register
  &rg voc: _CR
        &bi item \ 0
    $0 constant HSION \ Internal High Speed clock enable
        &bi item \ 1
    $1 constant HSIRDY \ Internal High Speed clock ready flag
        &bf item \ 7:3
    $a3 constant HSITRIM \ Internal High Speed clock trimming
        &bf item \ 15:8
    $108 constant HSICAL \ Internal High Speed clock Calibration
        &bi item \ 16
    $10 constant HSEON \ External High Speed clock enable
        &bi item \ 17
    $11 constant HSERDY \ External High Speed clock ready flag
        &bi item \ 18
    $12 constant HSEBYP \ External High Speed clock Bypass
        &bi item \ 19
    $13 constant CSSON \ Clock Security System enable
        &bi item \ 24
    $18 constant PLLON \ PLL enable
        &bi item \ 25
    $19 constant PLLRDY \ PLL clock ready flag
        previous definitions
  _CR item

$0 offset: CR

\ CFGR : Clock configuration register (RCC_CFGR)
  &rg voc: _CFGR
        &bf item \ 1:0
    $40 constant SW \ System clock Switch
        &bf item \ 3:2
    $42 constant SWS \ System Clock Switch Status
        &bf item \ 7:4
    $84 constant HPRE \ AHB prescaler
        &bf item \ 10:8
    $68 constant PPRE1 \ APB Low speed prescaler (APB1)
        &bf item \ 13:11
    $6b constant PPRE2 \ APB High speed prescaler (APB2)
        &bf item \ 15:14
    $4e constant ADCPRE \ ADC prescaler
        &bi item \ 16
    $10 constant PLLSRC \ PLL entry clock source
        &bi item \ 17
    $11 constant PLLXTPRE \ HSE divider for PLL entry
        &bf item \ 21:18
    $92 constant PLLMUL \ PLL Multiplication Factor
        &bi item \ 22
    $16 constant OTGFSPRE \ USB OTG FS prescaler
        &bf item \ 26:24
    $78 constant MCO \ Microcontroller clock output
        previous definitions
  _CFGR item

$4 offset: CFGR

#if 0

\ CIR : Clock interrupt register (RCC_CIR)
  &rg voc: _CIR
        &bi item \ 0
    $0 constant LSIRDYF \ LSI Ready Interrupt flag
        &bi item \ 1
    $1 constant LSERDYF \ LSE Ready Interrupt flag
        &bi item \ 2
    $2 constant HSIRDYF \ HSI Ready Interrupt flag
        &bi item \ 3
    $3 constant HSERDYF \ HSE Ready Interrupt flag
        &bi item \ 4
    $4 constant PLLRDYF \ PLL Ready Interrupt flag
        &bi item \ 7
    $7 constant CSSF \ Clock Security System Interrupt flag
        &bi item \ 8
    $8 constant LSIRDYIE \ LSI Ready Interrupt Enable
        &bi item \ 9
    $9 constant LSERDYIE \ LSE Ready Interrupt Enable
        &bi item \ 10
    $a constant HSIRDYIE \ HSI Ready Interrupt Enable
        &bi item \ 11
    $b constant HSERDYIE \ HSE Ready Interrupt Enable
        &bi item \ 12
    $c constant PLLRDYIE \ PLL Ready Interrupt Enable
        &bi item \ 16
    $10 constant LSIRDYC \ LSI Ready Interrupt Clear
        &bi item \ 17
    $11 constant LSERDYC \ LSE Ready Interrupt Clear
        &bi item \ 18
    $12 constant HSIRDYC \ HSI Ready Interrupt Clear
        &bi item \ 19
    $13 constant HSERDYC \ HSE Ready Interrupt Clear
        &bi item \ 20
    $14 constant PLLRDYC \ PLL Ready Interrupt Clear
        &bi item \ 23
    $17 constant CSSC \ Clock security system interrupt clear
        previous definitions
  _CIR item

$8 offset: CIR

#endif


\ APB1 bits
  &rg voc: _APB1R
        &bi item \ 0
    $0 constant TIM2 \ Timer 2 reset
        &bi item \ 1
    $1 constant TIM3 \ Timer 3 reset
        &bi item \ 2
    $2 constant TIM4 \ Timer 4 reset
        &bi item \ 3
    $3 constant TIM5 \ Timer 5 reset
        &bi item \ 4
    $4 constant TIM6 \ Timer 6 reset
        &bi item \ 5
    $5 constant TIM7 \ Timer 7 reset
        &bi item \ 6
    $6 constant TIM12 \ Timer 12 reset
        &bi item \ 7
    $7 constant TIM13 \ Timer 13 reset
        &bi item \ 8
    $8 constant TIM14 \ Timer 14 reset
        &bi item \ 11
    $b constant WWDG \ Window watchdog reset
        &bi item \ 14
    $e constant SPI2 \ SPI2 reset
        &bi item \ 15
    $f constant SPI3 \ SPI3 reset
        &bi item \ 17
    $11 constant USART2 \ USART 2 reset
        &bi item \ 18
    $12 constant USART3 \ USART 3 reset
        &bi item \ 19
    $13 constant UART4 \ UART 4 reset
        &bi item \ 20
    $14 constant UART5 \ UART 5 reset
        &bi item \ 21
    $15 constant I2C1 \ I2C1 reset
        &bi item \ 22
    $16 constant I2C2 \ I2C2 reset
        &bi item \ 23
    $17 constant USB \ USB reset
        &bi item \ 25
    $19 constant CAN \ CAN reset
        &bi item \ 27
    $1b constant BKP \ Backup interface reset
        &bi item \ 28
    $1c constant PWR \ Power interface reset
        &bi item \ 29
    $1d constant DAC \ DAC interface reset
        previous definitions

\ APB2 bits
  &rg voc: _APB2R
        &bi item \ 0
    $0 constant AFIO \ Alternate function I/O reset
        &bi item \ 2
    $2 constant IOPA \ IO port A reset
        &bi item \ 3
    $3 constant IOPB \ IO port B reset
        &bi item \ 4
    $4 constant IOPC \ IO port C reset
        &bi item \ 5
    $5 constant IOPD \ IO port D reset
        &bi item \ 6
    $6 constant IOPE \ IO port E reset
        &bi item \ 7
    $7 constant IOPF \ IO port F reset
        &bi item \ 8
    $8 constant IOPG \ IO port G reset
        &bi item \ 9
    $9 constant ADC1 \ ADC 1 interface reset
        &bi item \ 10
    $a constant ADC2 \ ADC 2 interface reset
        &bi item \ 11
    $b constant TIM1 \ TIM1 timer reset
        &bi item \ 12
    $c constant SPI1 \ SPI 1 reset
        &bi item \ 13
    $d constant TIM8 \ TIM8 timer reset
        &bi item \ 14
    $e constant USART1 \ USART1 reset
        &bi item \ 15
    $f constant ADC3 \ ADC 3 interface reset
        &bi item \ 19
    $13 constant TIM9 \ TIM9 timer reset
        &bi item \ 20
    $14 constant TIM10 \ TIM10 timer reset
        &bi item \ 21
    $15 constant TIM11 \ TIM11 timer reset
        previous definitions

\ APB2RSTR : APB2 peripheral reset register (RCC_APB2RSTR)

  _APB2R item
$c offset: APB2RSTR

\ APB1RSTR : APB1 peripheral reset register (RCC_APB1RSTR)
  _APB1R item
$10 offset: APB1RSTR

\ AHBENR : AHB Peripheral Clock enable register (RCC_AHBENR)
  &rg voc: _AHBENR
        &bi item \ 0
    $0 constant DMA1EN \ DMA1 clock enable
        &bi item \ 1
    $1 constant DMA2EN \ DMA2 clock enable
        &bi item \ 2
    $2 constant SRAMEN \ SRAM interface clock enable
        &bi item \ 4
    $4 constant FLITFEN \ FLITF clock enable
        &bi item \ 6
    $6 constant CRCEN \ CRC clock enable
        &bi item \ 8
    $8 constant FSMCEN \ FSMC clock enable
        &bi item \ 10
    $a constant SDIOEN \ SDIO clock enable
        previous definitions
  _AHBENR item

$14 offset: AHBENR

\ APB2ENR : APB2 peripheral clock enable register (RCC_APB2ENR)
  _APB2R item
$18 offset: APB2ENR

\ APB1ENR : APB1 peripheral clock enable register (RCC_APB1ENR)
  _APB1R item
$1c offset: APB1ENR

\ BDCR : Backup domain control register (RCC_BDCR)
  &rg voc: _BDCR
        &bi item \ 0
    $0 constant LSEON \ External Low Speed oscillator enable
        &bi item \ 1
    $1 constant LSERDY \ External Low Speed oscillator ready
        &bi item \ 2
    $2 constant LSEBYP \ External Low Speed oscillator bypass
        &bf item \ 9:8
    $48 constant RTCSEL \ RTC clock source selection
        &bi item \ 15
    $f constant RTCEN \ RTC clock enable
        &bi item \ 16
    $10 constant BDRST \ Backup domain software reset
        previous definitions
  _BDCR item

$20 offset: BDCR

#if 0

\ CSR : Control/status register (RCC_CSR)
  &rg voc: _CSR
        &bi item \ 0
    $0 constant LSION \ Internal low speed oscillator enable
        &bi item \ 1
    $1 constant LSIRDY \ Internal low speed oscillator ready
        &bi item \ 24
    $18 constant RMVF \ Remove reset flag
        &bi item \ 26
    $1a constant PINR \ PIN reset flag
        &bi item \ 27
    $1b constant PORR \ POR/PDR reset flag
        &bi item \ 28
    $1c constant SFTR \ Software reset flag
        &bi item \ 29
    $1d constant IWDGR \ Independent watchdog reset flag
        &bi item \ 30
    $1e constant WWDGR \ Window watchdog reset flag
        &bi item \ 31
    $1f constant LPWRR \ Low-power reset flag
        previous definitions
  _CSR item

$24 offset: CSR

#endif

    previous definitions \ end RCC

  forth definitions

$40021000 _RCC port: RCC

  bits ignore

forth definitions
#ok depth 0=

\ This file was auto-generated by "mapgen", part of MoaT Forth.
\ It has been edited for APB1 and APB1 register name overlap,
\ and because nobody needs clock interrupts. (Probably.)
\ The generator states:
\ SPDX-License-Identifier: GPL-3.0-only
\ 
\ EOF

