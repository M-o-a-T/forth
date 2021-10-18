\ I/O pin primitives

\ #if defined gpio
\ #end
\ #endif

forth definitions only

#require bits

#if-flag debug
#require h.4 debug/bits.fs
forth only definitions
#endif

voc: gpio

voc: \int

: _io ( port# pin# -- pin )  \ combine port and pin into single int
  swap 8 lshift or  2-foldable ;

: io# ( pin -- u )  \ convert pin to bit position
  $0F and  1-foldable ;

;voc
\int also

voc: \reg

$40010800 constant &GP

: CRL        1-foldable ; \ reset $44444444 port Conf Register Low
: CRH  $04 + 1-foldable ; \ reset $44444444 port Conf Register High
: IDR  $08 + 1-foldable ; \ RO              Input Data Register
: ODR  $0C + 1-foldable ; \ reset 0         Output Data Register
: BSRR $10 + 1-foldable ; \ reset 0         port Bit Set/Reset Reg
: BRR  $14 + 1-foldable ; \ reset 0         port Bit Reset Register


#if 0
\ this is probably rather useless.

: <% 0
  postpone first
  \ This is a 0-foldable word, thus it always runs at compile time, thus
  \ FIRST adds our vocabulary.
  1-foldable ;

: +! ( pin -- )
  1 swap lshift or
  1-foldable
;

: -! ( pin -- )
  1 swap 16 + lshift or
  1-foldable
;

: ! ( flag pin -- )
  swap if +! else -! then
  2-foldable
;

: %>! ( addr bits )
  state @ if \ compiling
    postpone swap
    __ postpone !
  else
    swap __ !
  then
  postpone previous
  immediate
;
#endif


;voc

: base: ( "name" port# -- )  \ define a port base
  [ (' \reg literal, ] \voc (dovoc item  10 lshift  \reg &GP +  token [with] constant
  postpone ..
;


\int also definitions
\reg also

: mask ( pin -- u )  \ convert pin to bit mask
  io# bits bit  1-foldable ;
: port ( pin -- u )  \ convert pin to port number (A=0, B=1, etc)
  8 rshift  1-foldable ;
  \reg item
: iobase ( pin -- addr )  \ convert pin to GPIO base address
  $F00 and 2 lshift &GP +  1-foldable ;


: 't ( "name" -- )
  ('  \voc lfa>xt,flags swap , h, 
;
: tc, ( const -- )
  ,  $2000 h,
;

: &@  (   pin -- pin* addr )
  dup mask swap iobase IDR  1-foldable ;
: &c! (   pin -- pin* addr )
  dup mask swap iobase BRR  1-foldable ;
: &s! (   pin -- pin* addr )
  dup mask swap iobase BSRR 1-foldable ;
: &x! (   pin -- pin* addr )
  dup mask swap iobase ODR  1-foldable ;
: &!  ( f pin -- pin* addr )
  \ depends on the fact that BRR is 4 bytes below BSRR
  &s! rot 0= 4 and + ! 2-foldable ;
;voc
\ \int


\reg ignore
\int also
voc: pin
: @ ( pin -- f )
\ get pin value (0 or -1)
  &@  bit@ exit
  [ $1000 setflags 2 h,
    't &@
    't bit@
  ] ;

: -! ( pin -- )
\ clear pin (> low)
  &c! ! exit
  [ $1000 setflags 2 h,
    't &c!
    't !
  ] ;

: +! ( pin -- )
\ set pin (> high)
  &s! ! exit
  [ $1000 setflags 2 h,
    't &s!
    't !
  ] ;

: ^! ( pin -- )
\ toggle pin; not interrupt safe
  &x! xor! exit
  [ $1000 setflags 2 h,
    't &x!
    't xor!
  ] ;

: ! ( flag pin -- )
  swap if +! else -! then
  exit
  2-foldable
  [ $1000 setflags 7 h,
    't &s!
    't rot
    't 0=
    4 tc,
    't and
    't +
    't !
  ] ;

voc: _mode

: ! ( pin mode -- )  \ set the CNF and MODE bits for a pin
  swap dup  ( mode pin pin )
  \int iobase CRL over 8 and  shr  + >r ( mode pin |R: crX )
  7 and 2 lshift ( mode shift )
  $F over lshift not ( mode shift Xmask )
  r@  forth @  and ( mode shift Xold )
  -rot lshift or
  r>  forth !
;

#if-flag debug
: ? ( xx pin -- mode )
\ show mode. "Pxx MODE" leaves a placeholder for the new mode on the stack,
\ which we drop.
  drop dup iobase CRL over 8 and  shr  +  forth @ ( pin crX )
  swap 7 and 2 lshift rshift $F and  4 bits b.n
;
#endif

\ input modes

  _mode item
: ADC   %0000 + 1-foldable ; \ input, analog
  _mode item
: FLOAT %0100 + 1-foldable ; \ input, floating
  _mode item
: PULL  %1000 + 1-foldable ; \ input, pull-up/down

\ output modifiers

  _mode item
: SLOW    %01 + 1-foldable ; \ use after OMODE for 2 MHz iso 10 MHz drive
  _mode item
: FAST    %10 + 1-foldable ; \ use after OMODE for 50 MHz iso 10 MHz drive
  _mode item
: AF    %1000 + 1-foldable ; \ alternate function

\ output modes

  _mode item
: PP    %0001 + 1-foldable ; \ output, push-pull
  _mode item
: OD    %0101 + 1-foldable ; \ output, open drain
;voc \ _mode

  _mode item
0 constant mode


;voc \ pin

: pin: ( "name" port# pin# -- )  \ define a pin constant
  [ (' pin literal, ] \voc (dovoc item  \int _io token [with] constant
  postpone ..
;


#if 0
: io-modes! ( mode pin mask -- )  \ shorthand to config multiple pins of a port
  16 0 do
    i bits bit over and if
      >r  2dup ( mode pin mode pin R: mask ) $F bic i or io-mode!  r>
    then
  loop 2drop drop
;
#endif

#if-flag debug
pin definitions
\int also

: ? ( pin -- )  \ display readable GPIO registers associated with a pin
  cr
    ." PIN " dup io#  dup .  10 < if space then
   ." PORT " dup port [char] A + emit
  iobase ..
  ."   CRL " dup forth @       hex. 4 +
   ."  CRH " dup forth @       hex. 4 +
   ."  IDR " dup forth @  bits h.4  4 +
  ."   ODR " dup forth @  bits h.4  4 +
  drop
;

#endif
